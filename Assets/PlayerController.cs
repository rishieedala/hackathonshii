using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float sprintMultiplier = 1.4f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 200f;

    private CharacterController controller;
    private Rigidbody rb;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Camera playerCamera;
    private MouseLook mouseLook;
    private float jumpBufferTimer = 0f;
    private PlayerDeath playerDeath;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        playerDeath = GetComponent<PlayerDeath>();
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
        // Fall protection: if player falls off into the abyss, trigger death / reset
        if (transform.position.y < -5f)
        {
            HandleFallDeath();
            return;
        }

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
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Jump input buffering
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = 0.2f;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // Mode 1: CharacterController (Level 2)
        if (controller != null)
        {
            bool isGrounded = controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, 1.2f);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (isGrounded && jumpBufferTimer > 0f)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpBufferTimer = 0f;
            }

            controller.Move(move * currentSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        // Mode 2: Rigidbody (Level 1)
        else if (rb != null)
        {
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);
            float yVel = rb.linearVelocity.y;

            if (isGrounded && jumpBufferTimer > 0f)
            {
                yVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpBufferTimer = 0f;
            }

            Vector3 targetVel = move * currentSpeed;
            rb.linearVelocity = new Vector3(targetVel.x, yVel, targetVel.z);
        }
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

    void HandleFallDeath()
    {
        if (playerDeath != null)
        {
            playerDeath.Die();
        }
        else if (LoopManager.Instance != null)
        {
            LoopManager.Instance.ResetLoop();
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != null)
        {
            PressurePlate plate = collision.gameObject.GetComponent<PressurePlate>();
            if (plate == null)
            {
                plate = collision.gameObject.GetComponentInParent<PressurePlate>();
            }

            if (plate != null)
            {
                plate.TriggerPlate();
            }
        }
    }

    private void OnGUI()
    {
        // Don't draw in-game HUD if cursor is unlocked (e.g. on win screen)
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Subtle reticle crosshair in the center of the screen for precision platforming
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        int size = 3;
        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        GUI.DrawTexture(new Rect(cx - size, cy - size, size * 2, size * 2), Texture2D.whiteTexture);

        // Control hint in bottom-left corner
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 13;
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
        GUI.Label(new Rect(20, Screen.height - 35, 300, 25), "[WASD] Move   [Shift] Sprint   [Space] Jump   [R] Loop Reset", hintStyle);
    }
}

