using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

public class VRMicRecorder : MonoBehaviour
{
    [Header("Mic Settings")]
    public MicController micController;
    public TextMeshProUGUI statusText;
    public EventDispatcher eventDispatcher;

    private InputDevice rightController;
    private bool isRecording = false;
    private bool buttonPressed = false;

    [Header("UI Button")]
    public TextMeshProUGUI buttonLabel;

    private const string apiEndpoint = "https://autecologic-uncoordinately-kellee.ngrok-free.dev/agent";

    void Start()
    {
        if (statusText != null)
            statusText.text = "Idle";
    }

    public void ToggleRecording()
    {
        isRecording = !isRecording;
        micController.ToggleRecording();

        if (isRecording)
        {
            if (statusText != null) statusText.text = "Recording...";
            if (buttonLabel != null) buttonLabel.text = "Stop";
            Debug.Log("🎙Recording started.");
        }
        else
        {
            Debug.Log("Recording stopped. Preparing to send data.");
            if (statusText != null) statusText.text = "Processing...";
            if (buttonLabel != null) buttonLabel.text = "Ask";

            AudioClip clipToSend = micController.recordedClip;

            if (clipToSend != null)
            {
                StartCoroutine(SendAudioRequest(clipToSend));
            }
            else
            {
                Debug.LogError("Recorded AudioClip is null!");
                if (statusText != null) statusText.text = "Error: No Clip";
            }
        }
    }

    IEnumerator SendAudioRequest(AudioClip clip)
    {
        // 1. Convert AudioClip to WAV
        byte[] wavData = ConvertAudioClipToWav(clip);
        if (wavData == null)
        {
            Debug.LogError("Failed to convert AudioClip to WAV.");
            yield break;
        }

        // 2. Encode to Base64
        string base64Audio = System.Convert.ToBase64String(wavData);

        // 3. Construct JSON payload with corrected event definitions
        string jsonPayload = $@"
        {{
            ""user_message"": ""{base64Audio}"",
            ""scene_context"": ""A cozy living room with white walls"",
            ""event_definitions"": [
                {{
                    ""name"": ""SetLightState"",
                    ""parameters"": {{
                        ""area"": ""Studyroom, Bedroom, Livingroom, Kitchen"",
                        ""state"": ""true/false""
                    }}
                }},
                {{
                    ""name"": ""SetLightIntensity"",
                    ""parameters"": {{
                        ""area"": ""Studyroom, Bedroom, Livingroom, Kitchen"",
                        ""intensity"": ""float""
                    }}
                }},
                {{
                    ""name"": ""SetWallColor"",
                    ""parameters"": {{
                        ""area"": ""Bedroom, Bathroom, Livingroom, Studyroom, Kitchen, Diningroom"",
                        ""color"": ""#RRGGBB""
                    }}
                }},
                {{
                    ""name"": ""TogglePlant"",
                    ""parameters"": {{
                        ""area"": ""Balcony, Bedroom, Livingroom, DiningRoom"",
                        ""active"": ""true/false""
                    }}
                }}
            ],
            ""chat_event_history"": """"
        }}";

        // 🔹 Debug log payload
        Debug.Log("📤 Sending JSON payload:\n" + jsonPayload);

        // 4. Send POST request
        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (statusText != null) statusText.text = "Sending...";

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                // 🔹 Debug log backend response
                Debug.Log("Backend response:\n" + responseText);

                if (eventDispatcher != null)
                {
                    eventDispatcher.ProcessBackendResponse(responseText);
                }
                else
                {
                    Debug.LogWarning("⚠EventDispatcher is not assigned in the inspector.");
                }

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
                            statusText.text = "Response Processed";
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to parse backend response JSON: {e.Message}");
                        statusText.text = "Action Performed";
                    }
                }
            }
            else
            {
                Debug.LogError("Error sending request: " + request.error);
                if (statusText != null) statusText.text = "Error: " + request.responseCode;
            }
        }
    }

    #region Audio Conversion Helper
    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        if (clip == null) return null;

        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(new byte[4], 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes(16), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes((ushort)1), 0, 2);
            memoryStream.Write(System.BitConverter.GetBytes(clip.channels), 0, 2);
            memoryStream.Write(System.BitConverter.GetBytes(clip.frequency), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4);
            memoryStream.Write(System.BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2);
            memoryStream.Write(System.BitConverter.GetBytes((ushort)16), 0, 2);

            memoryStream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            memoryStream.Write(new byte[4], 0, 4);

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            for (int i = 0; i < samples.Length; i++)
            {
                short intSample = (short)(samples[i] * short.MaxValue);
                byte[] byteSample = System.BitConverter.GetBytes(intSample);
                memoryStream.Write(byteSample, 0, 2);
            }

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