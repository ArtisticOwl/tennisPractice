using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI pointsText;
    public int points = 0;
    public int misses = 0;
    public bool gameStarted = false;
    public GameObject finalStatePanel;
    public void RequestPermission()
    {
        Application.ExternalEval("requestWebcamPermission();");
    }
    private void Awake()
    {
        RequestPermission();
        finalStatePanel.SetActive(false);
    }
    private void Start()
    {
        countdownText.gameObject.SetActive(false);
        pointsText.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        // Check if the game has already started
        if (gameStarted) return;

        // Start the countdown
        countdownText.gameObject.SetActive(true);
        pointsText.gameObject.SetActive(true);
        pointsText.text = "Points: ";
        StartCoroutine(Countdown());

        // Update the game state
        gameStarted = true;
    }

    public void AddPoint()
    {
        points++;
        pointsText.text = "Points: " + points;
    }

    public void Miss()
    {
        misses++;
        if (misses >= 10)
        {
            print("you lose!");
            gameStarted = false;
            StopCoroutine(FireTennisBall.Instance.FireBallsSequence());
            StopCoroutine(FireTennisBall.Instance.FireBall());
            finalStatePanel.SetActive(true);
            finalStatePanel.transform.Find("State").GetComponent<TextMeshProUGUI>().text = "GAME OVER!!";
//            finalStatePanel.transform.Find("Points").GetComponent<TextMeshProUGUI>().text += points.ToString();
        }
    }
    public void reloadScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
    public void backToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Win()
    {
        finalStatePanel.SetActive(true);
        finalStatePanel.transform.Find("State").GetComponent<TextMeshProUGUI>().text = "YOU WIN!!";
        finalStatePanel.transform.Find("Points").GetComponent<TextMeshProUGUI>().text += points.ToString();
    }

    IEnumerator Countdown()
    {
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1);
            count--;
        }
        countdownText.text = "Go!";
        
        yield return new WaitForSeconds(1);
        countdownText.gameObject.SetActive(false);
        gameStarted = true;
        StartCoroutine(FireTennisBall.Instance.FireBallsSequence());
    }
}
