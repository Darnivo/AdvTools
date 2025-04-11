using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MazeGenerator generator = (MazeGenerator)target;
        if (GUILayout.Button("Generate Maze"))
        {
            Undo.RecordObject(generator, "Generate Maze");
            generator.GenerateMaze();
        }
    }
}