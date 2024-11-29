import os
from PIL import Image
from io import BytesIO
import requests
import json
import base64
import socket
import llm_OA
from urllib.parse import urlparse

MIXED_REALITY_DEVICE_PORTAL_USERNAME = "bs"
MIXED_REALITY_DEVICE_PORTAL_PASSWORD = "1591590"
HOLOLENS_IP_ADDR = "http://" + MIXED_REALITY_DEVICE_PORTAL_USERNAME + ":" + MIXED_REALITY_DEVICE_PORTAL_PASSWORD + "@" + "192.168.4.70"

def get_current_activity(prompt):
    if is_device_online(0.1):
        image = get_Frame()
    else:
        return None
    
    if image:
        return [image, llm_OA.get_activity(image,prompt)]
    else:
        return None
    


def is_device_online(timeout=2):
    """
    Check if the HoloLens device is reachable using the HOLOLENS_IP_ADDR.
    
    Args:
        timeout (int): The timeout in seconds for the connection test.
        
    Returns:
        bool: True if the device is online, False otherwise.
    """
    try:
        # Parse the IP address from HOLOLENS_IP_ADDR
        parsed_url = urlparse(HOLOLENS_IP_ADDR)
        ip_address = parsed_url.hostname  # Extract hostname (IP address)

        # Default port is 80 for HTTP, but you can adjust as needed
        port = 80

        # Create a socket connection to the IP address and port
        with socket.create_connection((ip_address, port), timeout=timeout):
            return True
    except (socket.timeout, socket.error) as e:
        return False
    
def get_Frame():
    try:
        # Step 1: Make the POST request to capture a photo
        response = requests.post(
            url=HOLOLENS_IP_ADDR + '/api/holographic/mrc/photo?holo=true&pv=true',
            verify=False
        )
        
        # Check if the response status is 200 OK
        if response.status_code != 200:
            return None

        # Try to decode the JSON response
        try:
            photo_filename = json.loads(response.text).get("PhotoFileName")
            if not photo_filename:
                return None
        except json.JSONDecodeError as e:
            print(f"Response text: {e} {response.text}")
            return None

        # Step 2: Encode the filename for the next request
        encoded_file_name = str(base64.b64encode(photo_filename.encode('ascii')).decode('ascii'))
        
        # Step 3: Make a GET request to retrieve the photo
        response_image = requests.get(
            url=HOLOLENS_IP_ADDR + '/api/holographic/mrc/file?filename=' + encoded_file_name,
            verify=False
        )
        
        # Check if the response status is 200 OK
        if response_image.status_code != 200:
            return None

        # Step 4: Resize the image to 1/4 the resolution
        with Image.open(BytesIO(response_image.content)) as img:
            # Print original resolution
            #print(f"Original resolution: {img.width}x{img.height}")
            
            # Reduce resolution by 4 (halve width and height)
            new_width = img.width // 4
            new_height = img.height // 4
            resized_img = img.resize((new_width, new_height), Image.LANCZOS)

            # Print resized resolution
            #print(f"Resized resolution: {new_width}x{new_height}")

            # Save resized image to a local folder
            os.makedirs("capture", exist_ok=True)  # Create folder if it doesn't exist
            local_file_path = os.path.join("capture", "resized_image.jpg")
            resized_img.save(local_file_path, format="JPEG")
            #print(f"Resized image saved to: {local_file_path}")

        # Step 5: Encode the resized image in base64 and prepare the data URI
        resized_image_io = BytesIO()
        resized_img.save(resized_image_io, format="JPEG")  # Save to BytesIO for base64 encoding
        resized_image_io.seek(0)

        base64_encoded_image = base64.b64encode(resized_image_io.read()).decode('utf-8')
        data_uri = f"data:image/jpeg;base64,{base64_encoded_image}"
        
        return data_uri

    except requests.RequestException as e:
        print(f"Request error: {e}")
        return None
    except Exception as e:
        print(f"Unexpected error: {e}")
        return None
