import asyncio
import os
import sys
import wave
import numpy as np
from vosk import Model, KaldiRecognizer
import utils.utils as utils
import sounddevice as sd
import time

# Constants
SAMPLE_RATE = 16000
BLOCK_SIZE = 8000
ENERGY_THRESHOLD = 15000  # Minimum energy level to consider as valid audio for VAD

# Directory to save recordings
RECORDINGS_DIR = "recordings"
os.makedirs(RECORDINGS_DIR, exist_ok=True)

# Separate lock and shared variable
last_captured_sentence = ""
last_captured_sentence_lock = asyncio.Lock()  # Lock to synchronize access to captured_sentence

last_captured_sentence_wav = None
last_captured_sentence_wav = asyncio.Lock()  # Lock to synchronize access to captured_sentence

# Variable to track if the speaker is currently speaking
speaker_active = False
speaker_active_lock = asyncio.Lock()

audio_queue_mic = asyncio.Queue()

# Set device index for the speaker
device_speaker_index = 3  # Replace with your speaker output index (for windows, stereo mix seems to work well)
device_microphone_index = None  # Replace with your microphone index

async def audio_callback_mic(indata, status):
    """Handles the microphone callback asynchronously."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    timestamp = time.time()  # Record the current timestamp
    await audio_queue_mic.put((speaker_active, timestamp, bytes(indata)))
        

async def audio_callback_speaker(indata, status):
    """Handles the speaker callback asynchronously."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    speaker_energy = np.sum(np.frombuffer(indata, dtype=np.int16) ** 2)
    async with speaker_active_lock:
        speaker_active = speaker_energy > ENERGY_THRESHOLD

async def monitor_speaker_stream():
    """Continuously reads the speaker stream and updates the speaker activity status."""
    global device_speaker_index

    try:
        # Get the current event loop
        loop = asyncio.get_running_loop()

        print(f"Selected speaker device: {sd.query_devices()[device_speaker_index]['name']}")
        with sd.InputStream(
            samplerate=SAMPLE_RATE,
            blocksize=BLOCK_SIZE,
            device=device_speaker_index,
            dtype="int16",
            channels=2,
            callback=lambda indata, frames, time, status: asyncio.run_coroutine_threadsafe(
                audio_callback_speaker(indata, status), loop
            )
        ):
            while True:
                await asyncio.sleep(0.1)  # Slight delay to prevent high CPU usage
    except Exception as e:
        print(f"Error in monitoring speaker stream: {e}", file=sys.stderr)

async def process_mic_stream():
    """Processes the microphone stream, applying VAD and ASR, and outputs only microphone input that is not from the speaker."""
    global last_captured_sentence

    try:
        print(f"Selected mic device: {sd.query_devices(sd.default.device[0])['name']}")

        print("Initiating mic stream, VAD and ASR..")
        model = Model(model_name="vosk-model-en-us-0.22")

        # Get the current event loop
        loop = asyncio.get_running_loop()

        last_speaking=False
        rec_id = 0
        current_audio = bytearray()  # To store the current audio data for recording

        with sd.InputStream(
            samplerate=SAMPLE_RATE,
            blocksize=BLOCK_SIZE,
            device=device_microphone_index,
            dtype="int16",
            channels=1,
            callback=lambda indata, frames, time, status: asyncio.run_coroutine_threadsafe(
                audio_callback_mic(indata, status), loop
            )
        ):
            print("#" * 80)
            print("Press Ctrl+C to stop the recording")
            print("#" * 80)

            # Microphone recognizer
            rec_mic = KaldiRecognizer(model, SAMPLE_RATE)

            # Initialize variables to track partial results
            last_partial_result = ""
            partial_repeat_count = 0

            while True:
                # Get data from the microphone queue
                mic_speaking, _, mic_data = await audio_queue_mic.get()
                
                # Append audio data to current recording
                current_audio.extend(mic_data)
                
                if mic_speaking or last_speaking:
                    last_speaking = True
                    if not mic_speaking:
                        last_speaking = False
                    continue


                partial_result = parse_utterance(rec_mic.PartialResult())
                print(partial_result)

                # Perform ASR on microphone data
                mic_sentence = ""
                if rec_mic.AcceptWaveform(mic_data):
                    final_result = parse_utterance(rec_mic.Result())
                    # only accept results that haven't been used yet
                    if len(final_result.strip().split(' ')) > 4 and utils.similar(final_result, last_captured_sentence) < 0.5:
                        mic_sentence = final_result
                        partial_repeat_count = 0
                else:
                    # Check if partial result matches the last one
                    if len(partial_result.strip().split(' ')) > 4 and utils.similar(partial_result, last_partial_result) > 0.9:
                        partial_repeat_count += 1
                    else:
                        partial_repeat_count = 0  # Reset counter if different
                    
                    last_partial_result = partial_result

                    # Accept as final result if the same partial result is repeated three times
                    if partial_repeat_count >= 4:
                        mic_sentence = partial_result
                        partial_repeat_count = 0 
                        last_partial_result = ""
            
                # Update the captured sentence if a valid one is found
                if mic_sentence:
                    async with last_captured_sentence_lock:
                        last_captured_sentence = mic_sentence
                        print(f"**** Captured Sentence: {last_captured_sentence}")

                    # Save the audio to a file
                    rec_id += 1
                    filename = os.path.join(RECORDINGS_DIR, f"utterance_{rec_id}.wav")
                    with wave.open(filename, 'wb') as wf:
                        wf.setnchannels(1)  # Mono
                        wf.setsampwidth(2)  # 16-bit audio
                        wf.setframerate(SAMPLE_RATE)
                        wf.writeframes(current_audio)

                    # Reset the audio buffer
                    current_audio = bytearray()

    except KeyboardInterrupt:
        print("\nAudio processing stopped")
    except Exception as e:
        print(f"Error in processing microphone stream: {e}", file=sys.stderr)

def parse_utterance(utt):
    last = utt.rfind('"')
    second_to_last = utt[:last].rfind('"')
    if last != -1 and second_to_last != -1 and second_to_last < last:
        return utt[second_to_last + 1:last]
    return ""

async def start(surpress_speaker_input=True):
    """Main function to run the speaker monitoring and mic processing concurrently."""

    tasks =[]
    if surpress_speaker_input:
        speaker_task = monitor_speaker_stream()
        asyncio.create_task(speaker_task)
        tasks.append(speaker_task)

    mic_task = process_mic_stream()
    asyncio.create_task(mic_task)
    tasks.append(mic_task)

    if surpress_speaker_input:
        await asyncio.gather(
            mic_task,
            speaker_task
        )
    else:
        await mic_task
   
