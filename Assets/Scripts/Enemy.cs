using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Enemy : MonoBehaviour
{
    public event System.Action<Enemy> destroyed = null;

    #region Public variables
    public GameObject projectilePrefab;

    public int lengthToKill = 3;

    [Header("Available shooting directions")]
    public bool shootDirectionUp = false;
    public bool shootDirectionDown = false;
    public bool shootDirectionLeft = false;
    public bool shootDirectionRight = false;

    [Header("Movement")]
    public Direction[] movementSteps;
    public float movementDelay;

    [Header("Shooting")]
    public float shootingDelayMin;
    public float shootingDelayMax;
    public float projectileMoveDelay;

    #endregion

    #region Private variables
    private Transform snake;
    private GridManager gridManager;
    private float lastMovementTime;
    private float lastShootingTime;
    private float currentShootingDelay;
    private Vector2Int[] moveCycle; 
    private int currentCycleStep = 0;
    private bool isKnownByGameManager = false;
    #endregion

    #region Lifecyle
    void Start()
    {
        if (!isKnownByGameManager)
        {
            Debug.LogWarning($"[{gameObject.name}] GAME MANAGER DOES NOT KNOW ABOUT ME!");
        }
        lastMovementTime = Time.time;
        lastShootingTime = Time.time;
        SnapPositionAtStart();
        InitializeMoveCycle();
    }

    void Update()
    {
        if (Time.time - lastMovementTime > movementDelay)
        {
            lastMovementTime += movementDelay;
            ProcessMoveCycleStep();
        }

        if (Time.time - lastShootingTime > currentShootingDelay)
        {
            lastShootingTime += currentShootingDelay;
            CalculateNewShootingDelay();
            try
            {
                Shoot();
            }
            catch
            {
                Debug.LogWarning("Projectile spawn point was outside the grid");
            }
        }
    }

    #endregion

    #region Public methods
    public void SetInitialValues(Transform snake, GridManager gridManager)
    {
        isKnownByGameManager = true;
        this.snake = snake;
        this.gridManager = gridManager;
    }

    public void Kill()
    {
        if (destroyed != null)
        {
            destroyed(this);
        }
        Destroy(gameObject);
    }
    #endregion

    #region Private methods
    private void InitializeMoveCycle()
    {
        moveCycle = new Vector2Int[movementSteps.Length];

        for (int i = 0; i < movementSteps.Length; i++)
        {
            switch (movementSteps[i])
            {
                case Direction.Up:
                    moveCycle[i] = Vector2Int.up;
                    break;
                case Direction.Down:
                    moveCycle[i] = Vector2Int.down;
                    break;
                case Direction.Left:
                    moveCycle[i] = Vector2Int.left;
                    break;
                case Direction.Right:
                    moveCycle[i] = Vector2Int.right;
                    break;

            }
        }
    }

    private void ProcessMoveCycleStep()
    {
        if (moveCycle.Length > 0)
        {
            Vector2Int newPosition = ExtensionMethods.ConvertToVector2Int(transform.position) + moveCycle[currentCycleStep];
            Move(newPosition);
            currentCycleStep = ExtensionMethods.WrapIntValue(currentCycleStep + 1, moveCycle.Length);
        }
    }

    private void Move(Vector2Int destination)
    {
        destination = ExtensionMethods.WrapVector2Int(destination, gridManager.width, gridManager.height);
        switch (gridManager.GetCellContent(destination))
        {
            case CellContent.Empty:
                ApplyMovement(destination);
                break;
            default:
                Debug.Log($"Cell {destination} is occupied");
                break;
        }
    }

    private void ApplyMovement(Vector2Int newPosition)
    {
        Vector2Int oldPosition = ExtensionMethods.ConvertToVector2Int(transform.position);
        gridManager.ClearCellContentGameObject(oldPosition);
        gridManager.UpdateCellContent(CellContent.Empty, oldPosition);
        transform.position = ExtensionMethods.ConvertToVector3(newPosition);
        gridManager.UpdateCellContent(CellContent.Enemy, newPosition);
        gridManager.SetCellContentGameObject(newPosition, gameObject);
    }

    private void SnapPositionAtStart()
    {
        Vector2Int pos = ExtensionMethods.ConvertToVector2Int(transform.position);
        transform.position = ExtensionMethods.ConvertToVector3(pos);
        gridManager.UpdateCellContent(CellContent.Enemy, pos);
        gridManager.SetCellContentGameObject(pos, gameObject);
    }

    private void Shoot()
    {
        if (shootDirectionUp)
        {
            ShootStraightProjectile(Vector2Int.up);
        }
        if (shootDirectionDown)
        {
            ShootStraightProjectile(Vector2Int.down);
        }
        if (shootDirectionLeft)
        {
            ShootStraightProjectile(Vector2Int.left);
        }
        if (shootDirectionRight)
        {
            ShootStraightProjectile(Vector2Int.right);
        }
    }

    private void ShootStraightProjectile(Vector2Int direction)
    {
        Vector2Int spawnPosition = ExtensionMethods.ConvertToVector2Int(transform.position) + direction;
        if (gridManager.GetCellContent(spawnPosition) == CellContent.Empty)
        {
            GameObject projectile = Instantiate(projectilePrefab, ExtensionMethods.ConvertToVector3(spawnPosition), Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            projectileScript.SetInitialValues(direction, projectileMoveDelay, gridManager, snake);
        }
    }

    private void CalculateNewShootingDelay()
    {
        currentShootingDelay = Random.Range(shootingDelayMin, shootingDelayMax);
    }
    #endregion
}