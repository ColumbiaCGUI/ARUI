from difflib import SequenceMatcher
import json
from datetime import datetime
import os
import wave
import sys

# Define the directory to save conversation history
os.makedirs("logs", exist_ok=True)
filepath_prefix = str(datetime.now().strftime('%Y-%m-%d_%H-%M-%S-%f')[:-3])

# Read system prompt from a file name. Should be located in system_prompt folder
def load_prompt(file_name):
    try:
        with open("prompts/"+file_name, "r") as file:
            return file.read()
    except Exception as e:
        print(f"Error loading prompt file: '{file_name}' - {e}")
        return None

def similar(a, b):
    if b is None or a is None:
        return 1.0
    return SequenceMatcher(None, a, b).ratio()

def save_conv_to_file(filename, conv):
    """Save the conversation history to a file."""
    file_path = os.path.join("logs", f"{filepath_prefix}_{filename}.json")
    try:
        with open(file_path, 'w') as file:
            json.dump(conv, file, indent=4)
    except Exception as e:
        print(f"Error while saving conversation history: {e}")

def save_activity_to_file(filename, activity_deque):
    file_path = os.path.join("logs", f"{filepath_prefix}_{filename}_env")
    try:
        with open(file_path, 'w') as file:
            file.write(f"{datetime.now().strftime('%Y-%m-%d_%H-%M-%S-%f')[:-3] + " - " + str(activity_deque)}\n")
                
    except Exception as e:
        print(f"An error occurred while saving the deque to file: {e}")

def save_mic_data_to_wav(mic_data):
    """
    Saves mic_data directly to a WAV file.

    Args:
        mic_data (numpy.ndarray): The microphone audio data as a NumPy array.
        output_filename (str): The filename for the output WAV file.
    """

    output_filename = datetime.now().strftime('%Y-%m-%d_%H-%M-%S-%f')[:-3]+".wav"
    try:
        # Open the WAV file for writing
        with wave.open(output_filename, "wb") as wav_file:
            wav_file.setnchannels(1)  # Mono audio

            # Write the audio data to the WAV file
            wav_file.writeframes(mic_data.tobytes())

    except Exception as e:
        print(f"Error saving audio to WAV file: {e}")

def load_file(file_path):
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"The file at {file_path} does not exist.")
    
    try:
        with open(file_path, 'r') as file:
            return file.read()
    except IOError as e:
        raise IOError(f"An error occurred while reading the file: {e}")

def read_base64_text_file(file_path):
    try:
        with open(file_path, 'r') as file:
            base64_string = file.read().strip()
        return base64_string
    except Exception as e:
        print(f"An error occurred: {e}")
        return None


def json_to_dict(file_path):
    with open(file_path, 'r') as file:
        data = json.load(file)
    
    steps_dict = {}
    for index, step in enumerate(data["Steps"], start=1):
        steps_dict[f"{index}"] = {
            "StepDesc": step["StepDesc"],
            "ManualImage": step["manualImage"],
        }
    
    return steps_dict

def int_or_str(text):
    try:
        return int(text)
    except ValueError:
        return text
