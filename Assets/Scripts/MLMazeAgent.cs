using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class MLMazeAgent : AgentBase
{
    private Vector2Int targetPos;
    private Vector2Int startPos;
    private HashSet<Vector2Int> visitedPositions;

    // How big the local vision range is (e.g., 1 => 3x3 vision, 2 => 5x5 vision)
    private int visionRadius = 3;

    public override void Initialize()
    {
        base.Initialize();

        Transform environmentParent = transform.parent;
        maze = environmentParent.GetComponentInChildren<MazeGenerator>();

        if (maze == null)
        {
            Debug.LogError("MazeGenerator not found in environment hierarchy!");
            return;
        }

        startPos = Vector2Int.zero;
        targetPos = new Vector2Int(maze.width - 1, maze.height - 1);

        transform.localPosition = new Vector3(startPos.x, 0.5f, startPos.y);
    }

    public override void StartJourney()
    {
        // Controlled by ML-Agents
    }

    public override void OnEpisodeBegin()
    {
        currentPosition = startPos;
        transform.localPosition = new Vector3(startPos.x, 0.5f, startPos.y);

        if (visitedPositions == null)
            visitedPositions = new HashSet<Vector2Int>();
        else
            visitedPositions.Clear();

        visitedPositions.Add(currentPosition);

        StatsRecorder.Instance.StartRecording(this);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((targetPos.x - currentPosition.x) / (float)maze.height);
        sensor.AddObservation((targetPos.y - currentPosition.y) / (float)maze.width);

        // Add directional observations (normalized)
        Vector2Int dirToTarget = targetPos - currentPosition;
        Vector2 dirToTargetNormalized = ((Vector2)dirToTarget).normalized;
        sensor.AddObservation(dirToTargetNormalized.x);
        sensor.AddObservation(dirToTargetNormalized.y);

        for (int dx = -visionRadius; dx <= visionRadius; dx++)
        {
            for (int dy = -visionRadius; dy <= visionRadius; dy++)
            {
                Vector2Int checkPos = currentPosition + new Vector2Int(dx, dy);

                bool isWall = true;
                bool isVisited = false;

                if (checkPos.x >= 0 && checkPos.x < maze.height &&
                    checkPos.y >= 0 && checkPos.y < maze.width)
                {
                    isWall = !CanMoveTo(checkPos);
                    isVisited = visitedPositions.Contains(checkPos);
                }

                sensor.AddObservation(isWall ? 1.0f : 0.0f);
                sensor.AddObservation(isVisited ? 1.0f : 0.0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var move = actions.DiscreteActions[0] switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            3 => Vector2Int.left,
            _ => Vector2Int.zero
        };

        if (CanMove(move))
        {
            Vector2Int nextPosition = currentPosition + move;

            // Progressive penalty for revisits
            float revisitPenalty = visitedPositions.Contains(nextPosition) ? -0.1f : 0f;
            AddReward(revisitPenalty);

            // Distance-based reward
            float prevDist = Vector2Int.Distance(currentPosition, targetPos);
            float newDist = Vector2Int.Distance(nextPosition, targetPos);
            AddReward((prevDist - newDist) * 0.1f); // Scale as needed

            MoveStep(nextPosition);
            visitedPositions.Add(currentPosition);

            AddReward(-0.01f); // Small step penalty
        }
    }

    private bool CanMove(Vector2Int direction)
    {
        int x = currentPosition.x;
        int y = currentPosition.y;

        return direction switch
        {
            Vector2Int v when v == Vector2Int.up =>
                y < maze.width - 1 && !maze.grid[x, y].bottomWall,
            Vector2Int v when v == Vector2Int.right =>
                x < maze.height - 1 && !maze.grid[x, y].rightWall,
            Vector2Int v when v == Vector2Int.down =>
                y > 0 && !maze.grid[x, y - 1].bottomWall,
            Vector2Int v when v == Vector2Int.left =>
                x > 0 && !maze.grid[x - 1, y].rightWall,
            _ => false
        };
    }

    private bool CanMoveTo(Vector2Int target)
    {
        Vector2Int diff = target - currentPosition;
        return CanMove(diff);
    }

    protected void MoveStep(Vector2Int newPos)
    {
        MoveTo(newPos);
        if (newPos == targetPos)
        {
            AddReward(1.0f);
            NotifySuccess();
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = -1;

        if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 3;
    }

    protected override void OnDestroy()
    {
        if (isSolving)
            StatsRecorder.Instance.FinalizeRecording("Aborted");
        base.OnDestroy();
    }
}