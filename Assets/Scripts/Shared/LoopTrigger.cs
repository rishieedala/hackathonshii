using UnityEngine;

/// <summary>
/// Zone trigger that fires a loop reset when the Player enters.
/// Works with both LoopManager (Level 1) and LoopManager2 (Level 2).
/// </summary>
public class LoopTrigger : MonoBehaviour
{
    // Assign in Inspector — or leave null to auto-find
    public CloneSpawner cloneSpawner;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Try Level 1 manager first, then Level 2
        if (LoopManager.Instance != null)
        {
            LoopManager.Instance.ResetLoop();
        }
        else if (LoopManager2.Instance != null)
        {
            LoopManager2.Instance.ResetLoop();
        }
        else if (cloneSpawner != null)
        {
            cloneSpawner.SpawnClone();
        }

        Debug.Log("LoopTrigger: activated");
    }
}
