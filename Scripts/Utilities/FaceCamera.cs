using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera targetCamera;

    private void Start() { targetCamera = Camera.main; }

    private void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.LookAt(transform.position + targetCamera.transform.forward);
        }
    }
}
