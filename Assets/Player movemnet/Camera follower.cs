using UnityEngine;

/// <summary>
/// 2D camera that locks directly onto the player with no delay.
/// Attach to your Main Camera.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Drag your Player here

    [Header("Offset")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );
    }
}