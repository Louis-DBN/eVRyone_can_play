
using UnityEngine;
using UnityEngine.SceneManagement;

public class SphereGazeTrigger : MonoBehaviour
{
    [Header("Regard")]
    public Camera playerCamera;
    public float gazeDuration = 3f;
    public float maxDistance = 10f;

    [Header("Couleurs")]
    public Color startColor = Color.white;
    public Color endColor = Color.green;

    [Header("Changement de scène")]
    public string nextSceneName = "Scene2";

    private Renderer sphereRenderer;
    private float gazeTimer = 0f;
    private bool sceneChanging = false;

    void Start()
    {
        sphereRenderer = GetComponent<Renderer>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (sphereRenderer != null)
            sphereRenderer.material.color = startColor;
    }

    void Update()
    {
        if (sceneChanging || playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool isLookingAtSphere = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            isLookingAtSphere = hit.transform == transform;
        }

        if (isLookingAtSphere)
        {
            gazeTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                gazeTimer / gazeDuration
            );

            sphereRenderer.material.color = Color.Lerp(
                startColor,
                endColor,
                progress
            );

            if (progress >= 1f)
            {
                sceneChanging = true;
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            // Le joueur détourne le regard :
            // on réinitialise la progression.
            gazeTimer = 0f;

            sphereRenderer.material.color = startColor;
        }
    }
}
