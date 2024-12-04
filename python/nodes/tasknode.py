import json
import llm
import utils.utils as utils

task_classes = {
    1: "None",
    2: "dinosaur"
}

current_task = None
current_instructions = None

def initiate_task(task_id):
    global current_task
    global current_instructions

    current_instructions = utils.load_prompt("Assembly/"+task_classes[task_id])
    current_task = task_id

    prompt = f"Task instructions for '{task_classes[task_id]}': {str(current_instructions)}" \
             f"List in one sentence what objects I need to do this task."

    try:
        response = llm.answer_question(prompt)
        return response
        
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None
    
def estimate_task_progress():
    prompt = f"Here is a list of my: {str(task_classes)} " \
    "I will give you an user utterance. And you will give me the probability value for each user class. " \
    "Example - if the class is 'Filter change' and the user says 'I want to change the filter', the probability for this class should be 1. "\
    "Reserve the class 'None' for the case that the utterance is not related to any of the acceptable tasks. " \
    "Answer NOT in text, ONLY in JSON format without Backticks. " \
    "User: "+sentence
    

def get_mentioned_task(sentence):
    prompt = f"Here is an acceptable list of tasks classes for motor maintenance: {str(task_classes)} " \
    "I will give you an user utterance. And you will give me the probability value for each user class. " \
    "Example - if the class is 'Filter change' and the user says 'I want to change the filter', the probability for this class should be 1. "\
    "Reserve the class 'None' for the case that the utterance is not related to any of the acceptable tasks. " \
    "Answer NOT in text, ONLY in JSON format without Backticks. " \
    "User: "+sentence

    try:
        response = llm.answer_question(prompt)
        
        # Check if the response is not empty
        if response==None:
            return None
        
        # Attempt to parse the response as JSON
        task_probabilities = json.loads(response)
        
        # Find the intent ID with the highest probability
        highest_task = max(task_probabilities, key=task_probabilities.get)
        return [highest_task, task_probabilities[highest_task]]
    
    except json.JSONDecodeError as jde:
        print(f"JSON Decode Error: {jde} - Response was: {response}")
        return None
    
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None