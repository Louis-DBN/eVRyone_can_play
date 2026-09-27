using UnityEngine;
using UnityEngine.XR;

public class Movement : MonoBehaviour
{
    public Transform head;
    public float moveSpeed = 2f;
    [Range(0f, 0.95f)] public float joystickDeadzone = 0.15f;

    private readonly bool[] leftButtons = new bool[6];
    private readonly bool[] rightButtons = new bool[6];
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
        InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        CheckButtons(left, leftButtons, true);
        CheckButtons(right, rightButtons, false);

        if (left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick))
        {
            joystick = Vector2.ClampMagnitude(joystick, 1f);
            if (joystick.magnitude <= joystickDeadzone) return;

            // Only use the headset's horizontal direction.
            Quaternion heading = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
            Vector3 direction = heading * new Vector3(joystick.x, 0f, joystick.y);
            Vector3 movement = direction * Mathf.Max(0f, moveSpeed) * Time.deltaTime;

            if (characterController != null && characterController.enabled)
                characterController.Move(movement);
            else
                transform.position += movement;
        }
    }

    private void CheckButtons(InputDevice device, bool[] previous, bool isLeft)
    {
        string hand = isLeft ? "Left" : "Right";
        CheckButton(device, CommonUsages.primaryButton, previous, 0, isLeft ? "X" : "A");
        CheckButton(device, CommonUsages.secondaryButton, previous, 1, isLeft ? "Y" : "B");
        CheckButton(device, CommonUsages.primary2DAxisClick, previous, 2, hand + " joystick");
        CheckButton(device, CommonUsages.triggerButton, previous, 3, hand + " trigger");
        CheckButton(device, CommonUsages.gripButton, previous, 4, hand + " grip");
        if (isLeft) CheckButton(device, CommonUsages.menuButton, previous, 5, "Left menu");
    }

    private void CheckButton(InputDevice device, InputFeatureUsage<bool> button,
        bool[] previous, int index, string name)
    {
        bool pressed = device.TryGetFeatureValue(button, out bool value) && value;
        if (pressed && !previous[index]) Debug.Log(name + " pressed", this);
        previous[index] = pressed;
    }
}
