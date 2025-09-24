/*using UnityEngine;

public class MicController : MonoBehaviour
{
    public AudioSource playbackSource;
    public SimpleBackendConnector backendConnector;

    private bool isRecording = false;
    private AudioClip recordedClip;
    private string microphoneDevice;
    private int maxRecordingLength = 300; // 5 minutes

    void Start()
    {
        if (Microphone.devices.Length > 0) {
            microphoneDevice = Microphone.devices[0];
        } else {
            Debug.LogError("No microphone found!");
        }

        if (backendConnector == null) {
            Debug.LogError("SimpleBackendConnector is not assigned in the Inspector!");
        }
    }

    public void ToggleRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            Debug.Log("Recording started...");
            // Start recording into a large buffer
            recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingLength, 16000);
        }
        else
        {
            isRecording = false;
            Debug.Log("Recording stopped. Trimming and sending to backend...");

            // --- THE FIX IS HERE ---
            
            // 1. Get the exact position where the recording stopped
            int position = Microphone.GetPosition(microphoneDevice);

            // 2. Stop the microphone device
            Microphone.End(microphoneDevice);

            // 3. Create a new float array to hold only the recorded audio data
            float[] soundData = new float[position];
            recordedClip.GetData(soundData, 0);

            // 4. Create a new, perfectly-sized AudioClip
            AudioClip trimmedClip = AudioClip.Create("TrimmedSound", position, 1, 16000, false);
            trimmedClip.SetData(soundData, 0);

            // --- END OF FIX ---
            
            // Update the clip used for local playback
            recordedClip = trimmedClip;

            // Send the NEW, trimmed clip to the backend
            if (trimmedClip != null)
            {
                backendConnector.SendAudioToBackend(trimmedClip);
            }
        }
    }

    public void PlaySavedRecording()
    {
        if (recordedClip != null && !isRecording)
        {
            Debug.Log("Playing saved recording.");
            playbackSource.clip = recordedClip;
            playbackSource.Play();
        }
        else if (isRecording)
        {
            Debug.LogWarning("Cannot play while recording is in progress.");
        }
    }
}*/

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MicController : MonoBehaviour
{
    public AudioSource playbackSource;
    public SimpleBackendConnector backendConnector;

    [Header("UI References")]
    public Button recordButton;
    public TextMeshProUGUI buttonLabel;
    public TextMeshProUGUI transcriptionText; // <- NEW

    private bool isRecording = false;
    private AudioClip recordedClip;
    private string microphoneDevice;
    private int maxRecordingLength = 300; // 5 minutes

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone found!");
        }

        if (backendConnector == null)
        {
            Debug.LogError("SimpleBackendConnector is not assigned in the Inspector!");
        }

        if (recordButton != null)
            recordButton.onClick.AddListener(ToggleRecording);
    }

    public void ToggleRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            buttonLabel.text = "Stop 🎙️";
            Debug.Log("Recording started...");
            recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingLength, 16000);
        }
        else
        {
            isRecording = false;
            buttonLabel.text = "Start 🎙️";
            Debug.Log("Recording stopped. Trimming and sending to backend...");

            int position = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);

            float[] soundData = new float[position];
            recordedClip.GetData(soundData, 0);

            AudioClip trimmedClip = AudioClip.Create("TrimmedSound", position, 1, 16000, false);
            trimmedClip.SetData(soundData, 0);
            recordedClip = trimmedClip;

            if (trimmedClip != null)
            {
                backendConnector.SendAudioToBackend(trimmedClip, transcriptionText); // Pass text UI ref
            }
        }
    }
}
