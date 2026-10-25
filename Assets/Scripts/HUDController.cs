using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text scaredText;
    [SerializeField] private GameObject[] lifeIcons;
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private Text messageText;
    [SerializeField] private Button exitButton;

    public void SetScore(int score) { scoreText.text = score.ToString("D6"); }
    public void SetTime(float seconds) { timerText.text = FormatTime(seconds); }
    public void SetLives(int lives)
    {
        for (int i = 0; i < lifeIcons.Length; i++) lifeIcons[i].SetActive(i < lives);
    }
    public void SetScaredTime(float seconds)
    {
        scaredText.gameObject.SetActive(seconds > 0f);
        scaredText.text = "POWER-UP: " + Mathf.CeilToInt(seconds) + "s";
    }
    public void ShowMessage(string message)
    {
        blockingPanel.SetActive(true);
        messageText.text = message;
        exitButton.interactable = false;
    }
    public void HideMessage()
    {
        blockingPanel.SetActive(false);
        exitButton.interactable = true;
    }
    public void ExitLevel() { SceneManager.LoadScene("StartScene"); }
    public static string FormatTime(float seconds)
    {
        int hundredths = Mathf.FloorToInt(Mathf.Max(0, seconds) * 100f);
        return (hundredths / 6000).ToString("D2") + ":" +
            (hundredths / 100 % 60).ToString("D2") + ":" + (hundredths % 100).ToString("D2");
    }
}
