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

            winScreen.ShowWinScreen();

            Debug.Log("LEVEL 1 COMPLETE!");
        }
    }
}