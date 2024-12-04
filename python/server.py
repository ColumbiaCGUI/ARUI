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
    
    if tasknode.current_task is None:
        tasknode.initiate_task(2)

    while True:
        
        try:
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

                ## For debugging
                label = None
                observed_activity = await envnode.get_latest_activity()
                if observed_activity:
                    label = observed_activity['json']['hand_status']
                    if label is not None:
                        print(label)
                        await socket.send_string(label)
                continue

                # Handle task monitoring and response generation
                estimated_task_id = await asyncio.to_thread(tasknode.get_mentioned_task, last_AI_response)
                if estimated_task_id:
                    task_id = int(estimated_task_id[0])
                    print(f"Estimated Task: '{tasknode.task_classes[int(estimated_task_id[0])]}' with prob. {estimated_task_id[1]}")
                else:
                    task_id = 1
                
                response = None
                if task_id == 1 and tasknode.current_task is None:
                    print("Action: Task Clarification")
                    response = "First, please tell me what task you want to work on."

                elif task_id > 1 and tasknode.current_task is None:
                    print(f"Action: Start Task '{tasknode.task_classes[task_id]}'")
                    response = await asyncio.to_thread(tasknode.initiate_task, task_id)
                    llm.add_to_history(f"Let's tackle '{tasknode.task_classes[task_id]}'", response)

                else:
                    print("Action: QA")
                    label = None
                    observed_activity = await envnode.get_latest_activity()
                    if observed_activity:
                        label = observed_activity['label']
                    response = await asyncio.to_thread(llm.continue_conv, last_AI_response,label)

                if response:
                    print(f"Response: {response}")
                    await socket.send_string(response)
            else:
                await socket.send_string("0")  # Indicate no new input

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


async def main():
    # Create the tasks for different coroutines
    hl2node_task = hl2node.start()
    audionode_task = audionode.start()
    zmq_server_task = run_zmq_server()
    envnode_task = envnode.monitor_env()
    
    # Group them into a single task list
    tasks = [
        asyncio.create_task(hl2node_task),
        asyncio.create_task(audionode_task),
        asyncio.create_task(zmq_server_task),
        asyncio.create_task(envnode_task),
    ]

    try:
        # Run all tasks concurrently
        await asyncio.gather(*tasks)
    except asyncio.CancelledError:
        # Handle task cancellations
        print("Main task was cancelled. Cleaning up...")
    finally:
        # Properly cancel all running tasks
        for task in tasks:
            if not task.done():
                task.cancel()
                try:
                    await task
                except asyncio.CancelledError:
                    # Task was cancelled properly
                    pass
        print("All tasks have been cancelled and cleaned up.")

if __name__ == "__main__":
    try:
        # Run the main function in asyncio and handle KeyboardInterrupt gracefully
        asyncio.run(main())
    except KeyboardInterrupt:
        # If interrupted, this block will execute
        print("Program interrupted. Exiting gracefully.")