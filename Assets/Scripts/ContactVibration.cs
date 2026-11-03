using UnityEngine;
using Oculus.Haptics; 

public class ContactVibration : MonoBehaviour
{
    [Header("Configuration de la main")]
    // Choisissez dans l'Inspector s'il s'agit de la main gauche ou droite
    public OVRInput.Controller controllerType = OVRInput.Controller.RTouch;

    [Header("Paramètres Haptiques pour le mur")]
    [Range(0f, 1f)] public float frequency = 0.5f;
    [Range(0f, 1f)] public float amplitude = 0.5f;

    [Header("Tag à détecter pour le mur")]
    public string wallTag = "Wall";

    private bool isTouchingWall = false;
   

    [Header("Configuration Haptique pour la sortie")]
    [SerializeField] private HapticClip exitHapticClip;

    [Header("Tag à détecter pour la sortie")]
    public string exitTag = "Exit";

    private bool isTouchingExit = false;

    private HapticClipPlayer player;
    private Controller targetHapticController; // Détermine le côté (gauche/droite) pour l'API Meta

    void Awake()
    {
        if (exitHapticClip != null)
        {
            // Initialise le lecteur avec le clip sélectionné
            player = new HapticClipPlayer(exitHapticClip);
        }

        // Convertit le type de contrôleur OVRInput vers le type attendu par l'API Meta Haptics
        if (controllerType == OVRInput.Controller.LTouch)
        {
            targetHapticController = Controller.Left;
        }
        else
        {
            targetHapticController = Controller.Right;
        }
    }

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

        // LOGIQUE MODIFIÉE : Déclenchement du clip haptique de sortie en boucle
        if (other.CompareTag(exitTag))
        {
            isTouchingExit = true;

            if (player != null)
            {
                player.isLooping = true; // Active le bouclage automatique
                player.Play(targetHapticController); // Lance la vibration haute fidélité
            }
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

        // LOGIQUE AJOUTÉE : Arrêt propre de l'effet haptique de sortie
        if (other.CompareTag(exitTag))
        {
            isTouchingExit = false;

            if (player != null)
            {
                player.Stop(); // Arrête la boucle immédiatement
            }
        }
    }

    private void OnDisable()
    {
        // Sécurité : coupe la vibration si le script ou la main est désactivée
        OVRInput.SetControllerVibration(0f, 0f, controllerType);
        
        // Sécurité ajoutée pour la sortie
        if (player != null)
        {
            player.Stop();
        }
    }

    void OnDestroy()
    {
        // Bonne pratique : Libérer les ressources du player
        player?.Dispose();
    }
}
