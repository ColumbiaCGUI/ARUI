import nodes.hl2node as hl2node
import asyncio
import utils.utils as utils
import sys
import llm
import json 
import re
import nodes.tasknode as tasknode
from collections import deque

activity_log_rate = 2
activity_log = deque(maxlen=1000)
activity_log_lock = asyncio.Lock()  # Async-safe access to the log

prompt_file_path = "env_monitor"

async def monitor_env():
    """Continuously call `hl2utils.getPhoto()` every 2 seconds and log the last 20 activities."""

    activity_prompt = utils.load_prompt(prompt_file_path)
    if activity_prompt is None:
        print("Activity prompt could not be loaded.", file=sys.stderr)
        return

    if tasknode.current_instructions is not None:
        activity_prompt += "* Instructions: " + tasknode.current_instructions

    while True:
        try:
            # Call get_env_description() with the correct argument
            detected_activity = await get_env_description(activity_prompt)

            parsed_data = None
            # Validate detected_activity is not None and has the expected length
            if detected_activity and len(detected_activity) >= 2:
                json_string = detected_activity[1]

                if json_string:
                    json_string = json_string.strip()  # Strip only if json_string is not None
                    parsed_data = extract_json_data(json_string)
            
            if parsed_data is None:
                continue

            # Construct the activity dictionary, ensure fallback values for any None
            activity = {
                "json": parsed_data["json_data"] if parsed_data["json_data"] is not None else {},
                "timestamp": asyncio.get_running_loop().time(),
                "image": detected_activity[0] if detected_activity and detected_activity[0] is not None else "No image available",
            }

            # Debugging: Print parsed data to verify content
            # print(f"Parsed Activity Data: {parsed_data['json_data']}")

            # Ensure activity is valid before adding to log
            async with activity_log_lock:
                if activity["json"] and isinstance(activity["json"], dict) and activity["image"] != "No image available":
                    activity_log.append(activity)
                    try:
                        utils.save_activity_to_file(llm.session_id, parsed_data['json_data'])
                    except Exception as file_error:
                        print(f"An error occurred while saving the activity log: {file_error}", file=sys.stderr)
                else:
                    print("Activity is invalid and will not be logged.", file=sys.stderr)

            await asyncio.sleep(0.3)
        except Exception as e:
            print(f"Error in monitor_user_activity: {e}", file=sys.stderr)
            await asyncio.sleep(5)  # Retry after delay


async def get_env_description(prompt):
    if await hl2node.is_device_online(0.1):  # Await the async function
        image_base64 = await hl2node.get_frame()  # Await the async function
    else:
        return None

    if image_base64:
        response = llm.get_env(image_base64, prompt)  # Send Base64 encoded image
        return [image_base64, response]
    else:
        return None


def extract_json_data(json_string):
    """
    Extract and parse JSON data from a string.

    Args:
        json_string (str): The JSON string to parse.

    Returns:
        dict: Parsed JSON data and extracted fields.
    """
    try:
        # Clean up json_string if it contains Markdown formatting or extra backticks
        json_string = re.sub(r'^```json\s*', '', json_string)  # Remove ```json at the beginning
        json_string = re.sub(r'```$', '', json_string)  # Remove ``` at the end
        json_string = json_string.strip()  # Remove any leading or trailing whitespace

        # Parse JSON string
        json_data = json.loads(json_string)

        # Extract specific fields for detailed activity
        objects = json_data.get("objects", [])
        object_statuses = json_data.get("object_statuses", [])
        hand_status = json_data.get("hand_status", {})
        current_step = json_data.get("current_step", "")

        return {
            "json_data": json_data,  # Full parsed JSON data
            "objects": objects,  # Extracted list of objects
            "object_statuses": object_statuses,  # Extracted object statuses
            "hand_status": hand_status,  # Extracted hand status
            "current_step": current_step,  # Extracted current step
        }
    except json.JSONDecodeError as e:
        print(f"Failed to parse JSON: {e}", file=sys.stderr)
        return None

async def get_latest_activity():
    """Return the latest activity."""
    async with activity_log_lock:  # Ensure async-safe access
        return activity_log[-1] if activity_log else None

