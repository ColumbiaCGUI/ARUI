import os
from PIL import Image
from io import BytesIO
import requests
import json
import base64
import numpy as np
import cv2
import threading
import time
from urllib.parse import urlparse

# HoloLens connection details
MIXED_REALITY_DEVICE_PORTAL_USERNAME = "bs"
MIXED_REALITY_DEVICE_PORTAL_PASSWORD = "1591590"
HOLOLENS_IP_ADDR = (
    f"http://{MIXED_REALITY_DEVICE_PORTAL_USERNAME}:{MIXED_REALITY_DEVICE_PORTAL_PASSWORD}@192.168.4.70"
)

# Shared resources
last_frame = None
last_frame_time = 0
last_frame_lock = threading.Lock()  # Lock to synchronize access to `last_frame` and `last_frame_time`

def is_device_online(timeout=2):
    """
    Check if the HoloLens device is reachable using the HOLOLENS_IP_ADDR.
    """
    try:
        parsed_url = urlparse(HOLOLENS_IP_ADDR)
        ip_address = parsed_url.hostname  # Extract hostname (IP address)

        # Attempt a socket connection to the IP address and port
        import socket
        with socket.create_connection((ip_address, 80), timeout=timeout):
            return True
    except (socket.timeout, OSError):
        return False


def live_stream(resolution="live.mp4", holo=True, pv=True, mic=False, loopback=False):
    """
    Threaded function to handle the live stream and update the global `last_frame` variable.
    """
    global last_frame, last_frame_time
    params = []
    if holo:
        params.append("holo=true")
    if pv:
        params.append("pv=true")
    if mic:
        params.append("mic=true")
    if loopback:
        params.append("loopback=true")

    param_string = "&".join(params)
    stream_url = f"http://{MIXED_REALITY_DEVICE_PORTAL_USERNAME}:{MIXED_REALITY_DEVICE_PORTAL_PASSWORD}@192.168.4.70/api/holographic/stream/{resolution}?{param_string}"

    cap = cv2.VideoCapture(stream_url)
    if not cap.isOpened():
        print("Error: Unable to open the video stream.")
        return

    print("Streaming started.")
    while True:
        ret, frame = cap.read()
        if ret:
            with last_frame_lock:
                last_frame = frame
                last_frame_time = time.time()
        time.sleep(0.01)  # Allow other tasks to run

    cap.release()
    print("Streaming stopped.")


def get_frame():
    """
    Get the last frame from the stream if it is less than a second old. Otherwise, capture a photo.
    """
    global last_frame, last_frame_time
    with last_frame_lock:
        if last_frame is not None and time.time() - last_frame_time < 1:
            # Encode the image as JPEG and then encode to Base64
            _, buffer = cv2.imencode(".jpg", last_frame)
            frame_base64 = base64.b64encode(buffer).decode("utf-8")
            return frame_base64

    try:
        response = requests.post(
            url=HOLOLENS_IP_ADDR + "/api/holographic/mrc/photo?holo=true&pv=true",
            verify=False,
        )
        if response.status_code != 200:
            return None

        try:
            photo_filename = json.loads(response.text).get("PhotoFileName")
            if not photo_filename:
                print("No PhotoFileName in response.")
                return None
        except json.JSONDecodeError as e:
            print(f"Response text: {e} {response.text}")
            return None

        encoded_file_name = str(
            base64.b64encode(photo_filename.encode("ascii")).decode("ascii")
        )
        response_image = requests.get(
            url=HOLOLENS_IP_ADDR + "/api/holographic/mrc/file?filename=" + encoded_file_name,
            verify=False,
        )
        if response_image.status_code != 200:
            print(f"Failed to retrieve photo with status code {response_image.status_code}.")
            return None

        # Load image into PIL and then convert to OpenCV (optional but you can also resize here)
        with Image.open(BytesIO(response_image.content)) as img:
            new_width = img.width // 4
            new_height = img.height // 4
            resized_img = img.resize((new_width, new_height), Image.LANCZOS)

            # Convert PIL image to OpenCV format
            open_cv_image = cv2.cvtColor(np.array(resized_img), cv2.COLOR_RGB2BGR)

            # Encode OpenCV image as JPEG and then Base64
            _, buffer = cv2.imencode(".jpg", open_cv_image)
            frame_base64 = base64.b64encode(buffer).decode("utf-8")

            return frame_base64

    except requests.RequestException as e:
        print(f"Error in get_frame: {e}")
        return None
    except Exception as e:
        print(f"Unexpected error: {e}")
        return None


def start():
    """
    Start the HoloLens connection and streaming.
    """
    if not is_device_online():
        print("HoloLens device is not reachable. Please check the connection.")
        return

    # Start the live stream in a thread
    live_stream_thread = threading.Thread(target=live_stream, daemon=True)
    live_stream_thread.start()

    print("HoloLens live stream started.")
