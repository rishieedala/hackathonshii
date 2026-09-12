using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 1000f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX, Space.World);
            Vector3 currentEuler = transform.parent.eulerAngles;
            if (currentEuler.x != 0f || currentEuler.z != 0f)
            {
                transform.parent.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
            }
        }
    }
}