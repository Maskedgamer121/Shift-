using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private float lastDirection = 1f; // 1 = right, -1 = left

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Remember last direction
        if (horizontal != 0)
            lastDirection = horizontal;

        animator.SetFloat("MoveX", horizontal);
        animator.SetFloat("LastX", lastDirection);
    }
}