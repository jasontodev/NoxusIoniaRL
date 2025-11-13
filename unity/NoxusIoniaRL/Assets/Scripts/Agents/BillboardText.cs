using UnityEngine;

namespace NoxusIoniaRL.Agents
{
    /// <summary>
    /// Makes a GameObject always face the camera (billboard effect).
    /// Used for health text display above agents.
    /// </summary>
    public class BillboardText : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                // Make text face the camera
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}

