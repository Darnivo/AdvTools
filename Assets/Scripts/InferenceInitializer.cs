using UnityEngine;
using Unity.MLAgents;

public class InferenceInitializer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] MLMazeAgent agent;

    void Start()
    {
        if (mazeGenerator == null || agent == null)
        {
            Debug.LogError("InferenceInitializer needs references to MazeGenerator & MLMazeAgent");
            return;
        }
        
        // 1) Build the actual maze geometry + data
        mazeGenerator.GenerateAtParentPosition();
        // 2) Manually call the agent’s episode begin logic
        agent.OnEpisodeBegin();
        // 3) (Optional) force its first decision immediately
        agent.RequestDecision();
    }
}
