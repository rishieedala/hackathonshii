using UnityEngine;

/// <summary>
/// Laser wall that blocks movement via a BoxCollider on this object,
/// and controls visual beams via Renderer components in child objects.
///
/// Parent: BoxCollider (full height, invisible blocker) + this script
/// Children: thin red beam GameObjects (MeshRenderer)
///
/// When controlledByPlate is activated → permanently disable.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LaserWall : MonoBehaviour
{
    [Header("Initial State")]
    public bool startVisible  = true;
    public bool startBlocking = true;

    [Header("Control Plate (optional)")]
    [Tooltip("When this plate is activated, the laser is permanently disabled.")]
    public PressurePlate controlledByPlate;

    private Renderer[] beamRenderers;
    private Collider   col;
    private bool       permanentlyOff = false;

    void Start()
    {
        col           = GetComponent<Collider>();
        beamRenderers = GetComponentsInChildren<Renderer>(true);

        SetRenderers(startVisible);
        col.enabled = startBlocking;
    }

    void Update()
    {
        if (!permanentlyOff && controlledByPlate != null && controlledByPlate.activated)
            PermanentlyDisable();
    }

    /// <summary>Make beams visible + collider solid (called when clone reaches trigger).</summary>
    public void Activate()
    {
        if (permanentlyOff) return;
        SetRenderers(true);
        col.enabled = true;
    }

    /// <summary>Hide beams, keep collider off.</summary>
    public void Deactivate()
    {
        SetRenderers(false);
        col.enabled = false;
    }

    /// <summary>Permanently turn off — cannot be re-enabled.</summary>
    public void PermanentlyDisable()
    {
        permanentlyOff = true;
        Deactivate();
    }

    public bool IsPermanentlyOff() => permanentlyOff;

    private void SetRenderers(bool on)
    {
        if (beamRenderers == null) return;
        foreach (var r in beamRenderers)
            r.enabled = on;
    }
}
