using UnityEngine;

/// <summary>
/// Invisible trigger zone for Level 3.
/// When the PLAYER crosses this zone, it notifies L3_LevelManager to start
/// the hidden 5-second countdown that spawns Plate 2.
/// Only fires once for the player (subsequent crossings are ignored after first fire).
/// </summary>
[RequireComponent(typeof(Collider))]
public class L3_CrossZone : MonoBehaviour
{
    public L3_LevelManager levelManager;

    private bool fired = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        // No renderer on this trigger zone — it's invisible by design
    }

    void OnTriggerEnter(Collider other)
    {
        if (!fired && other.CompareTag("Player"))
        {
            fired = true;
            if (levelManager != null)
                levelManager.OnPlayerCrossedLaser1Zone();
        }
    }
}
