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

    [Header("Camera Settings")]
    public bool autoAdjustCamera = true;
    public Camera targetCamera;

    private Transform mazeParent;
    public Cell[,] grid;

    public class Cell
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

        if (autoAdjustCamera) RelocateCam();
    }

    public void RelocateCam()
    {
        if (targetCamera != null)
        {
            // Center the camera on the maze based on its actual dimensions.
            float centerZ = (width - 1) / 2f;
            float centerX = (height - 1) / 2f;
            targetCamera.transform.position = new Vector3(centerX, 20f, centerZ);

            float aspectRatio = targetCamera.aspect;

            // Calculate required dimensions including padding (0.5 on each side)
            float requiredWidth = width + 1f;
            float requiredHeight = height + 1f;

            // Calculate orthographic sizes based on both width and height
            float verticalOrtho = requiredHeight / 2f;
            float horizontalOrtho = requiredWidth / (2f * aspectRatio);

            // Use the larger size to ensure the entire maze fits
            targetCamera.orthographicSize = Mathf.Max(verticalOrtho, horizontalOrtho);
            
            // targetCamera.transform.position = new Vector3(center, 20f, center);
            // targetCamera.orthographicSize = height / 2f + 0.5f;
        }
        else
        {
            Debug.LogWarning("Target camera is not assigned.");
        }
    }

    private void CleanupMaze()
    {
        if (mazeParent != null) DestroyImmediate(mazeParent.gameObject);
        mazeParent = new GameObject("Maze").transform;
    }

    private void InitializeGrid()
    {
        grid = new Cell[height, width];
        for (int x = 0; x < height; x++)
            for (int z = 0; z < width; z++)
                grid[x, z] = new Cell();
    }

    private void ApplyEllersAlgorithm()
    {
        int maxSet = 0;
        for (int row = 0; row < width; row++)
        {
            // Assign sets for new row
            if (row == 0)
            {
                for (int x = 0; x < height; x++)
                    grid[x, row].set = maxSet++;
            }
            else
            {
                for (int x = 0; x < height; x++)
                {
                    if (!grid[x, row - 1].bottomWall)
                        grid[x, row].set = grid[x, row - 1].set;
                    else
                        grid[x, row].set = maxSet++;
                }
            }

            // Merge adjacent cells
            if (row != width - 1)
            {
                for (int x = 0; x < height - 1; x++)
                {
                    if (grid[x, row].set != grid[x + 1, row].set && Random.value > 0.5f)
                    {
                        MergeSets(row, x, x + 1);
                        grid[x, row].rightWall = false;
                    }
                }

                // Create vertical connections
                Dictionary<int, List<int>> setMembers = new Dictionary<int, List<int>>();
                for (int x = 0; x < height; x++)
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
                for (int x = 0; x < height - 1; x++)
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

        for (int x = 0; x < height; x++)
        {
            if (grid[x, row].set == oldSet)
                grid[x, row].set = targetSet;
        }
    }

    private void VisualizeMaze()
    {
        // Create floors
        for (int x = 0; x < height; x++)
        {
            for (int z = 0; z < width; z++)
            {
                Instantiate(floorPrefab, new Vector3(x, 0, z), Quaternion.identity, mazeParent);
            }
        }

        // Create walls
        for (int x = 0; x < height; x++)
        {
            for (int z = 0; z < width; z++)
            {
                // Right walls
                if (grid[x, z].rightWall && x < height - 1)
                {
                    Vector3 pos = new Vector3(x + 0.5f, 0.5f, z);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
                }

                // Bottom walls
                if (grid[x, z].bottomWall && z < width - 1)
                {
                    Vector3 pos = new Vector3(x, 0.5f, z + 0.5f);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
                }
            }
        }

        // Create outer walls for the maze
        // Top wall
        for (int x = 0; x < height; x++)
        {
            Vector3 pos = new Vector3(x, 0.5f, -0.5f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
        }

        // Left wall
        for (int z = 0; z < width; z++)
        {
            Vector3 pos = new Vector3(-0.5f, 0.5f, z);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
        }

        // Right outer wall
        for (int z = 0; z < width; z++)
        {
            Vector3 pos = new Vector3(height - 0.5f, 0.5f, z);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(0.1f, 1f, 1f);
        }

        // Bottom outer wall
        for (int x = 0; x < height; x++)
        {
            Vector3 pos = new Vector3(x, 0.5f, width - 0.5f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
        }

        // Place markers
        Instantiate(startMarker, new Vector3(0, 0.5f, 0), Quaternion.identity, mazeParent);
        Instantiate(endMarker, new Vector3(height - 1, 0.5f, width - 1), Quaternion.identity, mazeParent);
    }
}

