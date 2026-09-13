using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    public WinScreen winScreen;

    private bool won = false;

    void Start()
    {
        if (winScreen == null)
        {
            winScreen = FindAnyObjectByType<WinScreen>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !won)
        {
            won = true;

            if (winScreen != null)
            {
                winScreen.ShowWinScreen();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }

            Debug.Log("LEVEL COMPLETE! YOU WON!");
        }
    }

    void Update()
    {
        if (won && winScreen == null)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))
            {
                ReplayLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GoToLevel1();
            }
        }
    }

    private void ReplayLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    private void GoToLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("level1");
    }

    private void OnGUI()
    {
        if (won && winScreen == null)
        {
            // Center modal container
            float w = 500;
            float h = 260;
            float x = (Screen.width - w) / 2;
            float y = (Screen.height - h) / 2;

            // Semi-transparent background box
            GUI.Box(new Rect(x, y, w, h), "");

            // Main Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);
            GUI.Label(new Rect(x, y + 20, w, 45), "LEVEL 2 COMPLETE!", titleStyle);

            // Subtitle
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 18;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y + 65, w, 30), "PUZZLE SOLVED - YOU CONQUERED THE TIME LOOP!", subStyle);

            // Button styling
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 16;
            btnStyle.fontStyle = FontStyle.Bold;

            // Replay Button
            if (GUI.Button(new Rect(x + 50, y + 120, w - 100, 45), "Play Level 2 Again (Enter / Space)", btnStyle))
            {
                ReplayLevel();
            }

            // Level 1 Button
            if (GUI.Button(new Rect(x + 50, y + 180, w - 100, 45), "Return to Level 1 (Press 1)", btnStyle))
            {
                GoToLevel1();
            }
        }
    }
}

