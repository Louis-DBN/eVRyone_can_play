using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Joystick))]
[DisallowMultipleComponent]
public class GrabMovement : MonoBehaviour
{
    public Transform trackingSpace;
    public Transform leftHand;
    public Transform rightHand;
    [Min(0.01f)] public float grabRadius = 0.15f;
    public LayerMask solidLayers = ~0;

    private readonly Collider[] grabbedSurfaces = new Collider[2];
    private readonly Vector3[] previousPositions = new Vector3[2];
    private readonly bool[] previousGrips = new bool[2];

    public bool IsGrabbing => grabbedSurfaces[0] != null || grabbedSurfaces[1] != null;

    private void Start()
    {
        if (trackingSpace == null || leftHand == null || rightHand == null ||
            !trackingSpace.IsChildOf(transform) || !leftHand.IsChildOf(trackingSpace) ||
            !rightHand.IsChildOf(trackingSpace))
        {
            Debug.LogError("Assign TrackingSpace, LeftHandAnchor and RightHandAnchor from this rig to GrabMovement.", this);
            enabled = false;
            return;
        }
        Debug.Log("Climbing ready: hold the side grip near a wall to grab it.", this);
    }

    // Joystick calls this after the rig updates its hand anchors.
    public bool TryGetMovement(InputDevice left, InputDevice right, out Vector3 movement)
    {
        movement = Vector3.zero;
        if (trackingSpace == null) return false;

        int hands = 0;
        if (GetHandPull(left, leftHand, 0, out Vector3 leftPull))
        {
            movement += leftPull;
            hands++;
        }
        if (GetHandPull(right, rightHand, 1, out Vector3 rightPull))
        {
            movement += rightPull;
            hands++;
        }

        if (hands == 0) return false;
        movement /= hands;
        return true;
    }

    private bool GetHandPull(InputDevice device, Transform anchor, int hand, out Vector3 pull)
    {
        pull = Vector3.zero;
        bool holdingGrip = (device.TryGetFeatureValue(CommonUsages.grip, out float grip) && grip >= 0.5f)
            || (device.TryGetFeatureValue(CommonUsages.gripButton, out bool pressed) && pressed)
            || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,
                hand == 0 ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch) >= 0.5f;
        bool justPressed = holdingGrip && !previousGrips[hand];
        previousGrips[hand] = holdingGrip;
        if (justPressed) Debug.Log((hand == 0 ? "Left" : "Right") + " grip detected", this);

        if (!holdingGrip || anchor == null)
        {
            Release(hand);
            return false;
        }

        Vector3 position = trackingSpace.InverseTransformPoint(anchor.position);
        Collider surface = grabbedSurfaces[hand];
        if (surface != null && (!surface.enabled || !surface.gameObject.activeInHierarchy))
        {
            Release(hand);
            return false;
        }

        if (surface == null)
        {
            surface = FindSurface(anchor.position);
            if (surface == null)
            {
                if (justPressed) Debug.Log("No solid collider near " + anchor.name +
                    ". Check the wall collider, Solid Layers and Grab Radius.", this);
                return false;
            }

            grabbedSurfaces[hand] = surface;
            previousPositions[hand] = position;
            Debug.Log((hand == 0 ? "Left" : "Right") + " hand grabbed " + surface.name, this);
        }

        // Tracking-space positions exclude rig movement, preventing a feedback loop.
        pull = trackingSpace.TransformVector(previousPositions[hand] - position);
        previousPositions[hand] = position;
        return true;
    }

    private Collider FindSurface(Vector3 handPosition)
    {
        Physics.SyncTransforms();
        Collider[] nearby = Physics.OverlapSphere(handPosition, grabRadius,
            solidLayers, QueryTriggerInteraction.Ignore);
        foreach (Collider surface in nearby)
        {
            if (!surface.transform.IsChildOf(transform)) return surface;
        }
        return null;
    }

    private void Release(int hand)
    {
        if (grabbedSurfaces[hand] != null)
            Debug.Log((hand == 0 ? "Left" : "Right") + " hand released", this);
        grabbedSurfaces[hand] = null;
    }

    private void OnDisable()
    {
        Release(0);
        Release(1);
        previousGrips[0] = previousGrips[1] = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (leftHand != null) Gizmos.DrawWireSphere(leftHand.position, grabRadius);
        if (rightHand != null) Gizmos.DrawWireSphere(rightHand.position, grabRadius);
    }
}
