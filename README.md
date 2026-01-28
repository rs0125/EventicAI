# Eventic AI: Unity SDK & Event Orchestrator

**Eventic AI** is a plug-and-play Unity SDK that transforms static environments into fully interactive, voice-driven experiences. By bridging the gap between Natural Language Processing (NLP) and Unity’s event system, Eventic AI allows players to manipulate the world—changing lights, repainting walls, or toggling objects—simply by speaking.

https://github.com/user-attachments/assets/de28a6df-59ee-404c-a3a8-8ddf98336804

https://github.com/user-attachments/assets/0c955c8e-6af4-4b39-a72a-f5e295f7e985


##  Features

* **Natural Language Interactivity:** Move beyond hard-coded triggers; use voice or text to drive scene changes.
* **Backend Agnostic Architecture:** Connects to the **Eventic Stack** (STT -> LangChain Agent -> TTS).
* **Typed Event Dispatching:** A robust `EventDispatcher` that maps structured JSON from the AI directly to Unity C# methods.
* **XR Ready:** Includes specialized controllers for VR audio capture and "Face Camera" UI elements for immersive interactions.
* **Dynamic World Management:** Built-in managers for Lighting, Materials (Walls), and Object Visibility (Plants).

---

##  Architecture

The pipeline follows a stateless interpretation model:

1. **Capture:** `VRMicRecorder` captures user audio and converts it to a Base64 string.
2. **Request:** `GameManager` sends the audio and scene context to the `BackendClient`.
3. **Process:** The backend (Eventic Stack) transcribes the audio and uses a LangChain agent to return a JSON "To-Do List."
4. **Dispatch:** `EventDispatcher` parses the JSON and invokes the corresponding manager logic.

---

##  Installation & Setup

1. **Clone the Repository:**
```bash
git clone https://github.com/your-repo/eventic-ai.git

```


2. **Dependencies:** * Ensure **Newtonsoft.Json** is installed via the Unity Package Manager.
* **TextMeshPro** for status UI feedback.


3. **Configure Backend:**
* Open `BackendClient.cs` and update the `backendUrl` to your hosted Eventic Stack endpoint.


4. **Assign Managers:**
* In your Unity Scene, create an empty `EventOrchestrator` object.
* Attach `EventDispatcher`, `LightManager`, `WallManager`, and `PlantManager`.
* Link the managers to the `EventDispatcher` slots in the Inspector.



---

##  Supported Events

The `EventDispatcher` currently supports the following structured commands from the AI:

| Event Name | Parameters | Description |
| --- | --- | --- |
| `SetLightState` | `area` (string), `state` (bool/on/off) | Turns lights in a specific area on or off. |
| `SetLightIntensity` | `area` (string), `intensity` (float) | Dims or brightens lights in a specific area. |
| `SetWallColor` | `area` (string), `color` (Hex string) | Changes the material color of walls in a specific area. |
| `TogglePlant` | `area` (string), `active` (bool) | Shows or hides plant assets in a specific area. |

---

##  Directory Structure

* **`/Scripts`**
* `BackendClient.cs`: Handles HTTP communication with the AI stack.
* `EventDispatcher.cs`: The core engine that routes AI instructions to game logic.
* `GameManager.cs`: Orchestrates the flow between input and networking.
* **Managers:** `LightManager.cs`, `WallManager.cs`, `PlantManager.cs`.
* **XR/UI:** `VRMicRecorder.cs`, `FaceCameraYOnly.cs`.
* **Data Models:** `BackendResponse.cs` (JSON deserialization classes).



---

##  Usage Example

To trigger an event from the VR controller:

1. Hold the **Secondary Button (B)** on the Right Controller to start recording.
2. Say: *"Make the kitchen walls red and turn off the lights."*
3. Release the button. The `GameManager` will process the audio, and the `EventDispatcher` will automatically call `SetWallColor` and `SetLightState`.
