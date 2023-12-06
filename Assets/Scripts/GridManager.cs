using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Serializable]
    public struct CellData
    {
        public CellContent content;
        public bool hasApple;
        public GameObject contentGameObject;
        public GameObject appleGameObject;
        public bool canHaveApple;
    }

    #region Public variables
    public int height;
    public int width;
    public GameObject applePrefab;
    #endregion

    #region Private variables
    private CellData[,] grid;
    [SerializeField]
    #endregion

    #region Lifecyle
    void Awake()
    {
        InitializeGrid();
        GenerateNewApple();
    }

    void Update()
    {
        //PrintGrid();
    }
    #endregion

    #region Public methods
    public void UpdateCellContent(CellContent newContent, Vector2Int coordinates)
    {
        if (grid != null)
        {
            grid[coordinates.x, coordinates.y].content = newContent;
        }
        else
        {
            Debug.LogWarning("Grid does not exist");
        }
    }

    public CellContent GetCellContent(Vector2Int coordinates)
    {
        return grid[coordinates.x, coordinates.y].content;
    }

    public GameObject GetCellContentGameObject(Vector2Int coordinates)
    {
        return grid[coordinates.x, coordinates.y].contentGameObject;
    }

    public bool GetIfCellHasApple(Vector2Int coordinates, out GameObject apple)
    {
        apple = grid[coordinates.x, coordinates.y].appleGameObject;
        return grid[coordinates.x, coordinates.y].hasApple;
    }

    public void AppleAte(Vector2Int coordinates)
    {
        grid[coordinates.x, coordinates.y].hasApple = false;
        GenerateNewApple();
    }

    public void RemoveAppleSpawnPoint(Vector2Int coordinates)
    {
        grid[coordinates.x, coordinates.y].canHaveApple = false;
    }
    #endregion

    public void ClearCellContentGameObject (Vector2Int coordinates)
    {
        grid[coordinates.x, coordinates.y].contentGameObject = null;
    }

    public void SetCellContentGameObject(Vector2Int coordinates, GameObject instance)
    {
        grid[coordinates.x, coordinates.y].contentGameObject = instance;
    }

    #region Private methods
    private void InitializeGrid()
    {
        grid = new CellData[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y].canHaveApple = true;
            }
        }
    }

    private void PrintGrid()
    {
        Debug.Log("-------------------------------------------------------------------");
        for (int i = grid.GetLength(1) - 1; i >= 0; i--)
        {
            string prompt = "";
            for(int j = 0; j < grid.GetLength(0); j++)
            {
                prompt += "[" + grid[j, i].content + "] ";
            }
            Debug.Log(prompt);
        }
    }

    private void GenerateNewApple()
    {
        List<Vector2Int> availableSpots = new List<Vector2Int>();
        
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0;x < grid.GetLength(0); x++)
            {
                if (grid[x, y].content == CellContent.Empty && grid[x, y].canHaveApple)
                {
                    availableSpots.Add(new Vector2Int(x, y));
                }
            }
        }

        int selectedCell = UnityEngine.Random.Range(0, availableSpots.Count);
        Vector2Int newAppleSpot = availableSpots[selectedCell];

        grid[newAppleSpot.x, newAppleSpot.y].hasApple = true;
        grid[newAppleSpot.x, newAppleSpot.y].appleGameObject = Instantiate(applePrefab, ExtensionMethods.ConvertToVector3(newAppleSpot), Quaternion.identity);
    }

    #endregion
}
