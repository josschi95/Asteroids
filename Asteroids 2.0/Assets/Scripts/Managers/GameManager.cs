using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void OnPointsGainedCallback();
    public OnPointsGainedCallback onPointsGained;

    public delegate void OnGameOverCallback();
    public OnGameOverCallback onGameOver;

    public delegate void OnPauseCallback(bool isPaused);
    public OnPauseCallback onPause;

    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    public Material spriteFlashMat;

    public PlayerController player { get; private set; }

    private Vector2 bottomLeft, topRight;

    public int playerPoints { get; private set; }
    public bool gamePaused { get; private set; }

    private void Start()
    {
        Time.timeScale = 1;
        //PlayerPrefs.DeleteAll();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerPoints = 0;

        var cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
        topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));
    }

    private void Update()
    {
        KeepPlayerInBounds();
    }

    public void PauseGame()
    {
        gamePaused = !gamePaused;
        if (gamePaused) Time.timeScale = 0;
        else Time.timeScale = 1;

        onPause?.Invoke(gamePaused);
    }

    private void KeepPlayerInBounds()
    {
        //Player goes off right side of screen
        if (player.transform.position.x > topRight.x)
        {
            player.transform.position = new Vector2(bottomLeft.x, player.transform.position.y);
        }
        //Player goes off left side of screen
        if (player.transform.position.x < bottomLeft.x)
        {
            player.transform.position = new Vector2(topRight.x, player.transform.position.y);
        }
        //Player goes off top side of screen
        if (player.transform.position.y > topRight.y)
        {
            player.transform.position = new Vector2(player.transform.position.x, bottomLeft.y);
        }
        //Player goes off bottom side of screen
        if (player.transform.position.y < bottomLeft.y)
        {
            player.transform.position = new Vector2(player.transform.position.x, topRight.y);
        }
    }

    public static void OnPlayerPointsGained(int points)
    {
        instance.playerPoints += points;
        instance.onPointsGained?.Invoke();
    }

    public Vector3 GetSpawnPosition()
    {
        int sideToSpawn = Random.Range(0, 4);
        Vector3 spawnPosition = Vector3.zero;

        switch (sideToSpawn)
        {
            case 0: //Spawn from bottom
                spawnPosition.x = Random.Range(bottomLeft.x, topRight.x);
                spawnPosition.y = bottomLeft.y;
                break;
            case 1: //Spawn from left
                spawnPosition.x = bottomLeft.x;
                spawnPosition.y = Random.Range(bottomLeft.y, topRight.y);
                break;
            case 2: //spawn from top
                spawnPosition.x = Random.Range(bottomLeft.x, topRight.x);
                spawnPosition.y = topRight.y;
                break;
            case 3: //spawn from right
                spawnPosition.x = topRight.x;
                spawnPosition.y = Random.Range(bottomLeft.y, topRight.y);
                break;
        }
        return spawnPosition;
    }

    #region - Score -
    public void SetFinalScore()
    {
        if (CheckForNewHighScore())
            PlayerPrefs.SetInt("HighScore", playerPoints);
    }

    public bool CheckForNewHighScore()
    {
        if (playerPoints > GetHighScore()) return true;
        return false;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore");
    }
    #endregion
}