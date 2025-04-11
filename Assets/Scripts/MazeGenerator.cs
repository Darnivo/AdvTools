using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 10;
    public int height = 10;
    public int seed = 12345;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startMarker;
    public GameObject endMarker;

    private Transform mazeParent;
    private Cell[,] grid;

    private class Cell
    {
        public bool rightWall = true;
        public bool bottomWall = true;
        public int set;
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze()
    {
        Random.InitState(seed);
        CleanupMaze();
        InitializeGrid();
        ApplyEllersAlgorithm();
        VisualizeMaze();
    }

    private void CleanupMaze()
    {
        if (mazeParent != null) DestroyImmediate(mazeParent.gameObject);
        mazeParent = new GameObject("Maze").transform;
    }

    private void InitializeGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                grid[x, z] = new Cell();
    }

    private void ApplyEllersAlgorithm()
    {
        int maxSet = 0;
        for (int row = 0; row < height; row++)
        {
            // Assign sets for new row
            if (row == 0)
            {
                for (int x = 0; x < width; x++)
                    grid[x, row].set = maxSet++;
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    if (!grid[x, row - 1].bottomWall)
                        grid[x, row].set = grid[x, row - 1].set;
                    else
                        grid[x, row].set = maxSet++;
                }
            }

            // Merge adjacent cells
            if (row != height - 1)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    if (grid[x, row].set != grid[x + 1, row].set && Random.value > 0.5f)
                    {
                        MergeSets(row, x, x + 1);
                        grid[x, row].rightWall = false;
                    }
                }

                // Create vertical connections
                Dictionary<int, List<int>> setMembers = new Dictionary<int, List<int>>();
                for (int x = 0; x < width; x++)
                {
                    int currentSet = grid[x, row].set;
                    if (!setMembers.ContainsKey(currentSet))
                        setMembers[currentSet] = new List<int>();
                    setMembers[currentSet].Add(x);
                }

                foreach (var set in setMembers.Values)
                {
                    // Ensure at least one vertical connection
                    int mandatoryIndex = Random.Range(0, set.Count);
                    grid[set[mandatoryIndex], row].bottomWall = false;

                    // Optionally add more vertical connections (e.g., 50% chance)
                    for (int i = 0; i < set.Count; i++)
                    {
                        if (i != mandatoryIndex && Random.value < 0.5f)
                        {
                            grid[set[i], row].bottomWall = false;
                        }
                    }
                }
            }
            else
            {
                // Final row merging
                for (int x = 0; x < width - 1; x++)
                {
                    if (grid[x, row].set != grid[x + 1, row].set)
                    {
                        MergeSets(row, x, x + 1);
                        grid[x, row].rightWall = false;
                    }
                }
            }
        }
    }

    private void MergeSets(int row, int a, int b)
    {
        int targetSet = grid[a, row].set;
        int oldSet = grid[b, row].set;

        for (int x = 0; x < width; x++)
        {
            if (grid[x, row].set == oldSet)
                grid[x, row].set = targetSet;
        }
    }

    private void VisualizeMaze()
    {
        // Create floors
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Instantiate(floorPrefab, new Vector3(x, 0, z), Quaternion.identity, mazeParent);
            }
        }

        // Create walls
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Right walls
                if (grid[x, z].rightWall && x < width - 1)
                {
                    Vector3 pos = new Vector3(x + 0.5f, 0.5f, z);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
                }

                // Bottom walls
                if (grid[x, z].bottomWall && z < height - 1)
                {
                    Vector3 pos = new Vector3(x, 0.5f, z + 0.5f);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
                }
            }
        }

        // Create outer walls for the maze
        // Top wall
        for (int x = 0; x < width; x++)
        {
            Vector3 pos = new Vector3(x, 0.5f, -0.5f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
        }

        // Left wall
        for (int z = 0; z < height; z++)
        {
            Vector3 pos = new Vector3(-0.5f, 0.5f, z);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
        }

        // Right outer wall
        for (int z = 0; z < height; z++)
        {
            Vector3 pos = new Vector3(width - 0.5f, 0.5f, z);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
        }

        // Bottom outer wall
        for (int x = 0; x < width; x++)
        {
            Vector3 pos = new Vector3(x, 0.5f, height - 0.5f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
        }

        // Place markers
        Instantiate(startMarker, new Vector3(0, 0.5f, 0), Quaternion.identity, mazeParent);
        Instantiate(endMarker, new Vector3(width - 1, 0.5f, height - 1), Quaternion.identity, mazeParent);
    }
}

