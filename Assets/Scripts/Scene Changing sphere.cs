using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class sceneChangingSphere : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private bool leftControllerInside;
    private bool rightControllerInside;
    private bool sceneChanging;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftController"))
            leftControllerInside = true;

        if (other.CompareTag("RightController"))
            rightControllerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftController"))
            leftControllerInside = false;

        if (other.CompareTag("RightController"))
            rightControllerInside = false;
    }

    void Update()
    {
        if (sceneChanging)
            return;

        if (leftControllerInside && IsTriggerPressed(XRNode.LeftHand))
            ChangeScene();

        if (rightControllerInside && IsTriggerPressed(XRNode.RightHand))
            ChangeScene();
    }

    bool IsTriggerPressed(XRNode hand)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(hand);

        return device.isValid &&
               device.TryGetFeatureValue(
                   CommonUsages.triggerButton, out bool pressed
               ) &&
               pressed;
    }

    void ChangeScene()
    {
        sceneChanging = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
