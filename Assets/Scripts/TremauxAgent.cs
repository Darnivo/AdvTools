using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TremauxAgent : AgentBase
{
    [Header("Trémaux Settings")]
    public Color backtrackColor = Color.red;
    
    private Dictionary<Vector2Int, int> visitCounts = new Dictionary<Vector2Int, int>();
    private Stack<Vector2Int> pathHistory = new Stack<Vector2Int>();

    public override void StartJourney()
    {
        if(!maze) maze = FindObjectOfType<MazeGenerator>();
        StartCoroutine(SolveMaze());
    }

    private IEnumerator SolveMaze()
    {
        isSolving = true;
        currentPosition = Vector2Int.zero;
        visitCounts.Clear();
        pathHistory.Clear();
        
        while (currentPosition != new Vector2Int(maze.height-1, maze.width-1))
        {
            yield return new WaitForSeconds(stepDelay);
            var next = GetNextDirection();
            MoveStep(next);
        }
        
        Debug.Log("Maze solved!");
        OnMazeSolved?.Invoke();
        isSolving = false;
    }

    private Vector2Int GetNextDirection()
    {
        var neighbors = GetAvailableDirections();
        var leastVisited = GetLeastVisitedDirection(neighbors);
        
        // Trémaux marking rules
        visitCounts.TryGetValue(currentPosition, out int count);
        visitCounts[currentPosition] = count + 1;
        
        return leastVisited;
    }

    private List<Vector2Int> GetAvailableDirections()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        int x = currentPosition.x;
        int y = currentPosition.y;

        // Check North (Z+)
        if(!maze.grid[x,y].bottomWall && y < maze.width-1)
            validMoves.Add(new Vector2Int(x, y+1));
        
        // Check East (X+)
        if(!maze.grid[x,y].rightWall && x < maze.height-1)
            validMoves.Add(new Vector2Int(x+1, y));
        
        // Check South (Z-)
        if(y > 0 && !maze.grid[x,y-1].bottomWall)
            validMoves.Add(new Vector2Int(x, y-1));
        
        // Check West (X-)
        if(x > 0 && !maze.grid[x-1,y].rightWall)
            validMoves.Add(new Vector2Int(x-1, y));

        return validMoves;
    }

    private Vector2Int GetLeastVisitedDirection(List<Vector2Int> directions)
    {
        Vector2Int best = currentPosition;
        int minVisits = int.MaxValue;

        foreach(var dir in directions)
        {
            visitCounts.TryGetValue(dir, out int visits);
            if(visits < minVisits)
            {
                minVisits = visits;
                best = dir;
            }
        }

        return best;
    }

    private void MoveStep(Vector2Int direction)
    {
        pathHistory.Push(currentPosition);
        MoveTo(direction);
        
        // Visual feedback when backtracking
        if(visitCounts.ContainsKey(direction) && visitCounts[direction] > 0)
            trail.startColor = backtrackColor;
        else
            trail.startColor = Color.white;
    }
}