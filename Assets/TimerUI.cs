using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public LoopManager loopManager;

    private TextMeshProUGUI timerText;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float timeLeft = loopManager.GetTimeLeft();

        timerText.text = Mathf.Ceil(timeLeft).ToString();
    }
}