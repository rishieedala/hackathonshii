using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public PressurePlate pressurePlate;
    public float openHeight = 3.8f;
    public float slideSpeed = 6f;

    private Renderer doorRenderer;
    private Collider doorCollider;
    private Vector3 closedPos;
    private Vector3 openPos;

    void Start()
    {
        if (pressurePlate == null)
        {
            pressurePlate = FindAnyObjectByType<PressurePlate>();
        }

        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
        closedPos = transform.position;
        openPos = closedPos + new Vector3(0, openHeight, 0);

        if (doorRenderer != null)
        {
            doorRenderer.enabled = true;
        }
    }

    public void OpenDoor()
    {
        // Target updated smoothly in Update()
    }

    void Update()
    {
        if (pressurePlate == null)
        {
            pressurePlate = FindAnyObjectByType<PressurePlate>();
        }

        bool shouldOpen = (pressurePlate != null && pressurePlate.activated);

        // Smooth sliding motion
        Vector3 targetPos = shouldOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * slideSpeed);

        // Door renderer remains visible to showcase the sliding door
        if (doorRenderer != null && !doorRenderer.enabled)
        {
            doorRenderer.enabled = true;
        }

        // Clear collision once door has slid high enough, re-enable when closing
        if (doorCollider != null)
        {
            bool hasCleared = (transform.position.y - closedPos.y) > 1.8f;
            doorCollider.enabled = !hasCleared;
        }
    }
}

