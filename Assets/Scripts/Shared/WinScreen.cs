using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public GameObject winPanel;

    private bool isShowing = false;

    public void ShowWinScreen()
    {
        isShowing = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (isShowing)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                GoToLevel2();
            }
        }
    }

    public void GoToLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    public void GoToLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("level1");
    }
}