using UnityEngine;

/// <summary>
/// Level 2 ONLY — permanent door behaviour.
/// Opens when the linked PressurePlate activates, stays open forever.
/// Only resets to closed when LoopManager2 calls ResetDoor() on loop reset.
/// Do NOT use this on Level 1 — Level 1 uses ExitDoor.cs.
/// </summary>
public class PermanentDoor : MonoBehaviour
{
    [Header("Settings")]
    public float openHeight    = 3.8f;
    public float slideSpeed    = 6f;
    public float clearanceHeight = 1.8f;   // door must rise this far before collider disables

    [Header("References")]
    public PressurePlate pressurePlate;    // assign in Inspector (or auto-found)

    private Vector3  closedPos;
    private Vector3  openPos;
    private bool     isOpen       = false;
    private Collider doorCollider;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        closedPos     = transform.position;
        openPos       = closedPos + new Vector3(0, openHeight, 0);
        doorCollider  = GetComponent<Collider>();

        if (pressurePlate == null)
            pressurePlate = FindAnyObjectByType<PressurePlate>();

        // Ensure collider enabled at start
        if (doorCollider != null) doorCollider.enabled = true;
    }

    private void Update()
    {
        // Latch open: once isOpen is true it never closes until ResetDoor() is called
        if (!isOpen && pressurePlate != null && pressurePlate.activated)
        {
            isOpen = true;
            Debug.Log("PermanentDoor (Level 2): plate activated — door latched open.");
        }

        // Slide toward target
        Vector3 target = isOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * slideSpeed);

        // Manage collider based on how far the door has risen
        if (doorCollider != null)
        {
            float rise = transform.position.y - closedPos.y;
            doorCollider.enabled = (rise < clearanceHeight);
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Called by LoopManager2 when the loop resets (clone death / hazard pit).
    /// Instantly snaps door back to closed and re-enables its collider.
    /// </summary>
    public void ResetDoor()
    {
        isOpen                = false;
        transform.position    = closedPos;
        if (doorCollider != null) doorCollider.enabled = true;

        Debug.Log("PermanentDoor (Level 2): reset to closed.");
    }
}
