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

    public override void Initialize()
    {
        base.Initialize();

        // Get the parent environment object
        Transform environmentParent = transform.parent;

        // Find MazeGenerator in environment parent's children
        maze = environmentParent.GetComponentInChildren<MazeGenerator>();

        // Verify maze reference
        if (maze == null)
        {
            Debug.LogError("MazeGenerator not found in environment hierarchy!");
            return;
        }

        // Set positions relative to maze
        startPos = Vector2Int.zero;
        targetPos = new Vector2Int(maze.width - 1, maze.height - 1);

        // Set agent's local position to maze start
        transform.localPosition = new Vector3(
            startPos.x,
            0.5f,
            startPos.y
        );
    }

    public override void StartJourney()
    {
        // ML-Agents controls episode start
    }

    public override void OnEpisodeBegin()
    {
        currentPosition = startPos;
        // transform.position = new Vector3(startPos.x, 0.5f, startPos.y);
        transform.localPosition = new Vector3(startPos.x, 0.5f, startPos.y);

        if (visitedPositions == null)
        {
            visitedPositions = new HashSet<Vector2Int>();
        }
        else
        {
            visitedPositions.Clear();
        }
        visitedPositions.Add(currentPosition);

        StatsRecorder.Instance.StartRecording(this);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Relative target position (normalized)
        sensor.AddObservation((targetPos.x - currentPosition.x) / (float)maze.height);
        sensor.AddObservation((targetPos.y - currentPosition.y) / (float)maze.width);

        // Wall observations (4 directions)
        sensor.AddObservation(CanMove(Vector2Int.up));    // North
        sensor.AddObservation(CanMove(Vector2Int.right)); // East
        sensor.AddObservation(CanMove(Vector2Int.down));  // South
        sensor.AddObservation(CanMove(Vector2Int.left));  // West

        sensor.AddObservation(visitedPositions.Contains(currentPosition + Vector2Int.up) ? 1.0f : 0.0f);
        sensor.AddObservation(visitedPositions.Contains(currentPosition + Vector2Int.right) ? 1.0f : 0.0f);
        sensor.AddObservation(visitedPositions.Contains(currentPosition + Vector2Int.down) ? 1.0f : 0.0f);
        sensor.AddObservation(visitedPositions.Contains(currentPosition + Vector2Int.left) ? 1.0f : 0.0f);    
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
            // MoveStep(currentPosition + move);

            Vector2Int nextPosition = currentPosition + move;
            if (visitedPositions.Contains(nextPosition))
            {
                AddReward(-0.05f);
            }

            // float oldDist = Vector2Int.Distance(currentPosition, targetPos);
            MoveStep(currentPosition + move);
            // float newDist = Vector2Int.Distance(currentPosition, targetPos);
            // if (newDist < oldDist)
            // {
            //     AddReward(+0.01f); 
            // }
            visitedPositions.Add(currentPosition);


            AddReward(-0.01f); // Small penalty per step
        }
        else
        {
            AddReward(-0.1f); // Penalty for wall hit
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
        discreteActions[0] = -1; // Reset

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