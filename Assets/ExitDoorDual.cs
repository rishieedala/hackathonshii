using UnityEngine;

/// <summary>
/// Door that opens only when BOTH pressure plates are activated simultaneously.
/// Used in Level 2 where two ghosts must stand on two plates at the same time.
/// </summary>
public class ExitDoorDual : MonoBehaviour
{
    public PressurePlate pressurePlate1;
    public PressurePlate pressurePlate2;

    private Renderer doorRenderer;
    private Collider doorCollider;

    void Start()
    {
        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (pressurePlate1 == null || pressurePlate2 == null)
            return;

        bool shouldOpen = pressurePlate1.activated && pressurePlate2.activated;

        if (doorRenderer != null)
            doorRenderer.enabled = !shouldOpen;

        if (doorCollider != null)
            doorCollider.enabled = !shouldOpen;
    }
}
