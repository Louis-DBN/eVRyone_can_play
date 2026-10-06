using UnityEngine;

public class HandsFreeMovement : MonoBehaviour
{
    public Transform head;
    public float moveSpeed = 2f;

    // Minimum pitch degrees (downward look) to trigger/reset a nod
    [Range(0f, 80f)] public float minNodStartThreshold = 20f;
    [Range(0f, 80f)] public float minNodEndThreshold = 10f;

    // The time window allowed to return the head up to confirm a nod
    [Range(0f, 1f)] public float minNodDuration = 0.05f;
    [Range(0.05f, 1f)] public float maxNodDuration = 0.40f;

    private float nodTimer = 0f;
    private bool nodding = false;
    private bool walking = false;

    private CharacterController characterController;
    private Joystick joystick;

    private void Start()
    {
        if (head == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null) head = camera.transform;
        }

        if (head == null || head == transform || !head.IsChildOf(transform))
        {
            Debug.LogError("Attach HandsFreeMovement to the camera rig root and assign its headset camera to Head.", this);
            enabled = false;
            return;
        }

        characterController = GetComponent<CharacterController>();
        joystick = GetComponent<Joystick>();
    }

    private void Update()
    {
        // Get pitch angle (looking up/down) converted to signed degrees (-180 to 180)
        float pitch = head.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Timer countdown
        if (nodding)
        {
            nodTimer -= Time.deltaTime;
            if (nodTimer <= 0f)
            {
                NodDurationExpired();
            }
        }

        // Nod start detection (pitch down past threshold)
        if (!nodding && pitch > minNodStartThreshold)
        {
            StartNod();
        }

        // Nod end detection (returned head up within valid time window)
        if (nodding && pitch < minNodEndThreshold)
        {
            float timeElapsed = maxNodDuration - nodTimer;
            if (timeElapsed >= minNodDuration)
            {
                EndNod();
            }
        }

        // When Joystick is present, it applies movement after checking the hands.
        if (joystick == null || !joystick.isActiveAndEnabled)
        {
            Vector3 movement = GetMovement();
            if (characterController != null && characterController.enabled)
                characterController.Move(movement);
            else
                transform.position += movement;
        }
    }

    public Vector3 GetMovement()
    {
        if (walking)
        {
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            return forward * moveSpeed * Time.deltaTime;
        }
        return Vector3.zero;
    }

    public void StopWalking()
    {
        walking = false;
    }

    private void NodDurationExpired()
    {
        nodding = false;
        nodTimer = 0f;
    }

    private void StartNod()
    {
        nodding = true;
        nodTimer = maxNodDuration;
    }

    private void EndNod()
    {
        nodding = false;
        nodTimer = 0f;
        walking = !walking; // Toggle walking state
    }
}
