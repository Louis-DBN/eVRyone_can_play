using Unity.Mathematics;
using UnityEngine;

public class HandsFreeMovement : MonoBehaviour
{
    public Transform head;
    
    public float maxMoveSpeed = 3f;
    public AnimationCurve moveSpeedCurve;

    [Header("Tilt Camera (Degrees)")]
    [Range(5f, 60f)] public float minNodForwardThreshold = 10f;
    [Range(5f, 60f)] public float maxNodForwardThreshold = 25f;
    [Range(5f, 60f)] public float minLeanSidewaysThreshold = 10f;
    [Range(5f, 60f)] public float maxLeanSidewaysThreshold = 35f;
    
    [Header("Turning Camera")]
    public float maxTurnSpeed = 40f; // Speed of yaw rotation in degrees per second

    public AnimationCurve turnSpeedCurve;

    [Range(10f, 60f)] public float minYawTurnThreshold = 25f;

    [Range(10f, 60f)] public float maxYawTurnThreshold = 60f;

    

    private CharacterController characterController;

    private void Start()
    {
        if (head == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null) head = camera.transform;
        }

        if (head == null || head == transform || !head.IsChildOf(transform))
        {
            Debug.LogError("Attach Movement to XRRig and assign its headset camera to Head.", this);
            enabled = false;
            return;
        }

        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Extract local Euler angles relative to character body
        Vector3 localAngles = head.localEulerAngles;

        // Convert angles from [0, 360] to signed [-180, 180]
        float pitch = WrapAngle(localAngles.x); // Pitch (Looking Up / Down)
        float yaw   = WrapAngle(localAngles.y); // Yaw   (Looking Left / Right)
        float roll  = WrapAngle(localAngles.z); // Roll  (Tilting Head Left / Right)

        Vector3 moveDirection = Vector3.zero;

        // --- 1. Forward Movement (Pitch) ---
        // Only allow moving forward, never backward.
        if (pitch > minNodForwardThreshold)
        {
            float forwardAmount = Mathf.InverseLerp(minNodForwardThreshold, maxNodForwardThreshold, pitch);
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            moveDirection += forward * Mathf.Clamp01(forwardAmount);
        }

        // --- 2. Sideways Strafe Movement (Roll / Head Lean) ---
        // Roll < 0 = Lean Left, Roll > 0 = Lean Right (Unity standard)
        if (Mathf.Abs(roll) > minLeanSidewaysThreshold)
        {
            float rollAmount = Mathf.InverseLerp(minLeanSidewaysThreshold, maxLeanSidewaysThreshold, Mathf.Abs(roll));
            Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
            moveDirection += Mathf.Clamp01(rollAmount) * Mathf.Sign(-roll) * right;
        }

        // Execute combined translation movement
        if (moveDirection.sqrMagnitude > 0f)
        {
            float moveStrength = Mathf.Clamp01(moveDirection.magnitude);
            float speedMultiplier = EvaluateMovementSpeed(moveStrength);
            Move(moveDirection.normalized, speedMultiplier);
        }

        // --- 3. Turning / Rotating Body (Yaw) ---
        // When turning your head past threshold, rotate the XR Rig towards that direction
        if (Mathf.Abs(yaw) > minYawTurnThreshold)
        {
            // Value between 0 and 1 to multiply with curve
            float yawValue = Mathf.InverseLerp(minYawTurnThreshold, maxYawTurnThreshold, Mathf.Abs(yaw));//math.min(math.abs(yaw), maxYawTurnThreshold) / maxYawTurnThreshold;
            float turnDirection = Mathf.Sign(yaw);
            transform.Rotate(Vector3.up, turnDirection * yawValue * maxTurnSpeed * Time.deltaTime);
        }
    }

    private void Move(Vector3 direction, float speedMultiplier)
    {
        Vector3 movement = direction * maxMoveSpeed * speedMultiplier * Time.deltaTime;

        if (characterController != null && characterController.enabled)
            characterController.Move(movement);
        else
            transform.position += movement;
    }

    private float EvaluateMovementSpeed(float inputAmount)
    {
        float clamped = Mathf.Clamp01(inputAmount);

        if (moveSpeedCurve == null || moveSpeedCurve.length == 0)
            return clamped;

        return Mathf.Clamp01(moveSpeedCurve.Evaluate(clamped));
    }

    private float WrapAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}