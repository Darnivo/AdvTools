using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AStarAgent : AgentBase
{
    private List<Vector2Int> path;
    private Vector2Int startPos;
    private Vector2Int endPos;

    public override void StartJourney()
    {
        if (!maze) maze = FindFirstObjectByType<MazeGenerator>();
        startPos = Vector2Int.zero;
        endPos = new Vector2Int(maze.height - 1, maze.width - 1);
        StartCoroutine(SolveMaze());
    }

    private IEnumerator SolveMaze()
    {
        isSolving = true;
        currentPosition = startPos;
        path = FindPath(startPos, endPos);

        if (path == null)
        {
            Debug.LogError("No path found!");
            yield break;
        }

        foreach (var step in path)
        {
            yield return new WaitForSeconds(stepDelay);
            MoveTo(step);
        }

        Debug.Log("Maze solved with A*!");
        isSolving = false;
        NotifySuccess();
        OnMazeSolved?.Invoke();
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        var openSet = new PriorityQueue();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        // Initialize starting values
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);
        openSet.Enqueue(new Node(start, fScore[start]));

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            Vector2Int currentPos = currentNode.Position;

            // Early exit if we reach the target
            if (currentPos == target)
            {
                return ReconstructPath(cameFrom, currentPos);
            }

            closedSet.Add(currentPos);

            // Explore neighbors
            foreach (var neighbor in GetNeighbors(currentPos))
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[currentPos] + 1; // Cost between adjacent cells is 1

                // If this path to neighbor is better than previous ones
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = currentPos;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, target);

                    // Only add to open set if not already there
                    if (!openSet.ContainsPosition(neighbor))
                    {
                        openSet.Enqueue(new Node(neighbor, fScore[neighbor]));
                    }
                }
            }
        }

        // If we get here, no path exists
        return null;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = pos.x;
        int y = pos.y;

        // North
        if (!maze.grid[x, y].bottomWall && y < maze.width - 1)
            neighbors.Add(new Vector2Int(x, y + 1));
        // East
        if (!maze.grid[x, y].rightWall && x < maze.height - 1)
            neighbors.Add(new Vector2Int(x + 1, y));
        // South
        if (y > 0 && !maze.grid[x, y - 1].bottomWall)
            neighbors.Add(new Vector2Int(x, y - 1));
        // West
        if (x > 0 && !maze.grid[x - 1, y].rightWall)
            neighbors.Add(new Vector2Int(x - 1, y));

        return neighbors;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private class Node : IComparable<Node>
    {
        public Vector2Int Position { get; }
        public float Priority { get; }

        public Node(Vector2Int position, float priority)
        {
            Position = position;
            Priority = priority;
        }

        public int CompareTo(Node other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    private class PriorityQueue
    {
        private List<Node> elements = new List<Node>();
        private HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        public int Count => elements.Count;

        public void Enqueue(Node item)
        {
            elements.Add(item);
            positions.Add(item.Position);
            int childIndex = elements.Count - 1;
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (elements[childIndex].CompareTo(elements[parentIndex]) >= 0)
                    break;
                Node tmp = elements[childIndex];
                elements[childIndex] = elements[parentIndex];
                elements[parentIndex] = tmp;
                childIndex = parentIndex;
            }
        }

        public Node Dequeue()
        {
            int lastIndex = elements.Count - 1;
            Node frontItem = elements[0];
            positions.Remove(frontItem.Position);
            elements[0] = elements[lastIndex];
            elements.RemoveAt(lastIndex);

            lastIndex--;
            int parentIndex = 0;
            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1;
                if (leftChildIndex > lastIndex)
                    break;
                int rightChildIndex = leftChildIndex + 1;
                if (rightChildIndex <= lastIndex && elements[rightChildIndex].CompareTo(elements[leftChildIndex]) < 0)
                    leftChildIndex = rightChildIndex;
                if (elements[parentIndex].CompareTo(elements[leftChildIndex]) <= 0)
                    break;
                Node tmp = elements[parentIndex];
                elements[parentIndex] = elements[leftChildIndex];
                elements[leftChildIndex] = tmp;
                parentIndex = leftChildIndex;
            }
            return frontItem;
        }

        public bool ContainsPosition(Vector2Int position)
        {
            return positions.Contains(position);
        }
    }
}