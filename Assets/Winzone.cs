using UnityEngine;

public class WinZone : MonoBehaviour
{
    public WinScreen winScreen;
    public Level2Timer level2Timer; // Optional — only set in Level 2

    private bool won = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !won)
        {
            won = true;

            // Stop the level timer if present (Level 2)
            if (level2Timer != null)
                level2Timer.StopTimer();

            if (winScreen != null)
                winScreen.ShowWinScreen();

            Debug.Log("LEVEL COMPLETE!");
        }
    }
}