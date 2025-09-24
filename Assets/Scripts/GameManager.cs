using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BackendClient backendClient;
    public EventDispatcher eventDispatcher;

    //private void Awake()
    //{
    //    if (backendClient == null) backendClient = GetComponent<BackendClient>();
    //    if (eventDispatcher == null) eventDispatcher = GetComponent<EventDispatcher>();
    //}

    // Call this to send speech text to backend
    public void SendSpeechToBackend(string speech)
    {
        StartCoroutine(SendSpeechCoroutine(speech));
    }

    private IEnumerator SendSpeechCoroutine(string speech)
    {
        // send request
        bool completed = false;
        string responseJson = null;
        backendClient.SendSpeech(speech, (success, json) =>
        {
            completed = true;
            responseJson = json;
        });

        // wait until callback triggers
        while (!completed) yield return null;

        if (string.IsNullOrEmpty(responseJson))
        {
            Debug.LogWarning("Empty backend response");
            yield break;
        }

        // Dispatch events to Unity
        eventDispatcher.ProcessBackendResponse(responseJson);
    }
}
