using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public LoopManager loopManager;

    private TextMeshProUGUI timerText;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        if (loopManager == null)
        {
            loopManager = LoopManager.Instance != null ? LoopManager.Instance : FindAnyObjectByType<LoopManager>();
        }
    }

    void Update()
    {
        if (loopManager == null)
        {
            loopManager = LoopManager.Instance != null ? LoopManager.Instance : FindAnyObjectByType<LoopManager>();
        }

        if (timerText != null && loopManager != null)
        {
            float timeLeft = loopManager.GetTimeLeft();
            timerText.text = Mathf.Max(0, Mathf.Ceil(timeLeft)).ToString();
        }
    }
}