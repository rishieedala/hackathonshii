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
        if (won && (winScreen == null || winScreen.winPanel == null))
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))
            {
                ReplayLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GoToLevel1();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GoToLevel2();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                GoToLevel3();
            }
        }
    }

    private void ReplayLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("level1");
    }

    private void GoToLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    private void GoToLevel3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level3");
    }

    private void OnGUI()
    {
        if (won && (winScreen == null || winScreen.winPanel == null))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            bool isLevel3 = currentScene.Equals("Level3", System.StringComparison.OrdinalIgnoreCase);

            // Center modal container
            float w = 520;
            float h = 320;
            float x = (Screen.width - w) / 2;
            float y = (Screen.height - h) / 2;

            // Semi-transparent background box
            GUI.Box(new Rect(x, y, w, h), "");

            // Main Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 30;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);
            GUI.Label(new Rect(x, y + 15, w, 40), isLevel3 ? "LEVEL 3 COMPLETE!" : "LEVEL 2 COMPLETE!", titleStyle);

            // Subtitle
            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 16;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y + 55, w, 28), isLevel3 ? "CLONE TEAMWORK MASTERED!" : "PUZZLE SOLVED - TIME LOOP CONQUERED!", subStyle);

            // Button styling
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 15;
            btnStyle.fontStyle = FontStyle.Bold;

            if (isLevel3)
            {
                // Play Level 3 Again
                GUIStyle primaryBtn = new GUIStyle(GUI.skin.button);
                primaryBtn.fontSize = 16;
                primaryBtn.fontStyle = FontStyle.Bold;
                primaryBtn.normal.textColor = Color.yellow;
                if (GUI.Button(new Rect(x + 50, y + 95, w - 100, 45), "Play Level 3 Again (Enter / Space)", primaryBtn))
                {
                    ReplayLevel();
                }

                // Return to Level 2
                if (GUI.Button(new Rect(x + 50, y + 155, w - 100, 45), "Return to Level 2 (Press 2)", btnStyle))
                {
                    GoToLevel2();
                }

                // Return to Level 1
                if (GUI.Button(new Rect(x + 50, y + 215, w - 100, 45), "Return to Level 1 (Press 1)", btnStyle))
                {
                    GoToLevel1();
                }
            }
            else
            {
                // Level 2 default view
                GUIStyle advBtnStyle = new GUIStyle(GUI.skin.button);
                advBtnStyle.fontSize = 16;
                advBtnStyle.fontStyle = FontStyle.Bold;
                advBtnStyle.normal.textColor = Color.yellow;
                if (GUI.Button(new Rect(x + 50, y + 95, w - 100, 45), "Advance to Level 3 (Press 3)", advBtnStyle))
                {
                    GoToLevel3();
                }

                if (GUI.Button(new Rect(x + 50, y + 155, w - 100, 45), "Play Level 2 Again (Enter / Space)", btnStyle))
                {
                    ReplayLevel();
                }

                if (GUI.Button(new Rect(x + 50, y + 215, w - 100, 45), "Return to Level 1 (Press 1)", btnStyle))
                {
                    GoToLevel1();
                }
            }
        }
    }
}

