using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyController))]
public class EnemyControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnemyController enemyController = (EnemyController)target;

        // Add the "Mark" button to the inspector
        if (GUILayout.Button("On Marked"))
        {
            enemyController.OnMarked();  // Calls the OnMarked() method
        }

        // Add the "Unmark" button to the inspector
        if (GUILayout.Button("On Unmarked"))
        {
            enemyController.OnUnmarked();  // Calls the OnUnmarked() method
        }
    }

}

