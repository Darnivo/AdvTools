using Unity.MLAgents;
using UnityEngine;

public class MazeAcademy : MonoBehaviour
{
    public int LessonMazeSize { get; private set; } = 5;

    private void Start()
    {
        Academy.Instance.OnEnvironmentReset += OnEnvironmentReset;
        UpdateMazeParameters();
    }

    private void OnEnvironmentReset()
    {
        UpdateMazeParameters();
        ResetEnvironments();
    }

    private void UpdateMazeParameters()
    {
        var envParams = Academy.Instance.EnvironmentParameters;
        LessonMazeSize = Mathf.FloorToInt(envParams.GetWithDefault("maze_size", 5f));
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