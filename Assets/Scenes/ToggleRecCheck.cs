using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

// Helper classes to deserialize the full backend response

public class VRMicRecorder : MonoBehaviour
{
    [Header("Mic Settings")]
    public MicController micController; // Reference to your MicController script
    public TextMeshProUGUI statusText;  // UI Text element to update
    public EventDispatcher eventDispatcher; // Reference to the EventDispatcher to process responses

    private InputDevice rightController;
    private bool isRecording = false;
    private bool buttonPressed = false;
    
    // The endpoint URL provided in the prompt
    private const string apiEndpoint = "https://autecologic-uncoordinately-kellee.ngrok-free.dev/agent";

    void Start()
    {
        InitializeController();
        if (statusText != null)
            statusText.text = "Idle";
    }

    void InitializeController()
    {
        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandedControllers);

        if (rightHandedControllers.Count > 0)
        {
            rightController = rightHandedControllers[0];
            Debug.Log("Right controller found: " + rightController.name);
        }
        else
        {
            Debug.LogError("Right controller not found!");
        }
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            // Attempt to re-initialize if controller is lost
            InitializeController();
            return;
        }

        // Check for B button press
        if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bButtonValue))
        {
            if (bButtonValue && !buttonPressed)
            {
                buttonPressed = true;
                ToggleRecording();
            }
            else if (!bButtonValue && buttonPressed)
            {
                buttonPressed = false; // Reset when button released
            }
        }
    }

    void ToggleRecording()
    {
        isRecording = !isRecording;
        micController.ToggleRecording();

        if (isRecording)
        {
            // Started recording
            if (statusText != null)
                statusText.text = "Recording...";
            Debug.Log("Recording started.");
        }
        else
        {
            // Stopped recording
            Debug.Log("Recording stopped. Preparing to send data.");
            if (statusText != null)
                statusText.text = "Processing...";
            
            // Get the recorded audio clip from the controller
            AudioClip clipToSend = micController.recordedClip;
            
            if (clipToSend != null)
            {
                // Start the process of encoding and sending the data
                StartCoroutine(SendAudioRequest(clipToSend));
            }
            else
            {
                Debug.LogError("Recorded AudioClip is null!");
                if (statusText != null)
                    statusText.text = "Error: No Clip";
            }
        }
    }

    IEnumerator SendAudioRequest(AudioClip clip)
    {
        // 1. Convert AudioClip to WAV byte array
        byte[] wavData = ConvertAudioClipToWav(clip);
        if (wavData == null)
        {
            Debug.LogError("Failed to convert AudioClip to WAV.");
            yield break;
        }

        // 2. Encode WAV byte array to Base64 string
        string base64Audio = System.Convert.ToBase64String(wavData);

        // 3. Construct the JSON payload with appropriate context
        // Note: The backslashes are used to escape the quotes inside the string
        string jsonPayload = $@"
        {{
            ""user_message"": ""{base64Audio}"",
            ""scene_context"": ""A cozy living room with white walls"",
            ""event_definitions"": [
                {{""name"": ""SetLightState"", ""parameters"": {{""area"": ""string"", ""state"": ""boolean or string (on/off)""}}}},
                {{""name"": ""SetLightIntensity"", ""parameters"": {{""area"": ""string"", ""intensity"": ""float""}}}},
                {{""name"": ""SetWallColor"", ""parameters"": {{""area"": ""string"", ""color"": ""string (hex codes)""}}}},
                {{""name"": ""AddPlant"", ""parameters"": {{""area"": ""string"", ""plantType"": ""string"", ""position"": ""string""}}}},
                {{""name"": ""RemovePlant"", ""parameters"": {{""area"": ""string"", ""plantType"": ""string""}}}},
                {{""name"": ""TogglePlant"", ""parameters"": {{""area"": ""string"", ""add"": ""boolean"", ""plantType"": ""string""}}}}
            ],
            ""chat_event_history"": """"
        }}";

        // 4. Send the POST request
        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (statusText != null)
                statusText.text = "Sending...";

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully sent audio data!");
                string responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);

                // Pass the full response to the EventDispatcher to trigger scene changes
                if (eventDispatcher != null)
                {
                    eventDispatcher.ProcessBackendResponse(responseText);
                }
                else
                {
                    Debug.LogWarning("EventDispatcher is not assigned in the inspector.");
                }

                // Separately, parse the response to display the friendly text on the UI
                if (statusText != null)
                {
                    try
                    {
                        BackendResponse resp = JsonConvert.DeserializeObject<BackendResponse>(responseText);
                        if (resp != null && !string.IsNullOrEmpty(resp.npc_response))
                        {
                            statusText.text = resp.npc_response;
                        }
                        else
                        {
                            statusText.text = "Response Processed"; // Fallback message
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to parse backend response JSON for UI: {e.Message}");
                        statusText.text = "Action Performed"; // Fallback on JSON parse error
                    }
                }
            }
            else
            {
                Debug.LogError("Error sending request: " + request.error);
                if (statusText != null)
                    statusText.text = "Error: " + request.responseCode;
            }
        }
    }
    
    #region Audio Conversion Helper
    
    // This helper function converts a Unity AudioClip into a byte array in WAV format.
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        if (clip == null) return null;

        using (var memoryStream = new MemoryStream())
        {
            // WAV header
            memoryStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(new byte[4], 0, 4); // Placeholder for file size
            memoryStream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes(16), 0, 4); // Sub-chunk size
            memoryStream.Write(System.BitConverter.GetBytes((ushort)1), 0, 2); // Audio format (1 for PCM)
            memoryStream.Write(System.BitConverter.GetBytes(clip.channels), 0, 2);
            memoryStream.Write(System.BitConverter.GetBytes(clip.frequency), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4); // Byte rate
            memoryStream.Write(System.BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2); // Block align
            memoryStream.Write(System.BitConverter.GetBytes((ushort)16), 0, 2); // Bits per sample

            // Data chunk
            memoryStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            memoryStream.Write(new byte[4], 0, 4); // Placeholder for data size

            // Audio data
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            for (int i = 0; i < samples.Length; i++)
            {
                short intSample = (short)(samples[i] * short.MaxValue);
                byte[] byteSample = System.BitConverter.GetBytes(intSample);
                memoryStream.Write(byteSample, 0, 2);
            }

            // Fill in placeholders
            long fileSize = memoryStream.Length;
            memoryStream.Seek(4, SeekOrigin.Begin);
            memoryStream.Write(System.BitConverter.GetBytes((int)(fileSize - 8)), 0, 4);
            memoryStream.Seek(40, SeekOrigin.Begin);
            memoryStream.Write(System.BitConverter.GetBytes((int)(fileSize - 44)), 0, 4);

            return memoryStream.ToArray();
        }
    }
    
    #endregion
}

