using UnityEngine;
using Unity.MLAgents;

public abstract class AgentBase : Agent
{
    [Header("Base Settings")]
    public float stepDelay = 0.1f;
    public bool showPath = true;
    public GameObject pathMarkerPrefab;

    [HideInInspector] public Vector2Int currentPosition;
    // [HideInInspector] public MazeGenerator maze;
     public MazeGenerator maze;


    protected Transform markerParent;
    public bool isSolving = false;
    protected TrailRenderer trail;

    public System.Action OnMazeSolved;

    public event System.Action OnSuccess;

    protected override void Awake()
    {
        base.Awake();
        trail = GetComponent<TrailRenderer>();
        trail.enabled = showPath;
        markerParent = new GameObject("PathMarkers").transform;
    }

    // void Awake()
    // {
    //     trail = GetComponent<TrailRenderer>();
    //     trail.enabled = showPath;
    //     markerParent = new GameObject("PathMarkers").transform;
    // }

    public abstract void StartJourney();

    protected void MoveTo(Vector2Int newPosition)
    {
        currentPosition = newPosition;
        transform.localPosition = new Vector3(newPosition.x, 0.5f, newPosition.y);

        StatsRecorder.Instance?.RecordStep();

        if (showPath) AddPathMarker();
    }

    private void AddPathMarker()
    {
        if (pathMarkerPrefab)
            Instantiate(pathMarkerPrefab,
                      new Vector3(currentPosition.x, 0.1f, currentPosition.y),
                      Quaternion.identity,
                      markerParent);
    }

    public void ToggleVisualization(bool state)
    {
        trail.enabled = state;
        markerParent.gameObject.SetActive(state);
    }

    protected void NotifySuccess()
    {
        StatsRecorder.Instance?.FinalizeRecording("Success");
        Debug.Log("Maze reached");
        isSolving = false;
        OnSuccess?.Invoke();
    }

    public void StopJourney()
    {
        StopAllCoroutines();
        isSolving = false;
    }
        protected virtual void OnDestroy()
    {
        if(isSolving)
            StatsRecorder.Instance?.FinalizeRecording("Aborted");
    }
}