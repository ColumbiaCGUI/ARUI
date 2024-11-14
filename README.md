# ARUI - Non-Humanoid Virtual Agent

AngelARUI is a non-humanoid virtual agent designed for Unity projects, providing task management, notifications, dialog interactions, and tethering functionalities. This system integrates with the Mixed Reality Toolkit (MRTK) to create a seamless AR user experience.

## Getting Started

### Prerequisites
- Unity 2020.3.25f
- MRTK 2.7.3 ([Download Here](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.7.3))
- An example scene is located in `Plugins/ARUI/Scenes`.
- If you want to add the ARUI in your project. Simply copy the folder `Plugins/ARUI/` into your `Plugins` folder.

---

## Scene Setup

### Hierarchy Setup
1. In your scene, create an empty GameObject at position `(0, 0, 0)` and scale `(1, 1, 1)`.
2. Attach the `AngelARUI` script to this GameObject. This script initializes and manages ARUI components at runtime.
3. Access the ARUI system globally using:
   ```csharp
   AngelARUI.Instance...
   ```

### Layers
Define the following layers in your Unity project:
- `UI` (Layer Index 5)
- `zBuffer` (Layer Index 24)
- `Hand` (Layer Index 25)
- `VM` (Layer Index 26)
- `Spatial Awareness` (Layer Index 31) (created by MRTK)

### Mixed Reality Toolkit Configuration
- **Input > Pointers**:
  - Enable **Eye Tracking**.
  - Set **Pointing Raycast Layer Masks** to include the `UI` layer.
  - Assign `ShellHandRayPointer_ARUI` prefab for articulated and generic openVR pointers.
- **Speech**:
  - Add keywords such as `"stop"`, `"next step"`, or `"toggle debug"`.
- **Articulated Hand Tracking**:
  - Assign the prefab `Resources/ARUI/Prefabs/HandJoint/` to `handjoint`.

### Camera Configuration
Locate the `GazeProvider` script on the Main Camera and set its **Raycast Layer Masks** to include `UI`.

---

## Functionalities and Examples

### Task Management
AngelARUI supports multiple tasks. Load tasks using:
```csharp
var allJsonTasks = new Dictionary<string, string>();
AngelARUI.Instance.SetManual(allJsonTasks);
AngelARUI.Instance.GoToStep("TaskID", stepIndex);
```
Example JSON task:
```json
{
  "Name": "Filter Inspection",
  "Steps": [
    { "StepDesc": "Remove the nut.", "RequiredItems": ["nut"], "SubSteps": [] },
    { "StepDesc": "Remove the air clean cover.", "RequiredItems": ["nut"], "SubSteps": [] },
    { "StepDesc": "Remove the wing nut and air filter assembly", "RequiredItems": ["wing nut", "air filter assembly"], "SubSteps": [] }
  ],
  "CurrStepIndex": 0,
  "PrevStepIndex": -1,
  "NextStepIndex": 1
}
```
Key API functions:
- `SetManual(allJsonTasks)`: Load task data.
- `GoToStep("TaskID", 0)`: Set a specific task step.
- `ClearManual()`: Clear tasks and progress.

### Notifications
Show warning messages with:
```csharp
AngelARUI.Instance.SetWarningMessage("Be careful, the stove is hot.");
AngelARUI.Instance.RemoveWarningMessage();
```

### Dialog Windows
Create dialog interactions such as confirmations and choices:

#### Confirmation Dialog
```csharp
AngelARUI.Instance.TryGetUserConfirmation(
    "Did you mean to proceed?",
    actionTriggeredOnUserConfirmation,
    actionTriggerdOnTimeOut
);
```

#### Yes/No Choices
```csharp
AngelARUI.Instance.TryGetUserYesNoChoice(
    "Are you sure you want to continue?",
    onYesAction, onNoAction, onTimeoutAction
);
```

#### Multiple Choice Dialog
```csharp
AngelARUI.Instance.TryGetUserMultipleChoice(
    "Select an option:",
    new List<string> { "Option 1", "Option 2", "Option 3" },
    new List<UnityAction> { actionForOption1, actionForOption2, actionForOption3 },
    timeoutAction
);
```

### Custom Audio File Playback
Add custom audio by placing files in the `Resources` folder and playing them with:
```csharp
AngelARUI.Instance.PlaySoundAt(worldPosition, "NameOfFileWithoutExtension");
```

### Tethering and Untethering
Register objects for tethering with:
```csharp
AngelARUI.Instance.RegisterTetheredObject(objectID, objectToRegister);
AngelARUI.Instance.Tether(objectID);
AngelARUI.Instance.Untether(objectID);
```

---

## Debugging
### Common Debugging Tools
  ```csharp
  AngelARUI.Instance.DebugShowEyeGazeTarget(true); //Show eye-gaze debug cube
  AngelARUI.Instance.DebugShowMessagesInLogger(true); //Enable debug logs
  AngelARUI.Instance.MuteAudio(true); //Mute audio
  ```

### MRTK Keyword Registration System
1. Add the keyword in **MixedRealityToolkit > Input > Speech**.
2. Register the callback in code:
   ```csharp
   AngelARUI.Instance.RegisterKeyword("Show Diagram", () => { CallbackForKeyword(); });
   ```

---

## UI and Interactions
- **Eye-Gaze Input**: Users can interact with the UI using eye gaze or voice commands.
- **Move Virtual Agent**: Use tap gestures for near interactions or raycast for far interactions.
- **Audio Instructions**: Stop audio instructions using the `stop` keyword.

---

## Limitations

- **Eye Tracking Reliability**: The accuracy of eye tracking can be inconsistent for users who wear glasses.
- **Startup Initialization**: At application startup, there may be a brief delay before the eye gaze tracking becomes reliable.
- **New User Calibration**: If the system detects a new user, eye tracking calibration may be triggered automatically. This ensures accurate tracking as calibration is essential for reliable performance.
- **Text-to-Speech (TTS)**: The Text-to-Speech functionality is only available in the built application and does not work in the Unity editor.
- **Manual Eye Calibration**: If eye tracking calibration is suboptimal, users may need to manually recalibrate by navigating to the Hololens 2 settings and rerunning the eye calibration process.

---

## 3rd Party Libraries and Assets
3D Model for testing - https://grabcad.com/library/honda-gx-160
MRTK 2.7.3 - https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.7.3
Shapes - https://assetstore.unity.com/packages/tools/particles-effects/shapes-173167
Flat Icons - https://assetstore.unity.com/packages/2d/gui/icons/ux-flat-icons-free-202525
Simple Hand Pose Detector - https://github.com/RobJellinghaus/MRTK_HL2_HandPose/tree/main

## Changelog
11/13/24:
* Added Beta support for tethering and untethering interations
* Design change for dialog windows.

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
