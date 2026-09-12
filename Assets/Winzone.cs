using UnityEngine;

public class WinZone : MonoBehaviour
{
    public WinScreen winScreen;

    private bool won = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !won)
        {
            won = true;

            if (winScreen != null)
            {
                winScreen.ShowWinScreen();
            }

            Debug.Log("LEVEL 2 COMPLETE! YOU WON!");
        }
    }

    private void OnGUI()
    {
        if (won && winScreen == null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 32;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.green;

            float w = 400;
            float h = 100;
            GUI.Box(new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h), "LEVEL 2 COMPLETE!\nPUZZLE SOLVED!", style);
        }
    }
}
