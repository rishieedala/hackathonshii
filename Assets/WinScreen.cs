using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public GameObject winPanel;

    public void ShowWinScreen()
    {
        winPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void GoToLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }
}