using UnityEngine;
using Unity.MLAgents;

public class MLSimulationController : MonoBehaviour
{
    [Header("Training Settings")]
    public int mazeSize = 10;
    public bool randomSizes = false;
    public Vector2Int sizeRange = new Vector2Int(5, 10);

    void Start()
    {
        var generators = FindObjectsOfType<MazeGenerator>();
        foreach(var gen in generators)
        {
            // Configure maze generation
            gen.autoAdjustCamera = false;
            gen.seed = Random.Range(0, 10000);
            
            if(randomSizes)
            {
                gen.width = Random.Range(sizeRange.x, sizeRange.y);
                gen.height = gen.width;
            }
            else
            {
                gen.width = mazeSize;
                gen.height = mazeSize;
            }
            
            // Generate maze relative to environment parent
            gen.GenerateAtParentPosition();
        }
    }
}