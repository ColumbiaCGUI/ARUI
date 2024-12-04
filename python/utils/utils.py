from difflib import SequenceMatcher
import json
from datetime import datetime
import os
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

def int_or_str(text):
    try:
        return int(text)
    except ValueError:
        return text
