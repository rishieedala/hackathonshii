using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public PressurePlate pressurePlate;

    private Renderer doorRenderer;
    private Collider doorCollider;
    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;

    void Start()
    {
        if (pressurePlate == null)
        {
            pressurePlate = FindAnyObjectByType<PressurePlate>();
        }

        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
        closedPos = transform.position;
        openPos = closedPos + new Vector3(0, 3.8f, 0); // Slide up to ceiling
    }

    public void OpenDoor()
    {
        isOpen = true;
        Debug.Log("EXIT DOOR OPENING!");
    }

    void Update()
    {
        if (pressurePlate != null && pressurePlate.activated)
        {
            isOpen = true;
        }

        // Smoothly slide open
        Vector3 targetPos = isOpen ? openPos : closedPos;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

        // Disable collider once door begins moving open so player won't be blocked
        if (doorCollider != null)
        {
            if (isOpen && (transform.position.y - closedPos.y) > 0.5f)
            {
                doorCollider.enabled = false;
            }
        }
    }
}
