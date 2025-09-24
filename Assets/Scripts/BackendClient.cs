using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class BackendClient : MonoBehaviour
{
    [Tooltip("Backend URL that accepts speech and returns the event JSON.")]
    public string backendUrl = "https://autecologic-uncoordinately-kellee.ngrok-free.dev/agent";

    // callback(success, responseJson)
    public void SendSpeech(string speech, Action<bool, string> callback)
    {
        StartCoroutine(SendSpeechCoroutine(speech, callback));
    }

    private IEnumerator SendSpeechCoroutine(string speech, Action<bool, string> callback)
    {
        var payload = new BackendPayload { input = speech };
        string body = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

        using (UnityWebRequest www = new UnityWebRequest(backendUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"Backend error: {www.error}");
                callback?.Invoke(false, null);
            }
            else
            {
                callback?.Invoke(true, www.downloadHandler.text);
            }
        }
    }

    [Serializable]
    private class BackendPayload
    {
        public string input;
    }
}
