using UnityEngine;
using TMPro;

/// <summary>
/// Tracks total elapsed time for Level 2 (counts UP).
/// Shows MM:SS format. Stops when the level is won and stamps final time.
/// </summary>
public class Level2Timer : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI timerText;      // Elapsed time display  "TIME  MM:SS"
    public TextMeshProUGUI loopTimerText;  // Loop countdown        "LOOP  XX"
    public LoopManager loopManager;

    [Header("Win Screen")]
    public TextMeshProUGUI winTimeText;    // Text on win panel showing final time

    private float elapsed = 0f;
    private bool running = true;

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;

        if (timerText != null)
        {
            int m = Mathf.FloorToInt(elapsed / 60f);
            int s = Mathf.FloorToInt(elapsed % 60f);
            timerText.text = string.Format("TIME  {0:00}:{1:00}", m, s);
        }

        if (loopTimerText != null && loopManager != null)
        {
            float left = loopManager.GetTimeLeft();
            loopTimerText.text = string.Format("LOOP  {0:00}", Mathf.Ceil(left));
        }
    }

    public void StopTimer()
    {
        running = false;

        if (winTimeText != null)
        {
            int m = Mathf.FloorToInt(elapsed / 60f);
            int s = Mathf.FloorToInt(elapsed % 60f);
            winTimeText.text = string.Format("Completed in  {0:00}:{1:00}", m, s);
        }
    }

    public float GetElapsed() => elapsed;
}
