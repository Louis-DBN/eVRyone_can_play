using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(200)]
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

    [Header("Motion Sickness Aids")]
    [Min(0.005f)] public float focusPointSize = 0.02f;
    [Min(0.1f)] public float focusPointDistance = 0.5f;

    private CharacterController characterController;
    private Camera headCamera;
    private int suppressedFrame = -1;

    private Transform redDot;
    private Transform greenDot;
    private Texture2D dotTexture;
    private Material redMat;
    private Material greenMat;

    public void SuppressMovement()
    {
        suppressedFrame = Time.frameCount;
    }

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

        headCamera = head.GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
        CreateComfortDots();
    }

    private void LateUpdate()
    {
        if (suppressedFrame == Time.frameCount) return;

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
            float speedMultiplier = EvaluateCurve(moveSpeedCurve, moveStrength);
            Move(moveDirection.normalized, speedMultiplier);
        }

        // --- 3. Turning / Rotating Body (Yaw) ---
        // When turning your head past threshold, rotate the XR Rig towards that direction
        if (Mathf.Abs(yaw) > minYawTurnThreshold)
        {
            // Value between 0 and 1 to multiply with curve
            float yawValue = Mathf.InverseLerp(minYawTurnThreshold, maxYawTurnThreshold, Mathf.Abs(yaw));//math.min(math.abs(yaw), maxYawTurnThreshold) / maxYawTurnThreshold;
            float turnStrength = EvaluateCurve(turnSpeedCurve, yawValue);
            float turnDirection = Mathf.Sign(yaw);
            transform.Rotate(Vector3.up, turnDirection * turnStrength * maxTurnSpeed * Time.deltaTime);
        }

        UpdateComfortDots();
    }

    private void Move(Vector3 direction, float speedMultiplier)
    {
        Vector3 movement = direction * maxMoveSpeed * speedMultiplier * Time.deltaTime;

        if (characterController != null && characterController.enabled) {
            // Add gravity other wise can't here foot steps
            if (characterController.isGrounded && movement.y >= 0f)
            {
                movement.y = -0.001f;
            }
            characterController.Move(movement);
        }
        else
            transform.position += movement;
    }

    private float EvaluateCurve(AnimationCurve curve, float inputAmount)
    {
        float clamped = Mathf.Clamp01(inputAmount);
        if (curve == null || curve.length == 0)
            return clamped;
        return Mathf.Clamp01(curve.Evaluate(clamped));
    }

    private float WrapAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    private void CreateComfortDots()
    {
        dotTexture = CreateCircleTexture(64);
        redMat = CreateOverlayMaterial(Color.red, 4001);
        greenMat = CreateOverlayMaterial(Color.green, 4000);

        redDot = CreateDotQuad("Red Focus Dot", head, redMat);
        greenDot = CreateDotQuad("Green Center Dot", transform, greenMat);
        UpdateComfortDots();
    }

    private void UpdateComfortDots()
    {
        if (greenDot == null || redDot == null) return;

        // Ensure distance is never closer than the camera's near clip plane
        float minClipDist = headCamera != null ? headCamera.nearClipPlane + 0.02f : 0.05f;
        float dist = Mathf.Max(focusPointDistance, minClipDist);

        redDot.localScale = Vector3.one * focusPointSize;
        greenDot.localScale = Vector3.one * focusPointSize;
        redDot.localPosition = new Vector3(0f, 0f, dist);

        // Place green dot along the rig's neutral forward direction from the eyes
        greenDot.position = head.position + transform.forward * dist;
        // Billboarding the green dot to face the headset prevents edge-distortion at steep angles
        greenDot.rotation = Quaternion.LookRotation(greenDot.position - head.position, transform.up);
    }

    private Transform CreateDotQuad(string name, Transform parent, Material mat)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Destroy(quad.GetComponent<Collider>());

        quad.transform.SetParent(parent, false);
        quad.transform.localScale = Vector3.one * focusPointSize;

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return quad.transform;
    }

    private Material CreateOverlayMaterial(Color color, int renderQueue)
    {
        // UI/Default natively exposes "unity_GUIZTestMode" to bypass depth testing on 3D meshes
        Shader shader = Shader.Find("UI/Default");
        Material mat = new Material(shader)
        {
            mainTexture = dotTexture,
            color = color,
            renderQueue = renderQueue
        };

        // Set both property names so it works across Built-in, URP, and UI shaders
        mat.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
        mat.SetInt("_ZTest", (int)CompareFunction.Always);
        mat.SetInt("_ZWrite", 0);
        return mat;
    }

    private Texture2D CreateCircleTexture(int size, float outlineFraction = 0.2f)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        float outerRadius = (size * 0.5f) - 1f; // Leave a 1px transparent border to prevent edge clamping artifacts
        float innerRadius = outerRadius * (1f - Mathf.Clamp(outlineFraction, 0.05f, 0.5f));
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);

                // Smooth outer edge (alpha fades from 1 inside the dot to 0 outside)
                float outerAlpha = Mathf.Clamp01(outerRadius - dist + 0.5f);

                // Smooth inner transition (1 = white inner fill, 0 = black outer border)
                float innerFill = Mathf.Clamp01(innerRadius - dist + 0.5f);

                // Black (0,0,0) multiplied by material.color stays black; White (1,1,1) becomes red/green
                tex.SetPixel(x, y, new Color(innerFill, innerFill, innerFill, outerAlpha));
            }
        }

        tex.Apply();
        return tex;
    }
    private void OnDestroy()
    {
        if (dotTexture != null) Destroy(dotTexture);
        if (redMat != null) Destroy(redMat);
        if (greenMat != null) Destroy(greenMat);
    }
}