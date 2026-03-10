using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips; // Drag 1 or more clips in Inspector
    public float stepInterval = 0.4f; // Time between steps

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private float stepTimer = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // Reset so first step plays immediately
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        // Pick a random clip if you have multiple
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip);
    }
}