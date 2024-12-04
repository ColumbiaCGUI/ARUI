from openai import OpenAI
import utils.utils as utils
import uuid
import nodes.tasknode as tasknode
import copy

client = OpenAI()

# Load the system prompt
sys_directions = utils.load_prompt("system")
base_model = "gpt-4o-mini"
temperature = 0
max_tokens = 128

# Generate a unique session ID for every session
session_id = str(uuid.uuid4())
conversation_threads = None

def get_env(image_base64, prompt):
    """
    Function to send the prompt along with a Base64 encoded image to OpenAI API.
    """
    # Structure the OpenAI API message with the Base64 image in the "data URL" format.
    message = [{
        "role": "user",
        "content": [
            {
                "type": "text",
                "text": prompt
            },
            {
                "type": "image_url",
                "image_url": {
                    "url": f"data:image/jpeg;base64,{image_base64}"  # Base64 encoded image as per your example
                }
            }
        ]
    }]

    try:
        completion = client.chat.completions.create(
            model=base_model,
            messages=message,
            temperature=0.9,
        )
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None

    return completion.choices[0].message.content

def clear_conv_history():
    global conversation_threads
    conversation_threads = None

def continue_conv(sentence, image=None, observed_activity=None, task_status=None, observed_intent=None):
    global conversation_threads

    user_activity=""
    if observed_activity:   
        user_activity = f"Environment description: {observed_activity}\n\n"

    task_activity =""
    if task_status:
        task_activity =f"Task status: {task_status}\n\n"

    intent =""
    if observed_intent:
        intent =f"Observerd user intent: {observed_intent}\n\n"

    user_prompt = user_activity + task_activity + intent + "User utterance: "+ sentence + "\nYour concise response:"

    if conversation_threads is None:
        conversation_threads = {
        session_id: [{"role": "system", "content": sys_directions}]
    }
    
    prompt_prep = copy.deepcopy(conversation_threads[session_id])  # Creates a deep copy
    content = user_prompt
    if image:
        content = [ 
            {
                "type": "text",
                "text": user_prompt
            },
            {
                "type": "image_url",
                "image_url": {
                    "url": f"data:image/jpeg;base64,{image}"  # Base64 encoded image as per your example
                }
            }
        ]
        
    prompt_prep.append({ "role": "user", "content": user_prompt})
    response = query_LLM(prompt_prep)

    if utils.similar(response.lower(),"none") > 0.6:
        if image:
            user_content = content
        else:
            user_content = user_prompt
    
        response = "hmm"

        add_to_history(user_content, response)
            
    return response


def answer_question(sentence):
    messages=[{"role": "system", "content": "You are a helpful assistant."},
                  { "role": "user", "content": sentence}]
    
    return query_LLM(messages)


def query_LLM(messages):
    try:
        completion = client.chat.completions.create(
                model=base_model,
                messages=messages,
                temperature=temperature,
                max_tokens=max_tokens,
        )

        return completion.choices[0].message.content

    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        return None


def add_to_history(user_content=None, assistant=None):
    global conversation_threads

    if user_content:
        conversation_threads[session_id].append({"role": "user", "content": user_content}) 
        
    if assistant:
        conversation_threads[session_id].append({"role": "assistant", "content": assistant})
    
    utils.save_conv_to_file(session_id,conversation_threads[session_id])