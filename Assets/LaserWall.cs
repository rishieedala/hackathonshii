using UnityEngine;

/// <summary>
/// A laser wall (or blocking wall) that can be toggled visible/solid or permanently disabled.
/// Starts visible+blocking or invisible+passable depending on settings.
/// When a controlling PressurePlate is set and gets activated, the wall is permanently disabled.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class LaserWall : MonoBehaviour
{
    [Header("Initial State")]
    public bool startVisible  = true;
    public bool startBlocking = true;

    [Header("Control Plate (optional)")]
    [Tooltip("If this plate is activated, the laser is permanently disabled.")]
    public PressurePlate controlledByPlate;

    private new Renderer renderer;
    private Collider col;
    private bool permanentlyOff = false;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        col      = GetComponent<Collider>();

        renderer.enabled = startVisible;
        col.enabled      = startBlocking;
    }

    void Update()
    {
        if (!permanentlyOff && controlledByPlate != null && controlledByPlate.activated)
            PermanentlyDisable();
    }

    /// <summary>Make the laser visible and solid (called by L3_LevelManager when clone arrives).</summary>
    public void Activate()
    {
        if (permanentlyOff) return;
        renderer.enabled = true;
        col.enabled      = true;
    }

    /// <summary>Hide and make passable — but can re-activate.</summary>
    public void Deactivate()
    {
        renderer.enabled = false;
        col.enabled      = false;
    }

    /// <summary>Permanently turn off — cannot be re-enabled.</summary>
    public void PermanentlyDisable()
    {
        permanentlyOff   = true;
        renderer.enabled = false;
        col.enabled      = false;
    }

    public bool IsPermanentlyOff() => permanentlyOff;
}
