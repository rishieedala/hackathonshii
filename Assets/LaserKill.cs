using UnityEngine;

/// <summary>
/// Attach this to a TRIGGER child of a LaserWall.
/// When the player touches it, the loop resets (player respawns at start).
/// The parent LaserWall's BoxCollider is the solid blocker (not a trigger).
/// This child has a thin trigger slightly wider than the visual beam.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LaserKill : MonoBehaviour
{
    public LoopManager loopManager;

    private LaserWall parentWall;

    void Start()
    {
        // Ensure this collider is trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Find parent LaserWall
        parentWall = GetComponentInParent<LaserWall>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Only kill if the parent laser wall is active/visible
        if (parentWall != null && parentWall.IsPermanentlyOff()) return;

        Debug.Log("[LaserKill] Player hit laser! Resetting loop.");

        if (loopManager != null)
            loopManager.ForceResetLoop();
    }
}
