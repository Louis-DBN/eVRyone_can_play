using UnityEngine;

public class LightFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag your Player GameObject here from the scene or assign it via code.")]
    public Transform playerTransform;

    [Header("Offset")]
    [Tooltip("Offset from the player's pivot point (e.g., to place it at chest/head height).")]
    public Vector3 offset = new Vector3(0f, 1f, 0f);

    [Header("Movement Settings")]
    [Tooltip("If checked, the light will snap instantly. If unchecked, it will smoothly catch up.")]
    public bool instantFollow = true;
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        // Safety check to make sure the player is assigned
        if (playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position + offset;

        if (instantFollow)
        {
            transform.position = targetPosition;
        }
        else
        {
            // Smoothly interpolate towards the player's position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }
}