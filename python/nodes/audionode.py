import asyncio
import sys
import numpy as np
from vosk import Model, KaldiRecognizer
import sounddevice as sd
import time

# Constants
SAMPLE_RATE = 16000
BLOCK_SIZE = 8000
ENERGY_THRESHOLD = 15000  # Minimum energy level to consider as valid audio for VAD
CORRELATION_THRESHOLD = 0.7  # Threshold for considering signals similar (0 to 1)

# Separate lock and shared variable
last_captured_sentence = ""
last_captured_sentence_lock = asyncio.Lock()  # Lock to synchronize access to captured_sentence

# Variable to track if the speaker is currently speaking
speaker_active = False
speaker_active_lock = asyncio.Lock()

audio_queue_mic = asyncio.Queue()

async def audio_callback_mic(indata, status):
    """Handles the microphone callback asynchronously."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    timestamp = time.time()  # Record the current timestamp
    await audio_queue_mic.put((speaker_active, timestamp, np.frombuffer(indata, dtype=np.int16)))
        

async def audio_callback_speaker(indata, status):
    """Handles the speaker callback asynchronously."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    timestamp = time.time()  # Record the current timestamp
    speaker_energy = np.sum(np.frombuffer(indata, dtype=np.int16) ** 2)
    async with speaker_active_lock:
        speaker_active = speaker_energy > ENERGY_THRESHOLD

async def monitor_speaker_stream():
    """Continuously reads the speaker stream and updates the speaker activity status."""
    try:
        # Get the current event loop
        loop = asyncio.get_running_loop()

        # Set device index for the speaker
        device_speaker_index = 2  # Replace with your speaker output index (for windows, stereo mix seems to work well)
        print(f"**Speaker device:** {sd.query_devices()[device_speaker_index]}")
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
        model = Model(lang="en-us")

        # Get the current event loop
        loop = asyncio.get_running_loop()

        # Set device index for the microphone
        device_microphone_index = None  # Replace with your microphone index
        print(f"**Mic device:** {sd.query_devices(sd.default.device[0])}")

        last_speaking=False

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

            while True:
                # Get data from the microphone queue
                mic_speaking, mic_timestamp, mic_data = await audio_queue_mic.get()
                
                if rec_mic.AcceptWaveform(mic_data.tobytes()):
                    result = rec_mic.Result()
                    print(result)

                if mic_speaking or last_speaking:
                    last_speaking = True
                    if not mic_speaking:
                        last_speaking = False
                    continue

                # Perform ASR on microphone data
                mic_sentence = ""
                if rec_mic.AcceptWaveform(mic_data.tobytes()):
                    result = rec_mic.Result()
                    print(result)
                    last = result.rfind('"')
                    second_to_last = result[:last].rfind('"')
                    if last != -1 and second_to_last != -1 and second_to_last < last:
                        mic_sentence = result[second_to_last + 1:last]
                
                # Update the captured sentence if a valid one is found
                if mic_sentence:
                    async with last_captured_sentence_lock:
                        last_captured_sentence = mic_sentence
                        print(f"**** Captured Sentence: {last_captured_sentence}")

    except KeyboardInterrupt:
        print("\nAudio processing stopped")
    except Exception as e:
        print(f"Error in processing microphone stream: {e}", file=sys.stderr)

async def start():
    """Main function to run the speaker monitoring and mic processing concurrently."""

    # Create the tasks for concurrent execution
    speaker_task = monitor_speaker_stream()
    mic_task = process_mic_stream()
    
    tasks = [speaker_task, mic_task]

    try:
        # Run tasks concurrently
        await asyncio.gather(*tasks)
    except asyncio.CancelledError:
        # Handle task cancellations
        print("Audio node tasks were cancelled. Cleaning up...")
    finally:
        # Cancel all tasks if they haven't finished already
        for task in tasks:
            task.cancel()
        # Gather tasks to ensure they are all awaited properly
        await asyncio.gather(*tasks, return_exceptions=True)
