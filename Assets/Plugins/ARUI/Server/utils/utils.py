from difflib import SequenceMatcher

# Read system prompt from a file name. Should be located in system_prompt folder
def load_system_prompt(file_name):
    try:
        with open("system_prompts/"+file_name, "r") as file:
            return file.read()
    except Exception as e:
        print(f"Error loading system prompt: {e}")
        return None

def similar(a, b):
    return SequenceMatcher(None, a, b).ratio()


def int_or_str(text):
    try:
        return int(text)
    except ValueError:
        return text
