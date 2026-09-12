using UnityEngine;

/// <summary>
/// Invisible trigger zone for Level 3.
/// - When the PLAYER enters for the first time: immediately resets the loop (spawns ghost).
/// - When a GHOST enters: notifies L3_LevelManager that a clone reached this point.
///   This fires every time any ghost passes through, but the level manager handles idempotency.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LoopResetTrigger : MonoBehaviour
{
    public LoopManager loopManager;
    public L3_LevelManager levelManager;

    private bool playerFiredOnce = false;

    void Start()
    {
        // Ensure trigger mode
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playerFiredOnce && other.CompareTag("Player"))
        {
            playerFiredOnce = true;
            if (loopManager != null)
                loopManager.ForceResetLoop();
        }
        else if (other.CompareTag("Ghost"))
        {
            // Every ghost that passes through fires this — level manager handles it
            if (levelManager != null)
                levelManager.OnClonePassedResetZone();
        }
    }
}
