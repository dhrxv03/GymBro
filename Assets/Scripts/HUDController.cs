using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    public void ExitLevel()
    {
        SceneManager.LoadScene("StartScene");
    }
}
