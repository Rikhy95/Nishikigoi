using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    #region Public variables
    public Snake snake;
    public List<Enemy> enemies;
    public GridManager gridManager;
    public GameObject gameOverScreen;
    #endregion

    #region Private variables
    #endregion

    #region Lifecyle
    void Start()
    {
        snake.SetGridManager(gridManager);
        snake.gameOver += GameOver;

        if (enemies.Count > 0)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.SetInitialValues(snake.transform, gridManager);
                enemy.destroyed += HandleEnemyDestroyed;
            }
        }
        else
        {
            Debug.LogWarning("THERE ARE NO ENEMIES");
        }
    }

    void Update()
    {
        
        if (enemies.Count == 0)
        {
            GameOver(victory:true);
        }
    }
    #endregion

    #region Public methods
    
    #endregion

    #region Private methods
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        enemy.destroyed -= HandleEnemyDestroyed;
        enemies.RemoveAll(element => element == enemy);
    }

    private void GameOver(bool victory = false)
    {
        Time.timeScale = 0f;
        //gameOverScreen.SetActive(true);
        Debug.Log("----GAME OVER----");
        if (victory)
        {
            Debug.Log($"You won with a score of: {snake.GetScore()}");
        }
        else
        {
            Debug.Log("You lost");
        }
    }
    #endregion
}
