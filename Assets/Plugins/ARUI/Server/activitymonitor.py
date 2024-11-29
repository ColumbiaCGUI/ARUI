import hl2utils
import asyncio
import utils
import sys
from collections import deque

activity_log_rate = 2
activity_log = deque(maxlen=90)
activity_log_lock = asyncio.Lock()  # Async-safe access to the log

async def monitor_user_activity():
    """Continuously call `hl2utils.getPhoto()` every 2 seconds and log the last 20 activities."""
    while True:
        try:
            # Call hl2utils.getPhoto() (replace with the actual implementation)
            detected_activity = await asyncio.to_thread(hl2utils.get_current_activity)
            if detected_activity != None:
                activity = {
                    "label": detected_activity[1],
                    "timestamp": asyncio.get_running_loop().time(),
                    "image": detected_activity[0],
                }
                async with activity_log_lock:  # Ensure async-safe access
                    if utils.similar(detected_activity[1], "None")<0.8:
                        activity_log.append(activity)  # Append activity to the log
            
            await asyncio.sleep(0.5)
        except Exception as e:
            print(f"Error in monitor_user_activity: {e}", file=sys.stderr)
            await asyncio.sleep(5)  # Retry after delay

async def get_latest_activity():
    """Return the latest activity."""
    async with activity_log_lock:  # Ensure async-safe access
        return activity_log[-1] if activity_log else None