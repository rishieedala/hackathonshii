using UnityEngine;

/// <summary>
/// ExitDoor — slides up when the linked PressurePlate is activated,
/// slides back down when the plate is deactivated.
///
/// Driven entirely by PressurePlate.activated every frame, so the door
/// state always reflects exactly who is standing on the plate.
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [Header("References")]
    public PressurePlate pressurePlate;

    [Header("Settings")]
    public float openHeight  = 3.8f;
    public float slideSpeed  = 6f;
    /// <summary>
    /// How far the door must have risen (in world units) before its collider
    /// is disabled so the player can pass through the opening.
    /// </summary>
    public float clearanceHeight = 1.8f;

    private Renderer  doorRenderer;
    private Collider  doorCollider;
    private Vector3   closedPos;
    private Vector3   openPos;

    // ── Unity Lifecycle ─────────────────────────────────────────────────────

    void Start()
    {
        if (pressurePlate == null)
        {
            pressurePlate = FindAnyObjectByType<PressurePlate>();
        }

        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();

        // Capture the door's rest position in Start(); never modify closedPos again.
        closedPos = transform.position;
        openPos   = closedPos + new Vector3(0, openHeight, 0);

        if (doorRenderer != null)
        {
            doorRenderer.enabled = true;
        }

        // Make sure the collider is enabled at startup
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }
    }

    void Update()
    {
        // Lazily find the plate if it was not assigned
        if (pressurePlate == null)
        {
            pressurePlate = FindAnyObjectByType<PressurePlate>();
        }

        bool shouldOpen = (pressurePlate != null && pressurePlate.activated);

        // ── Smooth sliding motion ────────────────────────────────────────────
        Vector3 targetPos = shouldOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * slideSpeed);

        // Keep door renderer visible at all times so the slide is visible
        if (doorRenderer != null && !doorRenderer.enabled)
        {
            doorRenderer.enabled = true;
        }

        // ── Collider management ──────────────────────────────────────────────
        // Disable collision once the door has risen enough for the player to pass;
        // re-enable it as soon as it starts descending below the clearance threshold.
        if (doorCollider != null)
        {
            float riseAmount = transform.position.y - closedPos.y;
            bool hasCleared  = riseAmount > clearanceHeight;
            doorCollider.enabled = !hasCleared;
        }
    }

    /// <summary>
    /// Instantly snaps the door back to its closed position and re-enables its
    /// collider. Called by LoopManager (or externally) when a loop resets and
    /// you want the door to be immediately shut rather than smoothly lerping.
    /// </summary>
    public void ResetDoor()
    {
        transform.position = closedPos;

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        Debug.Log("ExitDoor: Reset to closed position.");
    }

    /// <summary>
    /// Legacy no-op — kept for backward compatibility with any code that calls
    /// OpenDoor() directly. The door now opens automatically via Update().
    /// </summary>
    public void OpenDoor()
    {
        // Opening is handled in Update() by reading pressurePlate.activated.
    }
}
