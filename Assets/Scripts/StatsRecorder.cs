using System;
using System.IO;
using System.Collections;
using System.Text;
using UnityEngine;

public class StatsRecorder : MonoBehaviour
{
    [Header("Core Settings")]
    public bool enableRecording = true;
    public float timeoutSeconds = 60f;
    
    [Header("File Settings")]
    public string folderName = "MazeStats";
    public bool separateFilesPerRun = true;

    private StringBuilder currentData;
    private string filePath;
    private float startTime;
    private int totalSteps;
    private bool isSuccessful;
    private Coroutine recordingCoroutine;
    private AgentBase trackedAgent;
    private MazeGenerator maze;

    public static StatsRecorder Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void StartRecording(AgentBase agent)
    {
        if (!enableRecording) return;

        trackedAgent = agent;
        maze = FindFirstObjectByType<MazeGenerator>();
        startTime = Time.time;
        totalSteps = 0;
        isSuccessful = false;
        
        InitializeFile();
        StartCoroutine(TimeoutWatchdog());
        recordingCoroutine = StartCoroutine(ContinuousRecording());
    }

    private void InitializeFile()
    {
        currentData = new StringBuilder();
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var agentType = trackedAgent.GetType().Name;
        
        // Create header block
        currentData.AppendLine($"AgentType,{agentType}");
        currentData.AppendLine($"MazeDimensions,{maze.height},{maze.width}");
        currentData.AppendLine($"MazeSeed,{maze.seed}");
        currentData.AppendLine($"StartTime,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        currentData.AppendLine("FrameTime,TotalTime,Steps,FPS,MemoryUsage,AvgFPS,MinFPS,MaxFPS");

        if (separateFilesPerRun)
        {
            var directory = Path.Combine(Application.persistentDataPath, folderName);
            Directory.CreateDirectory(directory);
            filePath = Path.Combine(directory, $"{timestamp}_{agentType}.csv");
        }
    }

    private IEnumerator ContinuousRecording()
    {
        float maxFPS = 0;
        float minFPS = float.MaxValue;
        float totalFPS = 0;
        int frameCount = 0;
        bool skipFirstFrame = true;

        while (true)
        {
            yield return new WaitForSecondsRealtime(0.1f); // Sample every 100ms

            if (skipFirstFrame)
        {
            skipFirstFrame = false;
            continue;
        }

            // Current frame metrics
            float currentFPS = 1f / Time.unscaledDeltaTime;
            long memoryUsage = System.GC.GetTotalMemory(false);
            float elapsed = Time.time - startTime;

            // Update FPS tracking
            maxFPS = Mathf.Max(maxFPS, currentFPS);
            minFPS = Mathf.Min(minFPS, currentFPS);
            totalFPS += currentFPS;
            frameCount++;

            currentData.AppendLine(
                $"{Time.time:F2}," +
                $"{elapsed:F2}," +
                $"{totalSteps}," +
                $"{currentFPS:F1}," +
                $"{memoryUsage / 1024}," + // KB
                $"{totalFPS / frameCount:F1}," +
                $"{minFPS:F1}," +
                $"{maxFPS:F1}"
            );
        }
    }

    private IEnumerator TimeoutWatchdog()
{
    yield return new WaitForSecondsRealtime(timeoutSeconds);
    if (!isSuccessful) 
    {
        FinalizeRecording("DNF");
        Debug.LogError($"Timeout after {timeoutSeconds} seconds!");
        
        // Stop the agent
        if(trackedAgent != null) 
        {
            trackedAgent.StopAllCoroutines();
            trackedAgent.isSolving = false;
        }
    }
}

    public void RecordStep()
    {
        if (!enableRecording) return;
        totalSteps++;
    }

    public void FinalizeRecording(string result)
{
    if (!enableRecording || currentData == null) return;

    // Stop all coroutines
    if (recordingCoroutine != null) 
    {
        StopCoroutine(recordingCoroutine);
        StopCoroutine(TimeoutWatchdog());
    }
    
    // Move final metrics to footer
    currentData.Insert(0, $"FinalResult,{result}\n");
    
    File.WriteAllText(filePath, currentData.ToString());
    Debug.Log($"Stats saved to: {filePath}");
    
    currentData = null;
    
    // Force Unity to flush the file write
    #if UNITY_EDITOR
    UnityEditor.AssetDatabase.Refresh();
    #endif
}

    void OnDestroy()
    {
        if (currentData != null)
        {
            FinalizeRecording("Aborted");
        }
    }
}