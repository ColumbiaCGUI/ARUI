from openai import OpenAI
import hl2utils
import utils
import uuid
import os

client = OpenAI()

# Load the system prompt
sys_directions = utils.load_prompt("system_directions")
base_model = "gpt-4o-mini"
temperature = 0
max_tokens = 128

# Generate a unique session ID for every session
session_id = str(uuid.uuid4())
conversation_threads = {
    session_id: [{"role": "system", "content": "You are a helpful assistant."+sys_directions}]
}

def get_activity(image):
    message = [{
                "role": "user",
                "content": [
                    {
                        "type": "text",
                        "text": "Describe in one sentence the current user activity in the given photo. If you can't see the user's hands, describe the object and the environment that you see and say that the user is inactive. In one sentence."
                    },
                    {
                        "type": "image_url",
                        "image_url": {
                            "url": image,
                        }
                    }
                ]
        }]

    try:
        completion = client.chat.completions.create(
                model=base_model,
                messages=message,
                temperature=temperature,
                max_tokens=max_tokens,
            )
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None
    
    return completion.choices[0].message.content
     
def query_LLM(sentence, add2conv, activity=None):
    global conversation_threads

    user_activity = f"last observed user activity: {activity}\n"
    
    if add2conv:
        messages=conversation_threads[session_id]
        messages.append({ "role": "user", "content": user_activity + "User utterance: "+ sentence})
    else:
        messages=[{"role": "system", "content": "You are a helpful assistant."+sys_directions},
                  { "role": "user", "content": "\n"+ user_activity + "User utterance: "+ sentence}]

    try:
        completion = client.chat.completions.create(
                model=base_model,
                messages=messages,
                temperature=temperature,
                max_tokens=max_tokens,
        )
        
        respone = completion.choices[0].message.content

        if add2conv and utils.similar(respone,"None")<0.8:
            add_to_history(sentence,respone)
        
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None

    return respone


def add_to_history(user=None, assistant=None):
    global conversation_threads

    if user:
        conversation_threads[session_id].append({"role": "user", "content": user})

    if assistant:
        conversation_threads[session_id].append({"role": "assistant", "content": assistant})
    
    utils.save_conv_to_file(session_id,conversation_threads[session_id])


def process_qa(sentence, activity=None):
    try:
        respone = query_LLM(sentence, True, activity)
        return respone
        
    except Exception as e:
        print(f"Error while parsing OpenAI response: {e}")
        return None