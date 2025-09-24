using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    // Drag your AudioSource component here in the Unity Editor
    public AudioSource soundEffect;

    // This function will be called by the button
    public void PlayMySound()
    {
        soundEffect.Play();
    }
}