using UnityEngine;

/// <summary>
/// Switches animation based on A/D input.
/// Attach to your Player GameObject.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Horizontal", horizontal);
    }
}