using UnityEngine;
using TMPro;

/// <summary>
/// Works in both Level 1 and Level 2.
/// Automatically finds whichever LoopManager is present in the scene.
/// </summary>
public class TimerUI : MonoBehaviour
{
    private TextMeshProUGUI timerText;

    // Level 1 manager (LoopManager)
    private LoopManager  loopManager1;
    // Level 2 manager (LoopManager2)
    private LoopManager2 loopManager2;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        FindManagers();
    }

    void Update()
    {
        // Lazy re-find if managers are null (scene just loaded)
        if (loopManager1 == null && loopManager2 == null)
            FindManagers();

        if (timerText == null) return;

        float timeLeft = 0f;
        if      (loopManager1 != null) timeLeft = loopManager1.GetTimeLeft();
        else if (loopManager2 != null) timeLeft = loopManager2.GetTimeLeft();

        timerText.text = Mathf.Max(0, Mathf.Ceil(timeLeft)).ToString();
    }

    private void FindManagers()
    {
        loopManager1 = LoopManager.Instance  ?? FindAnyObjectByType<LoopManager>();
        loopManager2 = LoopManager2.Instance ?? FindAnyObjectByType<LoopManager2>();
    }
}