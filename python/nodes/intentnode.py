import json
import llm
import utils.utils as utils

intent_classes = {
    1: "Go to the next task",
    2: "Go to the previous task",
    3: "Other"
}

few_shot_examples = utils.load_prompt("intent")

def get_estimated_intent(sentence):

    examples = ""
    if few_shot_examples:
        examples = few_shot_examples

    prompt = examples + f"Here is a list of user intent classes: {str(intent_classes)}" \
    "I will give you an user utterance. And you will give me the probability value for each user intent class. The order matters. When you go through the list start with the lowest index. the higher index are more fallbacks." \
    "Answer NOT in text, ONLY in JSON format wihtout Backticks." \
    "User: "+sentence

    try:
        response = llm.answer_question(prompt)
        
        # Check if the response is not empty
        if not response:
            raise ValueError("Received an empty response from the LLM.")
        
        # Attempt to parse the response as JSON
        intent_probabilities = json.loads(response)
        
        # Find the intent ID with the highest probability
        highest_intent = max(intent_probabilities, key=intent_probabilities.get)
        return [highest_intent, intent_probabilities[highest_intent]]
    except json.JSONDecodeError as jde:
        print(f"JSON Decode Error: {jde} - Response was: {response}")
        return None
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None

