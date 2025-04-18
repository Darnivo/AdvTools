using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public MazeGenerator mazeGenerator;
    public TremauxAgent tremauxAgent;
    
    void Start()
    {
        if(mazeGenerator && tremauxAgent)
        {
            mazeGenerator.GenerateMaze();
            StatsRecorder.Instance.StartRecording(tremauxAgent);
            tremauxAgent.StartJourney();
        }
    }
    
    public void SetStepDelay(float delay)
    {
        tremauxAgent.stepDelay = delay;
    }
}