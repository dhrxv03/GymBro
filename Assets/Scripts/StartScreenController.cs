using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    public void OpenLevelOne()
    {
        SceneManager.LoadScene("Level01");
    }

    public void OpenLevelTwo()
    {
        // This button becomes functional when the innovation level is added.
        if (Application.CanStreamedLevelBeLoaded("InnovationScene"))
            SceneManager.LoadScene("InnovationScene");
    }
}
