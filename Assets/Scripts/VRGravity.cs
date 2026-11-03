using UnityEngine;

[DefaultExecutionOrder(200)]
public class VRGravity : MonoBehaviour
{
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedVelocity = -2f;

    private CharacterController characterController;
    private GrabMovement grabMovement;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        grabMovement = GetComponent<GrabMovement>();

        if (characterController == null)
        {
            Debug.LogError("VRGravity requires a CharacterController.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (grabMovement != null && grabMovement.IsGrabbing)
        {
            verticalVelocity = 0f;
            return;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVelocity;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }
}