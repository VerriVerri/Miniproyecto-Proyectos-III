using UnityEngine;

public class FloorColliderDuplicatorBecauseApparentlyCinemachineHatesMe : MonoBehaviour
{
    public LayerMask floorLayer; // Set this in the Inspector to match the Floor layer
    public float offsetY = 9.66f; // Adjustable offset in the Inspector
    public string trueFloorLayerName = "TrueFloor";

    void Start()
    {
        DuplicateFloorObjects();
    }

    void DuplicateFloorObjects()
    {
        GameObject trueFloorsParent = new GameObject("TrueFloors"); // Parent object for duplicates
        GameObject[] floorObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int trueFloorLayer = LayerMask.NameToLayer(trueFloorLayerName);

        foreach (GameObject obj in floorObjects)
        {
            if (((1 << obj.layer) & floorLayer) != 0) // Check if object is in the Floor layer
            {
                GameObject duplicate = Instantiate(obj, obj.transform.position + Vector3.down * offsetY, obj.transform.rotation);
                duplicate.name = obj.name + "_Duplicate";
                duplicate.transform.SetParent(trueFloorsParent.transform);
                duplicate.layer = trueFloorLayer;
            }
        }
    }
}
