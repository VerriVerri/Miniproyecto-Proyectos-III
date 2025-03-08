using System.Collections;
using UnityEngine;

public class FlowarlColliderHandler : MonoBehaviour
{
    public Transform player;  // Reference to the player’s transform
    public float detectionRadius = 10f;  // Distance to check for nearby colliders
    public LayerMask cameraFlowarlLayer;  // Layer for "CameraFlowarl" objects
    public float deactivationDelay = 5f;  // Time before the collider is reactivated if no player is nearby

    private void Update()
    {

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, cameraFlowarlLayer);

        foreach (Collider col in colliders)
        {

            if (col.gameObject.layer == cameraFlowarlLayer)
            {
                Debug.Log("Collided with" + col.gameObject);

                if (player.position.y > col.bounds.max.y)
                {

                    col.gameObject.SetActive(false);

                    // Start a coroutine to reactivate the collider if no player is detected nearby
                    StartCoroutine(ReactivateCollider(col.gameObject));
                }
                else
                {
                    // Make sure the collider is active if the player is not above
                    col.gameObject.SetActive(true);
                }
            }
        }
    }

    private IEnumerator ReactivateCollider(GameObject collider)
    {
        // Wait for a certain period before reactivating the collider
        yield return new WaitForSeconds(deactivationDelay);

        // Reactivate the collider if it is still deactivated and no player is nearby
        if (!collider.activeSelf && !IsPlayerNearby(collider))
        {
            collider.SetActive(true);
        }
    }

    private bool IsPlayerNearby(GameObject collider)
    {
        // Check if the player is within detection radius of the collider
        return Vector3.Distance(player.position, collider.transform.position) <= detectionRadius;
    }
}

