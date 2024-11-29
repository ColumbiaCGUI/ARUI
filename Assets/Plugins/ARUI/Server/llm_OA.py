from openai import OpenAI
import utils
import uuid

client = OpenAI()

# Load the system prompt
sys_directions = utils.load_prompt("system_directions")
base_model = "gpt-4o-mini"
temperature = 0
max_tokens = 128

# Generate a unique session ID for every session
session_id = str(uuid.uuid4())
conversation_threads = {
    session_id: [{"role": "system", "content": sys_directions}]
}

def get_activity(image, prompt):
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

def continue_conv(sentence, observed_activity):
    global conversation_threads

    user_activity=""
    if observed_activity:   
        user_activity = f"Environment description: {observed_activity}\n\n"

    user_prompt = user_activity + "User utterance: "+ sentence + "\n Your response:"

    messages=conversation_threads[session_id]
    messages.append({ "role": "user", "content": user_prompt})

    response = query_LLM(messages)
    print(response)

    if utils.similar(response,"None")<0.8:
        add_to_history(sentence,response)
    
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


def add_to_history(user=None, assistant=None):
    global conversation_threads

    if user:
        conversation_threads[session_id].append({"role": "user", "content": user})

    if assistant:
        conversation_threads[session_id].append({"role": "assistant", "content": assistant})
    
    utils.save_conv_to_file(session_id,conversation_threads[session_id])