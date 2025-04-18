using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TremauxAgent : AgentBase
{
    [Header("Trémaux Settings")]
    public Color backtrackColor = Color.red;

    // Edge marks: 0 = unmarked, 1 = once, 2 = twice
    private Dictionary<(Vector2Int, Vector2Int), int> edgeMarks;

    public override void StartJourney()
    {
        if (!maze) maze = FindObjectOfType<MazeGenerator>();
        edgeMarks = new Dictionary<(Vector2Int, Vector2Int), int>();
        StartCoroutine(SolveMaze());
    }

    private IEnumerator SolveMaze()
    {
        isSolving = true;
        currentPosition = Vector2Int.zero;
        yield return null; // ensure initialization

        while (currentPosition != new Vector2Int(maze.height - 1, maze.width - 1))
        {
            yield return new WaitForSeconds(stepDelay);
            Vector2Int next = GetNextDirection();
            MoveStep(next);
        }

        Debug.Log("Maze solved!");
        isSolving = false;
        NotifySuccess();
        OnMazeSolved?.Invoke();
    }

    private Vector2Int GetNextDirection()
    {
        List<Vector2Int> neighbors = GetAvailableDirections();
        if (neighbors.Count == 0)
            return currentPosition;  // no move possible

        // Partition neighbors by edge mark count
        List<Vector2Int> unmarked = new List<Vector2Int>();
        List<Vector2Int> onceMarked = new List<Vector2Int>();

        foreach (var n in neighbors)
        {
            int marks = GetEdgeMark(currentPosition, n);
            if (marks == 0) unmarked.Add(n);
            else if (marks == 1) onceMarked.Add(n);
        }

        Vector2Int chosen;
        if (unmarked.Count > 0)
            chosen = unmarked[Random.Range(0, unmarked.Count)];
        else if (onceMarked.Count > 0)
            chosen = onceMarked[Random.Range(0, onceMarked.Count)];
        else
            chosen = neighbors[Random.Range(0, neighbors.Count)];

        // Mark the edge between current and chosen
        IncrementEdgeMark(currentPosition, chosen);
        return chosen;
    }

    private List<Vector2Int> GetAvailableDirections()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        int x = currentPosition.x;
        int y = currentPosition.y;

        // North
        if (!maze.grid[x, y].bottomWall && y < maze.width - 1)
            validMoves.Add(new Vector2Int(x, y + 1));
        // East
        if (!maze.grid[x, y].rightWall && x < maze.height - 1)
            validMoves.Add(new Vector2Int(x + 1, y));
        // South
        if (y > 0 && !maze.grid[x, y - 1].bottomWall)
            validMoves.Add(new Vector2Int(x, y - 1));
        // West
        if (x > 0 && !maze.grid[x - 1, y].rightWall)
            validMoves.Add(new Vector2Int(x - 1, y));

        return validMoves;
    }

    private int GetEdgeMark(Vector2Int a, Vector2Int b)
    {
        var key = a.x < b.x || (a.x == b.x && a.y < b.y) ? (a, b) : (b, a);
        edgeMarks.TryGetValue(key, out int count);
        return count;
    }

    private void IncrementEdgeMark(Vector2Int a, Vector2Int b)
    {
        var key = a.x < b.x || (a.x == b.x && a.y < b.y) ? (a, b) : (b, a);
        edgeMarks[key] = GetEdgeMark(a, b) + 1;
    }

    private void MoveStep(Vector2Int nextPosition)
    {
        Vector2Int prev = currentPosition;
        MoveTo(nextPosition);

        // Visual feedback when backtracking (edge crossed more than once)
        int marks = GetEdgeMark(prev, nextPosition);
        trail.startColor = (marks > 1) ? backtrackColor : Color.white;
    }

    protected void NotifySuccess()
    {
        StatsRecorder.Instance?.FinalizeRecording("Success");
    }
}
