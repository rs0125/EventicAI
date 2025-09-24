using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCTalkingAnimation : MonoBehaviour
{
    public Animator npcAnimator;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (npcAnimator != null)
        {
            // If audio is playing, set Talking true
            npcAnimator.SetBool("isTalking", audioSource.isPlaying);
        }
    }
}
