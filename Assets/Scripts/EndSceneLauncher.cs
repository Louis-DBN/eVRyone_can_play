using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneLauncher : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("player"))
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("EndSceneLauncher: no scene name set in Inspector.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
