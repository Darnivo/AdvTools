using UnityEngine;

public class AStarSimulationController : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public AStarAgent aStarAgent;
    
    void Start()
    {
        if(mazeGenerator && aStarAgent)
        {
            mazeGenerator.GenerateMaze();
            StatsRecorder.Instance.StartRecording(aStarAgent);
            aStarAgent.StartJourney();
        }
    }
    
    public void SetStepDelay(float delay)
    {
        aStarAgent.stepDelay = delay;
    }
}