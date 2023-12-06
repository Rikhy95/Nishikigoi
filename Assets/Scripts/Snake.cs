using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Snake : MonoBehaviour
{
    public event System.Action<bool> gameOver;

    #region Public variables
    [Header("Tweakables")]
    [Tooltip("Time between movements in seconds")]
    public float moveDelay = 0.7f;
    [Tooltip("Starting direction of the snake")]
    public Direction startingDirection = Direction.Down;
    [Tooltip("Starting length of the snake tail")]
    public int startingTailLength = 3;
    [Tooltip("Prefab of the tail block")]
    public GameObject tailBlockPrefab;

    #endregion

    #region Private variables
    private GridManager gridManager;
    private Vector2Int snakeDirection;
    private Vector2Int lastMovementDirection;
    private float lastMovementTime;
    private Vector2Int lastHeadPosition;
    private List<Vector2Int> lastTailBlockPositions = new List<Vector2Int>();
    private List<Transform> tail = new List<Transform>();
    private bool cellOccupied = false;
    private int score = 0;
    #endregion

    #region Lifecycle
    private void Start()
    {
        lastMovementTime = Time.time;
        ApplyStartingPositionAndDirection();
        CreateStartingTail();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0f;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Time.timeScale = 1f;
        }

        DirectionInput(KeyCode.W, Vector2Int.up, Vector2Int.down);
        DirectionInput(KeyCode.S, Vector2Int.down, Vector2Int.up);
        DirectionInput(KeyCode.A, Vector2Int.left, Vector2Int.right);
        DirectionInput(KeyCode.D, Vector2Int.right, Vector2Int.left);

        CheckTailGameOver();

        if (Time.time - lastMovementTime > moveDelay)
        {
            lastMovementTime += moveDelay;
            CheckForApple();
            UpdateLastPositions();
            MoveHead(snakeDirection);
            UpdateTailBlocks();
            UpdateCells();
        }

    }
    #endregion

    #region Public methods
    public void Damage()
    {
        RemoveTailBlocks(1);
    }

    public void SetGridManager(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public int GetScore()
    {
        return score;
    }
    #endregion

    #region Private methods
    private void CreateStartingTail()
    {
        for (int i = 0; i < startingTailLength; i++)
        {
            Vector2Int blockPosition = ExtensionMethods.WrapVector2Int(ExtensionMethods.ConvertToVector2Int(transform.position) - snakeDirection * (i + 1), gridManager.width, gridManager.height);
            tail.Add(Instantiate(tailBlockPrefab, ExtensionMethods.ConvertToVector3(blockPosition), Quaternion.identity).transform);
            gridManager.UpdateCellContent(CellContent.SnakeTail, blockPosition);
        }

        UpdateLastPositions();
    }

    private void MoveHead(Vector2Int direction)
    {
        //Calculate new position
        Vector2Int newPosition = ExtensionMethods.ConvertToVector2Int(transform.position) + direction;
        newPosition = ExtensionMethods.WrapVector2Int(newPosition, gridManager.width, gridManager.height);             //new Vector2Int(WrapIntValue(newPosition.x, gridScript.width), WrapIntValue(newPosition.y, gridScript.height));

        switch (gridManager.GetCellContent(newPosition))
        {
            case CellContent.Empty:
                ApplyHeadMovement(newPosition);
                break;
            case CellContent.SnakeTail:
            case CellContent.Wall:
                cellOccupied = true;
                gameOver(false);
                break;
            case CellContent.Enemy:
                GameObject enemy = gridManager.GetCellContentGameObject(newPosition);
                if (tail.Count >= enemy.GetComponent<Enemy>().lengthToKill)
                {
                    cellOccupied = false;
                    enemy.GetComponent<Enemy>().Kill();

                }
                else
                {
                    cellOccupied = true;
                    RemoveTailBlocks(1);

                }
                break;
            case CellContent.Projectile:
                Damage();
                gridManager.GetCellContentGameObject(newPosition).GetComponent<Projectile>().DestroyProjectile();
                ApplyHeadMovement(newPosition);
                break;
        }
        
    }

    private void ApplyHeadMovement(Vector2Int newPosition)
    {
        transform.position = ExtensionMethods.ConvertToVector3(newPosition);
        lastMovementDirection = snakeDirection;
        cellOccupied = false;
    }

    private void UpdateTailBlocks()
    {
        if (!cellOccupied)
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (i == 0)
                {
                    tail[i].position = ExtensionMethods.ConvertToVector3(lastHeadPosition);
                }
                else
                {
                    tail[i].position = ExtensionMethods.ConvertToVector3(lastTailBlockPositions[i - 1]);
                }
            }
        }
    }

    private void UpdateCells()
    {
        //Clear previous cells
        gridManager.UpdateCellContent(CellContent.Empty, lastHeadPosition);
        for (int i = 0; i < tail.Count; i++)
        {
            gridManager.UpdateCellContent(CellContent.Empty, lastTailBlockPositions[i]);
        }
        //Set new cells
        gridManager.UpdateCellContent(CellContent.SnakeHead, ExtensionMethods.ConvertToVector2Int(transform.position));
        for (int i = 0; i < tail.Count; i++)
        {
            gridManager.UpdateCellContent(CellContent.SnakeTail, ExtensionMethods.ConvertToVector2Int(tail[i].position));
        }
    }

    private void UpdateLastPositions()
    {
        lastHeadPosition = ExtensionMethods.ConvertToVector2Int(transform.position);
        lastTailBlockPositions.Clear();
        for (int i = 0; i < tail.Count; i++)
        {
            lastTailBlockPositions.Add(ExtensionMethods.ConvertToVector2Int(tail[i].position));
        }
    }

    private void DirectionInput(KeyCode key, Vector2Int dir, Vector2Int oppositeDirection)
    {
        if (Input.GetKeyDown(key) && lastMovementDirection != oppositeDirection)
        {
            snakeDirection = dir;
        }
    }

    private void ApplyStartingPositionAndDirection()
    {
        transform.position = ExtensionMethods.ConvertToVector3(ExtensionMethods.ConvertToVector2Int(transform.position));
        gridManager.UpdateCellContent(CellContent.SnakeHead, ExtensionMethods.ConvertToVector2Int(transform.position));
        switch (startingDirection)
        {
            case Direction.Up:
                snakeDirection = Vector2Int.up;
                break;
            case Direction.Down:
                snakeDirection = Vector2Int.down;
                break;
            case Direction.Left:
                snakeDirection = Vector2Int.left;
                break;
            case Direction.Right:
                snakeDirection = Vector2Int.right;
                break;
        }
    }

    private void AddTailBlocks(int amount)
    {
        if (tail.Count == 0)
        {
            tail.Add(Instantiate(tailBlockPrefab, ExtensionMethods.ConvertToVector3(lastHeadPosition), Quaternion.identity).transform);
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                tail.Add(Instantiate(tailBlockPrefab, tail[tail.Count - 1].position, Quaternion.identity).transform);
            }
        }
    }

    private void RemoveTailBlocks(int amount)
    {
        if (amount <= tail.Count)
        {
            int count = tail.Count - amount - 1;
            for (int i = tail.Count - 1; i > count; i--)
            {
                Destroy(tail[i].gameObject);
                gridManager.UpdateCellContent(CellContent.Empty, ExtensionMethods.ConvertToVector2Int(tail[i].position));
                tail.RemoveAt(i);
            }
        }
        else
        {
            for (int i = 0; i < tail.Count; i++)
            {
                Destroy(tail[i].gameObject);
                gridManager.UpdateCellContent(CellContent.Empty, ExtensionMethods.ConvertToVector2Int(tail[i].position));
            }
                tail.Clear();
        }
    }

    private void CheckForApple()
    {
        if (gridManager.GetIfCellHasApple(ExtensionMethods.ConvertToVector2Int(transform.position), out GameObject apple))
        {
            AddTailBlocks(1);
            Destroy(apple);
            gridManager.AppleAte(ExtensionMethods.ConvertToVector2Int(transform.position));
        }
    }

    private void CheckTailGameOver()
    {
        if (tail.Count == 0)
        {
            gameOver(false);
        }
    }
    #endregion
}