using UnityEngine;

public class PlayerLimbAnimation : MonoBehaviour
{
    [Header("Body Parts")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("Animation Settings")]
    public float swingAngle = 30f;
    public float swingSpeed = 8f;

    private float animationTime;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        // Check if player is pressing movement keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float inputMagnitude = new Vector2(horizontal, vertical).magnitude;

        // Check world movement / velocity (works for both player and replaying ghost!)
        Vector3 displacement = transform.position - lastPosition;
        displacement.y = 0f;
        float speed = Time.deltaTime > 0f ? displacement.magnitude / Time.deltaTime : 0f;
        lastPosition = transform.position;

        bool isMoving = inputMagnitude > 0.1f || speed > 0.2f;

        // If player or ghost is moving
        if (isMoving)
        {
            animationTime += Time.deltaTime * swingSpeed;

            float swing =
                Mathf.Sin(animationTime) * swingAngle;

            // Legs move opposite each other
            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);

            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);

            // Arms move opposite to legs
            if (leftArm != null)
                leftArm.localRotation = Quaternion.Euler(-swing, 0, 0);

            if (rightArm != null)
                rightArm.localRotation = Quaternion.Euler(swing, 0, 0);
        }
        else
        {
            // Return limbs to normal position
            float t = Time.deltaTime * 10f;
            if (leftArm != null)
                leftArm.localRotation = Quaternion.Lerp(leftArm.localRotation, Quaternion.identity, t);

            if (rightArm != null)
                rightArm.localRotation = Quaternion.Lerp(rightArm.localRotation, Quaternion.identity, t);

            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Lerp(leftLeg.localRotation, Quaternion.identity, t);

            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Lerp(rightLeg.localRotation, Quaternion.identity, t);
        }
    }
}