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
        bool shouldOpen = pressurePlate.activated;

        doorRenderer.enabled = !shouldOpen;
        doorCollider.enabled = !shouldOpen;
    }
}