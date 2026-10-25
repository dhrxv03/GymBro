using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    [SerializeField] private Text levelOneRecord;
    [SerializeField] private Text levelTwoRecord;

    private void Start()
    {
        ShowRecord(levelOneRecord, "Level01");
        ShowRecord(levelTwoRecord, "InnovationScene");
    }
    private void ShowRecord(Text label, string level)
    {
        label.text = "BEST  " + HighScoreStore.GetScore(level).ToString("D6") +
            "     TIME  " + HUDController.FormatTime(HighScoreStore.GetTime(level));
    }

    public void OpenLevelOne()
    {
        SceneManager.LoadScene("Level01");
    }

    public void OpenLevelTwo()
    {
        if (Application.CanStreamedLevelBeLoaded("InnovationScene"))
            SceneManager.LoadScene("InnovationScene");
    }
}
