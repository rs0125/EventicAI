using UnityEngine;

public class FaceCameraYOnly : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        Vector3 targetPos = transform.position + cam.forward;
        targetPos.y = transform.position.y; // lock Y-axis
        transform.LookAt(targetPos);
    }
}

