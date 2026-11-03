using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fire : MonoBehaviour
{

    public Transform orientation;
    public Transform position;
    public Transform leftLaunchPoint;
    public Transform rightLaunchPoint;
    
    public Rigidbody rg;

    private Vector3 direction;

    public float speed;


    [Header("Sounds and sfx")]
    public AudioClip[] collisionSfxs;

    
    // private vars
    private AudioSource audioSource;
    private float divide = 5f;

    private bool launched = false; 

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {

        bool mousePressed = UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        bool leftTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        bool rightTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if ((mousePressed || leftTrigger || rightTrigger) && !launched) {
            Transform launchPoint = leftTrigger ? leftLaunchPoint : rightLaunchPoint;
            Transform shotOrientation = launchPoint != null ? launchPoint : orientation;
            Transform shotPosition = launchPoint != null ? launchPoint : position;
            launched = true;
            direction = shotOrientation.forward;

            transform.rotation = shotOrientation.rotation;
            rg.position = shotPosition.position;

            rg.linearVelocity = direction.normalized * speed;
            rg.angularVelocity = Vector3.zero;

            audioSource.PlayOneShot(collisionSfxs[Random.Range(0, collisionSfxs.Length)], 1);
        }
    }

    private void FixedUpdate()
{
    if (launched)
    {
        rg.linearVelocity = direction.normalized * speed;
    }
}


    private void OnCollisionEnter(Collision collision) {
        PlayCollisionSfx(collision);
        launched = false; 
        //return to initial position

        rg.linearVelocity = Vector3.zero;
        rg.angularVelocity = Vector3.zero;
        
        rg.position = Vector3.zero;
        rg.rotation = Quaternion.identity;
    }

    private void PlayCollisionSfx(Collision collision)
    {
        if (!rg || !audioSource || collision==null)
        {
            // Can't play collision sfx
            return;
        }
        audioSource.pitch = Random.Range(0.85f, 1f);
        // the more the collisison is big, the more the sound velocity
        
        float volumeScale = (collision.relativeVelocity.magnitude / divide );
        AudioSource.PlayClipAtPoint(collisionSfxs[Random.Range(0, collisionSfxs.Length)],
            collision.GetContact(0).point);
    }
}
