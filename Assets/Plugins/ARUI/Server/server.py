import argparse
import time
import zmq
import queue
import sys
import sounddevice as sd
from threading import Thread, Lock
from vosk import Model, KaldiRecognizer
from openai import OpenAI
import utils.utils as utils

import utils.hl2utils as hl2utils

client = OpenAI()

q = queue.Queue()
context = zmq.Context()

# to avoid echo, check the string that was last published
last_publish = ""

# Load the system prompt
system_prompt = utils.load_system_prompt("system_prompt_spark_plug")

# Shared global variables
captured_sentence = ""
captured_sentence_lock = Lock()  # Lock to synchronize access to captured_sentence

def callback(indata, frames, time, status):
    if status:
        print(status, file=sys.stderr)
    q.put(bytes(indata))

def audio_processing(args):
    global captured_sentence

    try:
        if args.samplerate is None:
            device_info = sd.query_devices(args.device, "input")
            args.samplerate = int(device_info["default_samplerate"])

        model = Model(lang=args.model or "en-us")

        with sd.RawInputStream(
            samplerate=args.samplerate, blocksize=8000, device=args.device,
            dtype="int16", channels=1, callback=callback):
            print("#" * 80)
            print("Press Ctrl+C to stop the recording")
            print("#" * 80)

            rec = KaldiRecognizer(model, args.samplerate)

            while True:
                data = q.get()
                if rec.AcceptWaveform(data):
                    result = rec.Result()
                    # Find last and second-to-last double quotes
                    last = result.rfind('"')  # Index of last double quote
                    second_to_last = result[:last].rfind('"')  # Index of second-to-last double quote

                    if last != -1 and second_to_last != -1 and second_to_last < last:
                        # Extract substring between second-to-last and last quotes
                        parsed_sentence = result[second_to_last + 1:last]
                        print(f"Parsed sentence: {parsed_sentence}")
                        with captured_sentence_lock:
                            captured_sentence = parsed_sentence  # Update global variable


    except KeyboardInterrupt:
        print("\nAudio processing stopped")
    except Exception as e:
        print(f"Error in audio processing: {e}", file=sys.stderr)

def process_openai_request(sentence, socket):
    global last_publish

    if hl2utils.is_device_online(0.1):
        image = hl2utils.getPhoto()
    else:
        image = None
        
    try:
        print("Querying OpenAI with:", sentence)

        if image is None or len(image) <= 1:
            completion = client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[
                    {"role": "system", "content": "You are a helpful assistant."},
                        {
                            "role": "user",
                            "content": system_prompt + " " + sentence
                        }
                ],
                temperature=0.0,
                max_tokens=128,
            )

        else:
            completion = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
            {
                "role": "user",
                "content": [
                    {
                        "type": "text",
                        "text": system_prompt + ". Use the image to answer the question:4k " + sentence
                    },
                    {
                        "type": "image_url",
                        "image_url": {
                            "url": image,
                        }
                    }
                ]
            }
            ],
            temperature=0.0,
            max_tokens=128,
            )

        response = completion.choices[0].message.content
        print("Publishing response:", response)
        last_publish = response
        socket.send_string(response)
    except Exception as e:
        print(f"Error in OpenAI call: {e}")
        socket.send_string("Error processing request")

def zmq_server():
    global captured_sentence
    global last_publish

    socket = context.socket(zmq.PUB)
    socket.bind("tcp://*:5556")

    while True:
        try:
            with captured_sentence_lock:
                if captured_sentence:
                    if utils.similar(last_publish,captured_sentence) > 0.5:
                        local_sentence = None
                    else:
                        local_sentence = captured_sentence
                    
                    captured_sentence = ""  # Reset after reading
                else:
                    local_sentence = None

            if local_sentence:
                # Launch a thread to process the OpenAI request
                Thread(target=process_openai_request, args=(local_sentence, socket), daemon=True).start()
            else:
                socket.send_string("none")

            time.sleep(0.1)  # Prevent tight loop

        except Exception as e:
            print(f"Error in ZMQ server: {e}", file=sys.stderr)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-f", "--filename", type=str, metavar="FILENAME", help="Audio file to store recording to")
    parser.add_argument("-d", "--device", type=utils.int_or_str, help="Input device (numeric ID or substring)")
    parser.add_argument("-r", "--samplerate", type=int, help="Sampling rate")
    parser.add_argument("-m", "--model", type=str, help="Language model; e.g., en-us, fr, nl; default is en-us")
    args = parser.parse_args()

    # Start the audio processing thread
    audio_thread = Thread(target=audio_processing, args=(args,), daemon=True)
    audio_thread.start()

    # Start the ZMQ server
    zmq_server()
