# ARUI - 3D UI for angel system

Env: 2020.3.25f and MRTK 2.7.3

## How to use ARUI:

1) Create an empty GameObject in the highest hierarchy level at pos (0,0,0) and scale (1,1,1)
2) Add the AngelARUI script to it. This script generates all necessary UI elements at runtime
3) Call the AngelARUI methods from another script

## Example Scene

The Unity scene 'SampleScene' in folder 'Plugins/ARUI/Scenes' shows how one can use the AngelARUI script. Other than the MRTK toolkit, there are two important components in this scene: An object with the 'AngelARUI' script attached and a script 'ExampleScript' that calls the functions of AngelARUI (at 'DummyTestDataGenerator').

## Scene Setup
If you build your own scene using the AngelARUI script, there are a few things to note:
1) The ARUI uses layers to detect various collisions (e.g., between eye gaze and UI elements). It is essential that the layer "UI" exists in the Unity project, and the layer index should be 5. Please reserve the layer for the ARUI and do not set your objects with that layer
2) Make sure that the MRTK toolkit behavior in your scene is correctly set: (or use the AngelARUI settings file - AngelMRTKSettingsProfile)
    1) Tab 'Input' -> Pointers -> Eye-tracking has to be enabled
    2) Tab 'Input' -> Pointers -> Pointing Raycast Layer Masks -> There should be a layer called "UI"
    3) Tab 'Input' -> Pointers -> Assign a pointer to the 'ShellHandRayPointer_ARUI' prefab in Resource/ARUI/Prefabs/ ( {'articulated hand', 'generic openVR'} and 'any')
    4) Tab 'Input' -> Speech -> add keyword 'stop' (the parameters at the keyword can be left None or null)
    5) Tab 'Input' -> Articulated Hand Tracking -> Assign the prefab 'Resources/ARUI/Prefabs/HandJoint/' to 'handjoint' model (important for view management)
    
3) In the hierarchy, at the main camera, search for the "GazeProvider" script and select Raycast Layer Masks -> "UI" (if not already selected)

## Functions, Testing and Debugging
The ARUI can be customized as follows:
* 'AngelARUI.Instance.DebugShowEyeGazeTarget(..)' enable/disable an eye-gaze debug cube if the user is looking at a component in the ARUI. The default is false.
* 'AngelARUI.Instance.DebugShowMessagesInLogger(..)' enable/disable debugging messages in the logger window (see example scene), Default is true
* 'AngelARUI.Instance.SetViewManagement(..)' enable/disable view management. Default is true
* 'AngelARUI.Instance.MuteAudio(..)' mute or unmute audio instructions
* 'AngelARUI.Instance.SetAgentThinking(..)' enable/disable the 'thinking' animation at the virtual agent
* 'AngelARUI.Instance.CallAgentToUser()' to force the agent to appear in front of the user
* File 'ARUISettings.cs' contains some design variables (use with caution)

All features with the exception of TTS (audio instructions) should work with holographic remoting.

## Support for Multitasking
The main part of the UI is the virtual agent; the virtual agent tells the user what task the user is currently on.

For now, the ARUI supports multiple tasks. To set the tasks, call 'AngelARUI.Instance.InitManual(allJsonTasks);' 
allJsonTasks is a Dictionary with string as key and value. The key represents the name of the task (e.g., 'Filter Inspection'), while the value represents the JSON data for the task. 

Here is an example of a task called 'Filter Inspection' with 3 steps:
```
{"Name": "Filter Inspection", "Steps":[
    {
        "StepDesc": "Remove the nut.",
        "RequiredItems": ["nut" ],
        "SubSteps": [],
        "CurrSubStepIndex": -1
    },{
        "StepDesc": "Remove the air clean cover.",
        "RequiredItems": ["nut" ],
        "SubSteps": [],
        "CurrSubStepIndex": -1
    },
    {
        "StepDesc": "Remove the wing nut and air filter assembly",
        "RequiredItems": ["wing nut", "air filter assembly" ],
        "SubSteps": [],
        "CurrSubStepIndex": -1
    }],
    "CurrStepIndex":0,"PrevStepIndex":-1,"NextStepIndex":1}
```

'StepDesc' is the description of the action the user has to execute and 'RequiredItems' is a list of items that we will emphasize in the UI.

To set the first step in the task graph as the current one, call: 'AngelARUI.Instance.GoToStep("Filter Inspection", 0);' At the moment, there is NO support for subtasks.

Overall, if you call 'AngelARUI.Instance.GoToStep("Filter Inspection", 0);' the virutal agent message will change, the task list will refresh and the user will hear the instructions (only in build).

## Notifications (beta)
At the moment, the ARUI supports warnings and a confirmation dialogue.

#### Warnings
The virutal agent will display a warning, if desired. The warning has to be removed manually. If a warning message is shown, the previous and next step will disappear temporarily. 
```
AngelARUI.Instance.SetWarningMessage("Be careful, the stove is hot.");
AngelARUI.Instance.RemoveWarningMessage();
```

#### Confirmation Dialogue 
Here is an example of how to call the confirmation dialogue (found in ExampleScript.cs). For now, the purpose of the confirmation dialogue is to ask the user for permission to execute an action if the NLP node of the system detected a certain user intent (e.g., the user wanted to go to the next task)
```
int next = 2;
AngelARUI.Instance.TryGetUserFeedbackOnUserIntent(intentMsg, actionTriggeredOnUserConfirmation, actionTriggerdOnTimeOut);
```

## MRTK Keyword Registration System
Supporting voice-triggered callbacks. 
There are two steps involved to add a new keyword:

1) In your scene, you have to manually add the keyword to the MixedRealityToolkit --> Input --> Speech --> Add new speech command. Type your keyword, e.g.: "Show Diagram"

2) In the code, register a callback to the keyword. e.g.:
```
AngelARUI.Instance.RegisterKeyword("Show Diagram", () => { CallbackForKeyword(); });
```

## Support for QA
If you want the virtual agent to say a message to the user, and display it too, call:
```
AngelARUI.Instance.PlayMessageAtAgent
    ("", "Hello");
```

If you want the agent to e.g., repeat the user input AND display the message, call:
```
AngelARUI.Instance.PlayMessageAtAgent
    ("How many apples do I need for this recipe?", "You need three apples");
```

## Build, Deploy and Run
### Build and Deploy
Before you build the project, make sure that the following layers are defined: 'zBuffer' (24), 'Hand' (25), 'VM' (26) and 'UI' (5). The layer 'Spatial Awareness' (31) is used by the ARUI as well, but usually created if MRTK is imported. 

The building process is the same as a regular HL2 application, except that before you build the app package in VS, the VMMain.dll (Plugins\WSAPlayer\ARM64) has to be added to the projects. (Right click on the UWP Project in the explorer in VS, 'Add' -> 'External File' -> VMMain.dll. Set content to "True". 

After deployment, when you run the app for the first time, make sure to give permission to the eye-tracking and it is crucial that the calibration is done properly.

# UI and Interactions
* The UI uses eye gaze as input. The user can enable and disable the task list by looking at the button next to the white virtual agent. The position of the virtual agent and the tasklist can be adjusted using the tap gesture (near interactions) or the ray cast (far interactions).
* Audio task instructions can be stopped (just once) with keyword 'stop'. 
* The confirmation button on the confirmation dialogue can be triggered using eye-gaze or touching (index finger)

## Limitations
- Eye tracking might not be reliable if the user wears glasses.
- At start-up, it might take a few seconds until the eye gaze rays is reliable
- If it is recognized by the system that a new user uses the application, the eye tracking calibration might start. This is good since eye tracking is not reliable if not correctly calibrated to the current user.
- TextToSpeech only works in build
- If eye calibration is not ideal, one has to manually go to the hololens2 settings and rerun the eye calibration

## 3rd Party Libraries and Assets
3D Model for testing - https://grabcad.com/library/honda-gx-160
MRTK 2.7.3 - https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.7.3
Shapes - https://assetstore.unity.com/packages/tools/particles-effects/shapes-173167
Flat Icons - https://assetstore.unity.com/packages/2d/gui/icons/ux-flat-icons-free-202525
Simple Hand Pose Detector - https://github.com/RobJellinghaus/MRTK_HL2_HandPose/tree/main

## Changelog
5/2/24:
* Next/Previous step disappears if there is a warning at the same time
* Added speech to the virtual agent
* Improved the fix/follow modes in regards to their visual appearance and audio feedback
* The agent can be called to the user via CallAgentToUser()
* Implemented keyword registration system.
* Bugfix of the audio system (the voice is sometimes not spatialised)
* The system can show the current observed task at the task overview (if there are multiple tasks)
* Removed the taskoverview button, but task overview can be enabled/disabled through the API - ShowTaskoverviewPanel(..) or ToggleTaskOverview(..) and it's position set with SetTaskOverviewPosition(..)

10/5/23:
* Show Next/Previous task at virtual agent
* virtual agent scales with distance to user, so the content is still legible if virtual agent is further away
* Warning and error notification + growing disc to get user's attention 
* Fix vs. Follow Mode: If virtual agent is dragged and hand is closed, the virtual agent is fixed to a 3D position, Drag and pinch to undo

6/1/23: 
* Adding fall back options if eye gaze does not work: Enable/Disable attention-based task overview and allows users to 'touch' the task list button, in addition to dwelling

5/31/23: 
* Adding Space Managment, a more accurate representation of full space for the view management algorithm
* Adding 'RegisterDetectedObject(..)' and 'DeRegisterDetectedObject(..)' so a 3D mesh can be added to the view management.
* Small improvements confirmation dialogue
* Notification indicator for task messages
* Redesign virtual agent face ('eyes', 'mouth')

3/11/23: 
* Improvement confirmation dialogue (top of FOV, instead of the bottom, added audio feedback and touch input)
* Added view management (for virtual agent (controllable), tasklist, confirmation dialogue and hands (all non-controllables). The objective of view management is to avoid decreasing the legibility of virtual or real objects in the scene. Controllable will move away from non-controllable obejcts  (e.g., the virtual agent should not overlap with hands if the user is busy working on a task)
* Code documentation 
* Minor improvements (task list fixed with transparent items, fixed virtual agent message not shown when looking at task list)
* Added 'stop' keyword that immediately stops the audio task instructions
* Audio task instructions can be muted

2/19/23: 
* Fixed issue with task id. If the taskID given in SetTaskID(..) is the same as the current one, the virtual agent will not react anymore.
* Added confirmation dialogue
* Added option to mute text to speech for task instructions

10/30/22: 
* Added dragging signifier to the task list
* Added Skip notification (message + warning sound)
* Added textToSpeech option
* fixes with eye collisions with spatial mesh
* fixes with task progress and 'all done' handling
* fixed 'jitter' of virtual agent in some situations by adding lazy virtual agent reactions in the following solver
* Added halo, so user can find task list more easily
* Disabled auto repositioning of task list (but allowing manual adjustments)

10/20/22: 
* Adding direct manipulation to task list
* If the tasklistID is greater than the number if tasks, the user sees the message "All done". Alternatively, the recipe can be set as 
  done by calling AngelARUI.Instance.SetAllTasksDone();
* The virtual agent moves out of the way if the user reads the task list
* The virtual agent shows an indicator on the progress of the recipe
* The tasklist button is moved to the top 
* Delayed task message activation to avoid accidental trigger
* Both the task list and the virtual agent can be moved using far (with ray) and near interactions with either hand
