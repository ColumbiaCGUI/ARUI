import asyncio
import zmq
import zmq.asyncio
import json
import llm
import utils.utils as utils
import nodes.hl2node as hl2node
import nodes.audionode as audionode
import nodes.envnode as envnode
import nodes.tasknode as tasknode
import nodes.intentnode as intentnode
import keyboard  # using module keyboard

# Async-compatible queue and ZMQ context
context = zmq.asyncio.Context()
subscriber_messages = asyncio.Queue()  # Queue to store incoming subscriber messages

last_AI_response = ""  # To avoid echo


async def run_zmq_server():
    global last_AI_response

    socket = context.socket(zmq.PUB)
    try:
        socket.bind("tcp://*:5556")
        print("ZMQ server bound to port 5556")
    except Exception as e:
        print(f"Failed to bind ZMQ socket: {e}")
        return

    last_left_pressed = False
    last_right_pressed = False
    last_reset = False

    await socket.send_string("400")

    while True:
        
        try:
            [current_left,current_right,current_reset] = check_keyboard(last_left_pressed,last_right_pressed,last_reset)
            if current_left:
                tasknode.go_to_previous()
                await socket.send_string("100-"+str(tasknode.current_taskID))
            elif current_right:
                tasknode.go_to_next()
                await socket.send_string("001-"+str(tasknode.current_taskID))
            elif current_reset:
                print("Initating dinosaur task")
                tasknode.initiate_task(2)
                socket.send_string("888:"+utils.load_file("data/dinosaur"))
            
            last_left_pressed = current_left 
            last_right_pressed = current_right
            last_reset = current_reset

            if tasknode.current_task == 1:
                print("Initating dinosaur task")
                tasknode.initiate_task(2)
                socket.send_string("888:"+utils.load_file("data/dinosaur"))

            # Access the shared sentence safely
            async with audionode.last_captured_sentence_lock:
                if audionode.last_captured_sentence:
                    last_AI_response = audionode.last_captured_sentence  # Use the captured sentence
                    audionode.last_captured_sentence = ""  # Reset after reading
                else:
                    last_AI_response = ""  # No sentence available

            if len(last_AI_response) > 1 and len(last_AI_response.strip().split(' ')) > 1:
                print(f"Processing user utterance: {last_AI_response}")
                await socket.send_string("400")

                response="---"
                needs_response = False
                # Handle task monitoring and response generation
                estimated_intent, prob = await asyncio.to_thread(intentnode.get_estimated_intent, last_AI_response)
                intent_string = str(intentnode.intent_classes[int(estimated_intent)])
                if 1==int(estimated_intent):
                    print("Detected Intent: Go to next step")
                    tasknode.go_to_next()
                    await socket.send_string("001-"+str(tasknode.current_taskID))
                elif 2==int(estimated_intent):
                    print("Detected Intent: Go to previous step")
                    tasknode.go_to_previous()
                    await socket.send_string("100-"+str(tasknode.current_taskID))
                elif 3==int(estimated_intent):
                    print("Needs material")
                    await socket.send_string("010")
                    response("You will find the manual reference close to me.")
                else:
                    needs_response = True

                if needs_response:
                    print("Prompting for QA")
                    label_activity = None
                    observed_activity = await envnode.get_latest_activity()
                    if observed_activity:
                        label_activity = observed_activity['label']

                    label_task = None
                    task_status = tasknode.get_task_status_prompt()
                    if task_status:
                        label_task = task_status

                    response = llm.continue_conv(last_AI_response,observed_activity=label_activity,task_status=label_task, observed_intent=intent_string)
                
                print(f"Estimated intent was: ({str(prob)}) '{intent_string}'")
                    
                if response:
                    print(f"Response: {response}")
                    await socket.send_string(response)
            else:
                await socket.send_string("---")  # Indicate no new input

            await asyncio.sleep(0.1)  # Prevent tight loop

        except Exception as e:
            print(f"Error in ZMQ server: {e}")


async def run_zmq_subscriber():
    socket = context.socket(zmq.SUB)
    try:
        socket.connect("tcp://localhost:5557")  # Connect to a publisher (adjust port if needed)
        socket.setsockopt_string(zmq.SUBSCRIBE, "")  # Subscribe to all topics
        print("ZMQ subscriber connected to port 5557")
    except Exception as e:
        print(f"Failed to connect ZMQ subscriber socket: {e}")
        return

    while True:
        try:
            message = await socket.recv_string()  # Receive message as string
            print(f"Received message: {message}")
            
            # Assuming the message is a JSON string, parse it
            try:
                message_json = json.loads(message)
                await subscriber_messages.put(message_json)  # Add the parsed JSON to the queue
            except json.JSONDecodeError:
                print(f"Failed to decode JSON message: {message}")

        except Exception as e:
            print(f"Error in ZMQ subscriber: {e}")

def check_keyboard(left_pressed, right_pressed,last_reset):
    new_left = False  
    if keyboard.is_pressed('left') and not left_pressed:
        new_left = True

    new_right = False
    if keyboard.is_pressed('right') and not right_pressed:
        new_right = True
              
    new_reset = False
    if keyboard.is_pressed('0') and not last_reset:
        new_reset = True

    return [new_left, new_right,new_reset]

async def main():
    # Create the tasks for different coroutines
    #hl2node_task = hl2node.start()
    audionode_task = audionode.start()
    zmq_server_task = run_zmq_server()
    #envnode_task = envnode.monitor_env()
    
    await asyncio.gather(
       audionode_task,
       zmq_server_task
    )

if __name__ == "__main__":
    try:
        # Run the main function in asyncio and handle KeyboardInterrupt gracefully
        asyncio.run(main())
    except KeyboardInterrupt:
        # If interrupted, this block will execute
        print("Program interrupted. Exiting gracefully.")