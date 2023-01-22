using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private GameObject gameOverScreen, pauseScreen;
    [SerializeField] private Button restartButton, restart02, mainMenuButton, mainMenu02, exitButton, exit02;
    [SerializeField] private TMP_Text finalScore, highscore;
    private Animator highScoreAnim;
    [Space]
    [SerializeField] private GameObject[] lifeIcons;
    [SerializeField] private TMP_Text scoreboard, invasionWarning;
    private string click01 = "button_click_01", click02 = "button_click_02";

    private PlayerController player;

    private void Start()
    {
        gameManager = GameManager.instance;

        scoreboard.text = "00000";
        gameManager.onPointsGained += OnPointsGained;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.onHealthChange += OnPlayerHealthChange;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].SetActive(true);
        }

        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(QuitToMainMenu);
        exitButton.onClick.AddListener(QuitGame);

        restart02.onClick.AddListener(RestartGame);
        mainMenu02.onClick.AddListener(QuitToMainMenu);
        exit02.onClick.AddListener(QuitGame);

        gameOverScreen.gameObject.SetActive(false);
        pauseScreen.gameObject.SetActive(false);

        gameManager.onGameOver += OnGameOver;
        gameManager.onPause += TogglePause;

        highScoreAnim = gameOverScreen.GetComponentInChildren<Animator>();
        highScoreAnim.enabled = false;

        invasionWarning.gameObject.SetActive(false);
        InvasionManager.instance.onInvasionWarning += ActivateInvasionWarning;
    }

    private void TogglePause(bool isPaused)
    {
        pauseScreen.SetActive(isPaused);
    }

    private void OnPointsGained()
    {
        scoreboard.text = "" + gameManager.playerPoints.ToString("00000");
    }

    private void OnPlayerHealthChange()
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (i + 1 <= player.currentHealth)
                lifeIcons[i].SetActive(true);
            else lifeIcons[i].SetActive(false);
        }
    }

    public void ActivateInvasionWarning()
    {
        StartCoroutine(FlashInvasionWarning());
    }

    private IEnumerator FlashInvasionWarning()
    {
        invasionWarning.gameObject.SetActive(true);

        float t = 0;
        float timeToFlash = 10;
        Color warningColor = invasionWarning.color;
        while (t < timeToFlash)
        {
         
            warningColor.a = Mathf.PingPong(Time.time, 1);
            invasionWarning.color = warningColor;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        warningColor.a = 1;
        invasionWarning.color = warningColor;
        invasionWarning.gameObject.SetActive(false);
    }

    private void OnGameOver()
    {
        if (gameManager.CheckForNewHighScore())
        {
            highScoreAnim.enabled = true;
        }
        else highScoreAnim.enabled = false;

        gameManager.SetFinalScore();

        finalScore.text = gameManager.playerPoints.ToString("00000");
        highscore.text = gameManager.GetHighScore().ToString("00000");

        gameOverScreen.SetActive(true);
    }

    #region - Buttons -
    private void RestartGame()
    {
        PlayClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitToMainMenu()
    {
        PlayClick();
        SceneManager.LoadScene(0);
    }

    private void QuitGame()
    {
        PlayClick();
        Application.Quit();
    }

    private void PlayClick()
    {
        string clip = click01;
        if (Random.value > 0.5) clip = click02;
        AudioManager.PlayClip(clip);
    }
    #endregion
}
