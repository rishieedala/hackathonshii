using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 200f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Camera playerCamera;
    private MouseLook mouseLook;
    private float jumpBufferTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            mouseLook = playerCamera.GetComponent<MouseLook>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();

        // Only handle rotation here if MouseLook component is not handling it
        if (mouseLook == null)
        {
            Look();
        }
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        jumpBufferTimer = 0f;
    }

    void Move()
    {
        if (controller == null)
            return;

        // Ground detection: CharacterController flag or downward raycast
        bool isGrounded = controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, 1.2f);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Jump input buffering for crisp responsive jumping
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = 0.2f;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (isGrounded && jumpBufferTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            Debug.Log("JUMP!");
        }

        // WASD movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical movement
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject != null)
        {
            PressurePlate plate = hit.gameObject.GetComponent<PressurePlate>();
            if (plate == null)
            {
                plate = hit.gameObject.GetComponentInParent<PressurePlate>();
            }

            if (plate != null)
            {
                plate.TriggerPlate();
            }
        }
    }
}
