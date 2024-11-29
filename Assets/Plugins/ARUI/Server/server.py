import argparse
import asyncio
import zmq
import zmq.asyncio

import utils
import llm_OA
import audio
import activitymonitor
import taskmonitor

# Async-compatible queue and ZMQ context
context = zmq.asyncio.Context()

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

    while True:
        try:
            # Access the shared sentence safely
            async with audio.last_captured_sentence_lock:
                
                if audio.last_captured_sentence:
                    last_AI_response = audio.last_captured_sentence  # Use the captured sentence
                    audio.last_captured_sentence = ""  # Reset after reading
                else:
                    last_AI_response = ""  # No sentence available

            if len(last_AI_response)>1:
                print(f"Processing user utterance: {last_AI_response}")
                await socket.send_string("400")

                # Handle task monitoring and response generation
                estimated_task_id = await asyncio.to_thread(taskmonitor.get_mentioned_task, last_AI_response)
                print(f"Estimated Task: '{taskmonitor.task_classes[int(estimated_task_id[0])]}' with prob. {estimated_task_id[1]}")

                task_id = int(estimated_task_id[0])
                response = None
                if task_id == 1 and taskmonitor.current_task is None:
                    print("Action: Task Clarification")
                    response = "What task can I help you with?"
                elif task_id > 1 and taskmonitor.current_task is None:
                    print(f"Action: Start Task '{taskmonitor.task_classes[task_id]}'")
                    response = await asyncio.to_thread(taskmonitor.initiate_task, task_id)
                    llm_OA.add_to_history(f"Let's work on '{taskmonitor.task_classes[task_id]}'", response)
                else:
                    print("Action: QA")
                    response = await asyncio.to_thread(llm_OA.process_qa, last_AI_response)

                if response:
                    print(f"Response: {response}")
                    await socket.send_string(response)
            else:
                await socket.send_string("0")  # Indicate no new input

            await asyncio.sleep(0.1)  # Prevent tight loop

        except Exception as e:
            print(f"Error in ZMQ server: {e}")


async def main(args):
    # Launch audio processing, ZMQ server, and activity monitor concurrently
    await asyncio.gather(
        audio.audio_processing(args),
        run_zmq_server(),
        activitymonitor.monitor_user_activity(),
    )


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-f", "--filename", type=str, metavar="FILENAME", help="Audio file to store recording to")
    parser.add_argument("-d", "--device", type=utils.int_or_str, default=None, help="Input device (numeric ID or substring)")
    parser.add_argument("-r", "--samplerate", type=int, default=16000, help="Sampling rate (default: 16000)")
    parser.add_argument("-m", "--model", type=str, default="en-us", help="Language model; default is en-us")
    args = parser.parse_args()

    # Run the asyncio main loop
    asyncio.run(main(args))
