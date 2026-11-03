using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip footstepsClip;
    [SerializeField] private AudioClip bodyThudClip;

    private const string WallTag = "Wall";
    private const float MinMoveSpeed = 0.15f;
    private const float MaxFootstepVolume = 0.4f;
    private const float ThudVolume = 0.65f;
    private const float CollisionCooldown = 0.45f;
    private const float MinImpactVelocity = 0.35f;

    private CharacterController controller;
    private AudioSource footstepSource;
    private AudioSource collisionSource;

    private Vector3 lastPosition;
    private Vector3 velocity;
    private float targetVolume;
    private float lastThudTime = -10f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.spatialBlend = 0f;
        footstepSource.clip = footstepsClip;

        collisionSource = gameObject.AddComponent<AudioSource>();
        collisionSource.playOnAwake = false;
        collisionSource.spatialBlend = 0.3f;
    }

    private void Update()
    {
        Vector3 displacement = transform.position - lastPosition;
        displacement.y = 0f;

        float dt = Time.deltaTime;
        float speed = dt > 0.0001f ? displacement.magnitude / dt : 0f;
        velocity = dt > 0.0001f ? displacement / dt : Vector3.zero;
        lastPosition = transform.position;

        bool isWalking = speed > MinMoveSpeed && controller.isGrounded;
        targetVolume = isWalking ? MaxFootstepVolume : 0f;

        if (isWalking)
        {
            footstepSource.pitch = Mathf.Lerp(0.92f, 1.08f, Mathf.Clamp01(speed / 3f));
            if (!footstepSource.isPlaying && footstepSource.clip != null)
                footstepSource.Play();
        }

        if (footstepSource.clip != null)
        {
            footstepSource.volume = Mathf.MoveTowards(footstepSource.volume, targetVolume, 5f * Time.deltaTime);
            if (footstepSource.volume <= 0.001f && footstepSource.isPlaying)
                footstepSource.Pause();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag(WallTag)) return;
        if (Mathf.Abs(hit.normal.y) > 0.6f) return;
        if (Time.time - lastThudTime < CollisionCooldown) return;

        float impactSpeed = Vector3.Dot(velocity, -hit.normal);
        if (impactSpeed < MinImpactVelocity) return;

        if (bodyThudClip != null)
        {
            collisionSource.pitch = Random.Range(0.92f, 1.06f);
            collisionSource.PlayOneShot(bodyThudClip, ThudVolume);
        }

        lastThudTime = Time.time;
    }
}
