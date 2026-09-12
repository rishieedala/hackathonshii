using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public PressurePlate pressurePlate;

    private Renderer doorRenderer;
    private Collider doorCollider;
    private Vector3 closedPos;
    private Vector3 openPos;

    void Start()
    {
        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
        closedPos = transform.position;
        openPos = closedPos + new Vector3(0, 3.2f, 0); // Slide upwards
    }

    void Update()
    {
        if (pressurePlate == null)
            return;

        bool shouldOpen = pressurePlate.activated;

        // Smoothly slide open
        Vector3 targetPos = shouldOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 6f);

        // Disable collider once door is sufficiently open
        if (doorCollider != null)
        {
            doorCollider.enabled = (transform.position.y - closedPos.y) < 1.0f;
        }
    }
}
