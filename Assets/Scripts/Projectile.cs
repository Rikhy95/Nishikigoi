using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Public variables

    #endregion

    #region Private variables
    private Vector2Int direction;
    private float moveDelay;
    private GridManager gridManager;
    private Transform snake;
    private float lastMovementTime;
    #endregion

    #region Lifecyle
    void Start()
    {
        lastMovementTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastMovementTime > moveDelay)
        {
            lastMovementTime += moveDelay;
            Move(ExtensionMethods.ConvertToVector2Int(transform.position) + direction);
        }
    }
    #endregion

    #region Public methods
    public void SetInitialValues(Vector2Int direction, float moveDelay, GridManager gridManager, Transform snake)
    {
        this.direction = direction;
        this.moveDelay = moveDelay;
        this.gridManager = gridManager;
        this.snake = snake;
    }

    public void DestroyProjectile()
    {
        Vector2Int currentPosition = ExtensionMethods.ConvertToVector2Int(transform.position);
        gridManager.ClearCellContentGameObject(currentPosition);
        gridManager.UpdateCellContent(CellContent.Empty, currentPosition);
        Destroy(gameObject);
    }
    #endregion

    #region Private methods
    private void Move(Vector2Int newPosition)
    {
        if (newPosition.x > gridManager.width - 1 || newPosition.x < 0 || newPosition.y > gridManager.height - 1 || newPosition.y < 0)
        {
            DestroyProjectile();
            return;
        }
        switch (gridManager.GetCellContent(newPosition))
        {
            case CellContent.Empty:
                ApplyMovement(newPosition);
                break;
            case CellContent.SnakeHead:
                snake.GetComponent<Snake>().Damage();
                DestroyProjectile();
                break;
            case CellContent.SnakeTail:
            case CellContent.Wall:
            case CellContent.Projectile:
            case CellContent.Enemy:
                DestroyProjectile();
                break;

        }
    }

    private void ApplyMovement(Vector2Int newPosition)
    {
        gridManager.UpdateCellContent(CellContent.Empty, ExtensionMethods.ConvertToVector2Int(transform.position));
        gridManager.ClearCellContentGameObject(ExtensionMethods.ConvertToVector2Int(transform.position));
        transform.position = ExtensionMethods.ConvertToVector3(newPosition);
        gridManager.UpdateCellContent(CellContent.Projectile, newPosition);
        gridManager.SetCellContentGameObject(newPosition, gameObject);
    }
    #endregion
}
