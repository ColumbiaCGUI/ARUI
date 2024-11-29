import asyncio
import sys
from vosk import Model, KaldiRecognizer
import sounddevice as sd

audio_queue = asyncio.Queue()

# Separate lock and shared variable
last_captured_sentence = ""
last_captured_sentence_lock = asyncio.Lock()  # Lock to synchronize access to captured_sentence


async def audio_callback_handler(indata, status):
    """Handles the callback asynchronously."""
    if status:
        print(status, file=sys.stderr)
    await audio_queue.put(bytes(indata))


async def audio_processing(args):
    global last_captured_sentence

    try:
        if args.samplerate is None:
            device_info = sd.query_devices(args.device, "input")
            args.samplerate = int(device_info["default_samplerate"])

        model = Model(lang=args.model or "en-us")

        # Get the current event loop
        loop = asyncio.get_running_loop()

        with sd.RawInputStream(
            samplerate=args.samplerate,
            blocksize=8000,
            device=args.device,
            dtype="int16",
            channels=1,
            callback=lambda indata, frames, time, status: asyncio.run_coroutine_threadsafe(
                audio_callback_handler(indata, status), loop
            ),
        ):
            print("#" * 80)
            print("Press Ctrl+C to stop the recording")
            print("#" * 80)

            rec = KaldiRecognizer(model, args.samplerate)

            while True:
                data = await audio_queue.get()  # Get data from the queue
                if rec.AcceptWaveform(data):
                    result = rec.Result()
                    # Extract the parsed sentence
                    last = result.rfind('"')
                    second_to_last = result[:last].rfind('"')
                    if last != -1 and second_to_last != -1 and second_to_last < last:
                        parsed_sentence = result[second_to_last + 1:last]
                        # Thread-safe update to shared variable
                        async with last_captured_sentence_lock:
                            last_captured_sentence = parsed_sentence

    except KeyboardInterrupt:
        print("\nAudio processing stopped")
    except Exception as e:
        print(f"Error in audio processing: {e}", file=sys.stderr)
