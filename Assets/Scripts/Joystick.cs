using UnityEngine;
using UnityEngine.XR;

[DefaultExecutionOrder(100)]
public class Joystick : MonoBehaviour
{
    public Transform head;
    public float moveSpeed = 2f;
    [Range(0f, 0.95f)] public float joystickDeadzone = 0.15f;

    private CharacterController characterController;
    private GrabMovement climbing;
    private HandsFreeMovement handsFree;

    private void Start()
    {
        if (head == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null) head = camera.transform;
        }

        if (head == null || head == transform || !head.IsChildOf(transform))
        {
            Debug.LogError("Attach Joystick to the camera rig root and assign its headset camera to Head.", this);
            enabled = false;
            return;
        }

        characterController = GetComponent<CharacterController>();
        climbing = GetComponent<GrabMovement>();
        handsFree = GetComponent<HandsFreeMovement>();
    }

    private void LateUpdate()
    {
        InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (climbing != null && climbing.isActiveAndEnabled &&
            climbing.TryGetMovement(left, right, out Vector3 pull))
        {
            if (handsFree != null) handsFree.SuppressMovement();
            MoveRig(pull);
            return;
        }

        if (left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick))
        {
            joystick = Vector2.ClampMagnitude(joystick, 1f);
            if (joystick.magnitude > joystickDeadzone)
            {
                if (handsFree != null) handsFree.SuppressMovement();

                Quaternion heading = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
                Vector3 direction = heading * new Vector3(joystick.x, 0f, joystick.y);
                Vector3 movement = direction * Mathf.Max(0f, moveSpeed) * Time.deltaTime;

                MoveRig(movement);
                return;
            }
        }
    }

    private void MoveRig(Vector3 movement)
    {
        if (characterController != null && characterController.enabled)
            characterController.Move(movement);
        else
            transform.position += movement;
    }
}
