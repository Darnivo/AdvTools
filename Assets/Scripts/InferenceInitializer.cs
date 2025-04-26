using UnityEngine;
using Unity.MLAgents;
using System.Collections;


public class InferenceInitializer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] MLMazeAgent agent;
    private Coroutine _stepLoop;
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

        agent.OnSuccess += HandleAgentSuccess;
        _stepLoop = StartCoroutine(DecisionLoop());
        // 3) (Optional) force its first decision immediately
        // agent.RequestDecision();

    }

    IEnumerator DecisionLoop()
    {
        while (true)
        {
            agent.RequestDecision();
            yield return new WaitForSeconds(agent.stepDelay);
        }
    }

    private void HandleAgentSuccess()
    {
        if (_stepLoop != null)
        {
            StopCoroutine(_stepLoop);
            _stepLoop = null;
            Debug.Log("Agent reached the target. Stopping inference.");
        }
    }
}
