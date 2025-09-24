using UnityEngine;



public class MicController : MonoBehaviour

{
    


    public AudioSource playbackSource;

   // public SimpleBackendConnector backendConnector;



    private bool isRecording = false;

    public AudioClip recordedClip { get; private set; }

    private string microphoneDevice;

    private int maxRecordingLength = 300; // 5 minutes



    void Start()

    {

        if (Microphone.devices.Length > 0) {

            microphoneDevice = Microphone.devices[0];

        } else {

            Debug.LogError("No microphone found!");

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

                //backendConnector.SendAudioToBackend(trimmedClip);

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

}