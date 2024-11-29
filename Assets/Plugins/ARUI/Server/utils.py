from difflib import SequenceMatcher
import json
import os

# Define the directory to save conversation history
HISTORY_DIR = "conversation_history"
os.makedirs(HISTORY_DIR, exist_ok=True)

# Read system prompt from a file name. Should be located in system_prompt folder
def load_prompt(file_name):
    try:
        with open("prompts/"+file_name, "r") as file:
            return file.read()
    except Exception as e:
        print(f"Error loading prompt: {e}")
        return None

def similar(a, b):
    if b is None or a is None:
        return 1.0
    return SequenceMatcher(None, a, b).ratio()

def save_conv_to_file(session_id, conv):
    """Save the conversation history to a file."""
    file_path = os.path.join(HISTORY_DIR, f"{session_id}.json")
    try:
        with open(file_path, 'w') as file:
            json.dump(conv, file, indent=4)
    except Exception as e:
        print(f"Error while saving conversation history: {e}")

def int_or_str(text):
    try:
        return int(text)
    except ValueError:
        return text
