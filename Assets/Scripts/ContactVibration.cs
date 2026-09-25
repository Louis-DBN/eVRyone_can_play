using UnityEngine;

public class ContactVibration : MonoBehaviour
{
    [Header("Configuration de la main")]
    // Choisissez dans l'Inspector s'il s'agit de la main gauche ou droite
    public OVRInput.Controller controllerType = OVRInput.Controller.RTouch;

    [Header("Paramètres Haptiques")]
    [Range(0f, 1f)] public float frequency = 0.5f;
    [Range(0f, 1f)] public float amplitude = 0.5f;

    [Header("Tag à détecter")]
    public string wallTag = "Wall";

    private bool isTouchingWall = false;

    private void Update()
    {   
        // Envoie la vibration en continu tant que la main touche un mur
        if (isTouchingWall)
        {
            OVRInput.SetControllerVibration(frequency, amplitude, controllerType);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("COLIDE");
        // Si l'objet touché possède le tag configuré (ex: "Wall")
        if (other.CompareTag(wallTag))
        {
            isTouchingWall = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(wallTag))
        {
            isTouchingWall = false;
            // Arrête immédiatement la vibration
            OVRInput.SetControllerVibration(0f, 0f, controllerType);
        }
    }

    private void OnDisable()
    {
        // Sécurité : coupe la vibration si le script ou la main est désactivée
        OVRInput.SetControllerVibration(0f, 0f, controllerType);
    }
}
