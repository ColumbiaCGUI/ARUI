import zmq
import threading
import asyncio
import llm
import keyboard
import utils.utils as utils
import nodes.hl2node as hl2node
import nodes.audionode as audionode
import nodes.envnode as envnode
import nodes.tasknode as tasknode
import nodes.intentnode as intentnode

# ZMQ Context
context = zmq.Context()

# Shared state for audio data
audio_data = {"last_captured_sentence": ""}
audio_data_lock = threading.Lock()

async def process_user_utterance(socket):
    """Process the user utterance asynchronously."""
    global audio_data
    user_utterance = ""
    with audio_data_lock:
        user_utterance = audio_data["last_captured_sentence"]
        audio_data["last_captured_sentence"] = ""

    if len(user_utterance) > 1 and len(user_utterance.strip().split(" ")) > 1:
        print(f"Processing user utterance: {user_utterance}")
        socket.send_string("400")

        response = "---"
        needs_response = False

        # Estimate intent in a thread-safe way
        [estimated_intent,prob] = intentnode.get_estimated_intent(user_utterance)
        if estimated_intent is None:
            intent_string = ""
            estimated_intent = 99
            prob = 0
        else:
            intent_string = str(intentnode.intent_classes[int(estimated_intent)])

        if 1 == int(estimated_intent):
            print("Detected Intent: Go to next step")
            tasknode.go_to_next()
            socket.send_string("001-" + str(tasknode.current_taskID))
        elif 2 == int(estimated_intent):
            print("Detected Intent: Go to previous step")
            tasknode.go_to_previous()
            socket.send_string("100-" + str(tasknode.current_taskID))
        elif 3 == int(estimated_intent):
            print("Needs material")
            socket.send_string("010")
            response = "You will find the manual reference close to me."
        else:
            needs_response = True

        if needs_response:
            print("Prompting for QA")
            label_activity = None
            observed_activity = envnode.get_latest_activity()
            if observed_activity:
                label_activity = observed_activity["label"]

            label_task = None
            task_status = tasknode.get_task_status_prompt()
            if task_status:
                label_task = task_status

            response = llm.continue_conv(
                user_utterance,
                observed_activity=label_activity,
                task_status=label_task,
                observed_intent=intent_string,
            )

        print(f"Estimated intent was: ({str(prob)}) '{intent_string}'")

        if response:
            print(f"Response: {response}")
            socket.send_string(response)
    else:
        socket.send_string("---")  # Indicate no new input

async def main_loop():
    """Main asyncio loop."""
    socket = context.socket(zmq.PUB)
    try:
        socket.bind("tcp://*:5556")
        print("ZMQ server bound to port 5556")
    except Exception as e:
        print(f"Failed to bind ZMQ socket: {e}")
        return
    
    tasknode.initiate_task(2)
    socket.send_string("888:" + utils.load_file("data/dinosaur"))

    last_left_pressed = False
    last_right_pressed = False
    last_reset = False

    while True:
        try:
            [current_left,current_right,current_reset] = check_keyboard(last_left_pressed,last_right_pressed,last_reset)
            if current_left:
                tasknode.go_to_previous()
                socket.send_string("100-"+str(tasknode.current_taskID))
                await asyncio.sleep(0.2)
            elif current_right:
                tasknode.go_to_next()
                socket.send_string("001-"+str(tasknode.current_taskID))
                await asyncio.sleep(0.3)
            elif current_reset:
                print("Initating dinosaur task")
                tasknode.initiate_task(2)
                socket.send_string("888:"+utils.load_file("data/dinosaur"))
                await asyncio.sleep(0.3)
            
            last_left_pressed = current_left 
            last_right_pressed = current_right
            last_reset = current_reset

            await process_user_utterance(socket)

            await asyncio.sleep(0.01)  # Prevent tight loop

        except Exception as e:
            print(f"Error in main loop: {e}")

def audio_callback(captured_sentence):
    """Callback to handle audio data from audionode."""
    with audio_data_lock:
        audio_data["last_captured_sentence"] = captured_sentence
        print(audio_data["last_captured_sentence"])

def start_audionode_thread():
    """Starts the audionode in a separate thread."""
    audio_thread = threading.Thread(target=audionode.start, args=(True, audio_callback), daemon=True)
    audio_thread.start()

def check_keyboard(left_pressed, right_pressed, last_reset):
    new_left = False  
    if keyboard.is_pressed('left') and not left_pressed:
        new_left = True

    new_right = False
    if keyboard.is_pressed('right') and not right_pressed:
        new_right = True
              
    new_reset = False
    if keyboard.is_pressed('0') and not last_reset:
        new_reset = True

    return [new_left, new_right, new_reset]

if __name__ == "__main__":
    start_audionode_thread()  # Start the audionode in a separate thread
    try:
        asyncio.run(main_loop())
    except KeyboardInterrupt:
        print("Server interrupted. Shutting down...")
