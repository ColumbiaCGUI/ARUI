import json
import llm
import utils.utils as utils

task_classes = {
    1: "None",
    2: "dinosaur",
    3: "chest"
}

current_task = 1
current_instructions = None
current_taskID = 0 #step index
current_taskoverview = None

instruction_string = ""

def initiate_task(task_id,startID=0):
    global current_task
    global current_instructions
    global instruction_string
    global current_taskID
    global current_taskoverview

    current_instructions = utils.json_to_dict("data/"+task_classes[task_id]+"/instructions")
    current_task = task_id
    current_taskID = startID

    current_taskoverview = None
    from_file = utils.read_base64_text_file("data/"+task_classes[task_id]+"/overview")
    if from_file:
        current_taskoverview = from_file

    steps_string = ""
    for step, details in current_instructions.items():
        steps_string+=str(step) + ": " + details['StepDesc']+ "\n"

    instruction_string = f"Please guide me through the following tasks one-by-one. In this converstaion I might ask some questions. Use the givne visual context to answer them. \
             Complete task list is here: {steps_string}."
    llm.clear_conv_history()
    llm.continue_conv(instruction_string, image=current_taskoverview)
    
def get_current_task_str():
    if current_instructions is None:
        return ""
    
    if current_taskID>=len(current_instructions):
        return "User is done!"
    else:
        return current_instructions[str(current_taskID+1)]['StepDesc']

def get_task_status_prompt():
    if current_instructions is None:
        return ""
    return f"The user is currently working on currently working on step {str(current_taskID+1)} - {get_current_task_str()}"
    
def go_to_previous():
    global current_taskID
    if current_taskID > 0:  # Ensure it doesn't go below 0
        current_taskID -= 1
        #llm.continue_conv(f"I just wanted to let you know that I moved on to {str(current_taskID+1)} - {get_current_task_str()}")
        print(f"Moved to the previous task with id {current_taskID+1} - {get_current_task_str()}")

def go_to_next():
    global current_taskID
    if current_task is not None and current_instructions is not None:
        max_taskID = len(current_instructions) - 1
        if current_taskID < max_taskID:  # Ensure it doesn't exceed the limit
            current_taskID += 1
            #llm.continue_conv(f"I just wanted to let you know that I went back to {str(current_taskID+1)} - {get_current_task_str()}")
            print(f"Moved to the next task with id {current_taskID+1} - {get_current_task_str()}")
        elif current_taskID == max_taskID:  # Ensure it doesn't exceed the limit
            current_taskID = max_taskID+1
            #llm.continue_conv(f"I just wanted to let you know that I went back to {str(current_taskID+1)} - {get_current_task_str()}")
            print(f"User is done")