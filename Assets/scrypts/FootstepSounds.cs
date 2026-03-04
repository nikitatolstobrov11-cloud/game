using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;

    [Header("Jump Sound")]
    public AudioClip jumpClip;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Этот метод вызывается из событий в анимациях ходьбы/бега
    public void Footstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || audioSource == null)
            return;

        int index = Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[index];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            // Debug.Log("Звук шага");
        }
    }

    // Этот метод вызывается из события в анимации прыжка
    public void JumpSound()
    {
        if (jumpClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpClip);
            // Debug.Log("Звук прыжка");
        }
    }
}