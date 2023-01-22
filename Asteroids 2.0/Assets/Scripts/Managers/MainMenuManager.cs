using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton, quitButton;
    [SerializeField] private AudioClip click1, click2;
    private AudioSource source;

    private void Start()
    {
        Time.timeScale = 1;
        if (playButton == null)
        {
            playButton = GameObject.Find("Play Button").GetComponent<Button>();
        }
        if (quitButton == null)
        {
            quitButton = GameObject.Find("Quit Button").GetComponent<Button>();
        }
        source = GetComponent<AudioSource>();
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void PlayGame()
    {
        PlayClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void QuitGame()
    {
        PlayClick();
        Application.Quit();
    }

    private void PlayClick()
    {
        var clip = click1;
        if (Random.value < 0.5) clip = click2;

        source.PlayOneShot(clip);
    }
}