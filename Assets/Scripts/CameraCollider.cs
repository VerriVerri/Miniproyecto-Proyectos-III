using UnityEngine;
using Unity.Cinemachine;

public class CameraCollider : MonoBehaviour
{
    public CinemachineCamera cam;
    public GameObject cameraPivot; //The object whose collider will spawn 
    public Vector3 cameraOffset;
    public float collisionOffset;
    public float forwardOffset;
    public LayerMask collidesWithCamera;

    public Camera cam1;

    public float lineLength = 40f;// Length of the gizmo line

    void OnDrawGizmos()
    {
        if (Camera.main == null)
            return;

        // Get the camera's position and forward direction
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 endPosition = startPosition + Camera.main.transform.forward * lineLength;

        // Set the gizmo color
        Gizmos.color = Color.red;

        // Draw the line
        Gizmos.DrawLine(startPosition, endPosition);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam1 = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
