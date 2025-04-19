using UnityEngine;
using Unity.MLAgents;

public class MLSimulationController : MonoBehaviour
{
    [Header("Training Settings")]
    public int mazeSize = 10;
    public bool randomSizes = false;
    public Vector2Int sizeRange = new Vector2Int(5, 11);

    void Start()
    {
        var academy = FindObjectOfType<MazeAcademy>();
        var generators = FindObjectsOfType<MazeGenerator>();

        foreach (var gen in generators)
        {
            gen.width = academy.LessonMazeSize;
            gen.height = academy.LessonMazeSize;
            gen.GenerateAtParentPosition();
        }
    }
}