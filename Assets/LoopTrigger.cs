using UnityEngine;

public class LoopTrigger : MonoBehaviour
{
    public CloneSpawner cloneSpawner;
    public LoopManager loopManager;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (loopManager != null)
        {
            loopManager.ResetLoop();
        }
        else if (cloneSpawner != null)
        {
            cloneSpawner.SpawnClone();
        }

        Debug.Log("LOOP TRIGGER ACTIVATED");
    }
}
