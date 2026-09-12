using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public PressurePlate pressurePlate;

    private Renderer doorRenderer;
    private Collider doorCollider;

    void Start()
    {
        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (pressurePlate == null)
            return;

        bool shouldOpen = pressurePlate.activated;

        if (doorRenderer != null)
            doorRenderer.enabled = !shouldOpen;

        if (doorCollider != null)
            doorCollider.enabled = !shouldOpen;
    }
}