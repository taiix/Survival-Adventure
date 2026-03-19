using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldCreator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
      
        WorldCreator worldCreator = (WorldCreator)target;

        EditorGUILayout.Space();
        if (DrawDefaultInspector())
        {
            worldCreator.Generate();
        }

        if (GUILayout.Button("Generate World"))
        {
            worldCreator.Generate();
        }

        if (GUILayout.Button("Clear World"))
        {
            worldCreator.ClearWorld();
        }
    }
}
