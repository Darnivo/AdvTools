using Unity.MLAgents;
using UnityEngine;

public class MazeAcademy : MonoBehaviour
{
    public int LessonMazeSize { get; private set; } = 5;

    private void Start()
    {
        UpdateMazeSize();
        ResetEnvironments();
    }

    private void UpdateMazeSize()
    {
        // Get value from training config or set default
        LessonMazeSize = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("maze_size", 5);
    }

    private void ResetEnvironments()
    {
        var generators = FindObjectsByType<MazeGenerator>(FindObjectsSortMode.None);
        foreach (var gen in generators)
        {
            gen.width = LessonMazeSize;
            gen.height = LessonMazeSize;
            gen.GenerateAtParentPosition();
        }
    }
}
