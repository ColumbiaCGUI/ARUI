import json
import llm

intent_classes = {
    1: "Go to the next task",
    2: "Go to the previous task",
    3: "Repeat Instruction",
    4: "Asking for help in the current task",
    5: "Start a new task",
    6: "Asking a task related question",
    7: "Change to another task",
}

current_intent = None


def get_intent(sentence):
    prompt = f"Here is a list of user intent classes: {str(intent_classes)}" \
    "I will give you an user utterance. And you will give me the probability value for each user intent class." \
    "Answer NOT in text, ONLY in JSON format wihtout Backticks." \
    "User: "+sentence

    try:
        completion = llm.answer_question(prompt, False)
        
        # Extract the assistant's response
        response = completion.choices[0].message.content.strip()
        
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

