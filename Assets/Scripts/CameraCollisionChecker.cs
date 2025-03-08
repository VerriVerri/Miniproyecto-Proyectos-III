using UnityEngine;

public class CameraCollisionChecker : MonoBehaviour
{
    //The main camera's corners
    public Vector3? topLeft { get; private set; }
    public Vector3? topRight { get; private set; }
    public Vector3? bottomLeft{ get; private set; }
    public Vector3? bottomRight { get; private set; }

    private Vector3? collisionPoint;
    /// <summary>
    /// Checks if there are any objects colliding with the camera 
    /// </summary>
    /// <param name="camera"> The camera whose collisions will be checked</param>
    public void CheckCameraCorners(Camera camera)
    {
        topLeft = camera.ViewportToWorldPoint(new Vector3(0, 1, camera.nearClipPlane));
        topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
        bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        bottomRight = camera.ViewportToWorldPoint(new Vector3(1, 0, camera.nearClipPlane));
    }
    public void CheckCameraCollisions(Camera camera)
    {

    }
    public void CheckCameraCollisions()
    {

    }


    #region drawing stuff


    /// <summary>
    /// Draw a line between two points, and if a collision is detected, change the color to red.
    /// </summary>
    private void DrawLine(Vector3 from, Vector3 to, Vector3? collision)
    {
        // Check if there was a collision
        if (collision.HasValue)
        {
            Debug.DrawLine(from, collision.Value, Color.red);  // Draw red line if collision detected
            Debug.DrawLine(collision.Value, to, Color.red);  // Draw red line to the next point
        }
        else
        {
            Debug.DrawLine(from, to, Color.yellow);  // Otherwise, draw in yellow
        }
    }

    #endregion
    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"> The vector from which the </param>
    /// <param name="to"></param>
    /// <param name="collides">The layers which count as collisions</param>
    public Vector3? Raycast(Vector3 from, Vector3 to, LayerMask collides)
    {
        Vector3 direction = to - from;

        RaycastHit hit;

        if (Physics.Raycast(from, direction, out hit, direction.magnitude, collides))
        {
            return hit.point;
        }
        else
        {
            return null;
        }
    }
}
