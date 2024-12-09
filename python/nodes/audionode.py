import os
import sys
import wave
import threading
import queue
import time
import numpy as np
from vosk import Model, KaldiRecognizer
import utils.utils as utils
import sounddevice as sd

######## Constants
SAMPLE_RATE = 16000
BLOCK_SIZE = 8000
ENERGY_THRESHOLD = 15000  # Minimum energy level to consider valid audio for VAD
# Set device indices for speaker and microphone
device_speaker_index = 3  # Replace with your speaker output index 
device_microphone_index = None  # Replace with your microphone input index
######## 

# Directory to save recordings
RECORDINGS_DIR = "recordings"
os.makedirs(RECORDINGS_DIR, exist_ok=True)

# Shared resources
last_captured_sentence_lock = threading.Lock()  # Lock to synchronize access to last_captured_sentence

speaker_active = False
speaker_active_lock = threading.Lock()  # Lock to synchronize access to speaker_active

audio_queue_mic = queue.Queue()

def audio_callback_mic(indata, frames, time_info, status):
    """Handles the microphone callback."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    timestamp = time.time()
    audio_queue_mic.put((speaker_active, timestamp, bytes(indata)))


def audio_callback_speaker(indata, frames, time, status):
    """Handles the speaker callback."""
    global speaker_active

    if status:
        print(status, file=sys.stderr)
    speaker_energy = np.sum(np.frombuffer(indata, dtype=np.int16) ** 2)
    with speaker_active_lock:
        speaker_active = speaker_energy > ENERGY_THRESHOLD


def monitor_speaker_stream():
    """Continuously reads the speaker stream and updates the speaker activity status."""
    global device_speaker_index

    try:
        print(f"Selected speaker device: {sd.query_devices()[device_speaker_index]['name']}")
        with sd.InputStream(
            samplerate=SAMPLE_RATE,
            blocksize=BLOCK_SIZE,
            device=device_speaker_index,
            dtype="int16",
            channels=2,
            callback=audio_callback_speaker,
        ):
            while True:
                time.sleep(0.01)  # Slight delay to prevent high CPU usage
    except Exception as e:
        print(f"Error in monitoring speaker stream: {e}", file=sys.stderr)


import re

# Define the forbidden phrases
forbidden_phrases = [r'\bfollow\b', r'\bunfollow\b', r'\bon follow\b', r'\band follow\b', r'\bthe follow\b']


def process_mic_stream(server_callback):
    """Processes the microphone stream, applying VAD and ASR."""
    try:
        print(f"Selected mic device: {sd.query_devices()[sd.default.device[0]]['name']}")
        
        model = Model(model_name="vosk-model-en-us-0.22-lgraph")

        last_speaking=False
        rec_id = 0
        current_audio = bytearray()

        with sd.InputStream(
            samplerate=SAMPLE_RATE,
            blocksize=BLOCK_SIZE,
            device=device_microphone_index,
            dtype="int16",
            channels=1,
            callback=audio_callback_mic,
        ):
            print("#" * 80)
            print("Press Ctrl+C to stop the recording")
            print("#" * 80)

            rec_mic = KaldiRecognizer(model, SAMPLE_RATE)

            while True:
                mic_speaking, _, mic_data = audio_queue_mic.get()

                if mic_speaking or last_speaking:
                    last_speaking = True
                    if not mic_speaking:
                        last_speaking = False
                    continue

                current_audio.extend(mic_data)

                temp = parse_utterance(rec_mic.PartialResult())
                if len(temp.strip().split(" ")) > 1:
                    print(parse_utterance(rec_mic.PartialResult()))
                    
                mic_sentence = ""
                if rec_mic.AcceptWaveform(mic_data):
                    final_result = parse_utterance(rec_mic.Result())
                    if len(final_result.strip().split(" ")) > 1 and "follow" not in final_result.lower() and "unfollow" not in final_result.lower() and "on follow" not in final_result.lower() and "and follow" not in final_result.lower() and "the follow" not in final_result.lower():
                        mic_sentence = final_result
                        if mic_sentence.lower().startswith("the "):
                            mic_sentence = mic_sentence[4:]  # Remove the first 4 characters (length of "the ")

                if len(mic_sentence.strip().split(" ")) > 1:
                    with last_captured_sentence_lock:
                        server_callback(mic_sentence)
                    print(f"**** Captured Sentence: {mic_sentence}")

                    rec_id += 1
                    filename = os.path.join(RECORDINGS_DIR, f"utterance_{rec_id}.wav")
                    with wave.open(filename, "wb") as wf:
                        wf.setnchannels(1)
                        wf.setsampwidth(2)
                        wf.setframerate(SAMPLE_RATE)
                        wf.writeframes(current_audio)

                    current_audio = bytearray()

    except KeyboardInterrupt:
        print("\nAudio processing stopped")
    except Exception as e:
        print(f"Error in processing microphone stream: {e}", file=sys.stderr)


def parse_utterance(utt):
    """Extracts the recognized utterance from the Vosk output."""
    last = utt.rfind('"')
    second_to_last = utt[:last].rfind('"')
    if last != -1 and second_to_last != -1 and second_to_last < last:
        return utt[second_to_last + 1 : last]
    return ""


def start(suppress_speaker_input=True, callback=None):
    """Main function to run the speaker monitoring and mic processing in threads."""
    threads = []

    if suppress_speaker_input:
        speaker_thread = threading.Thread(target=monitor_speaker_stream, daemon=True)
        threads.append(speaker_thread)
        speaker_thread.start()

    mic_thread = threading.Thread(target=process_mic_stream, args=(callback,), daemon=True)
    threads.append(mic_thread)
    mic_thread.start()

    try:
        while True:
            time.sleep(0.01)
    except KeyboardInterrupt:
        print("\nProgram interrupted. Exiting gracefully.")
        sys.exit()
