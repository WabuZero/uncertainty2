using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("01Scenes/02Opening");
    }
   
    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#endif
        Application.Quit();
    }
}