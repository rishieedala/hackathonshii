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

    void Update()
    {
        // Check if player is pressing movement keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        float movementAmount =
            new Vector2(horizontal, vertical).magnitude;

        // If player is moving
        if (movementAmount > 0.1f)
        {
            animationTime += Time.deltaTime * swingSpeed;

            float swing =
                Mathf.Sin(animationTime) * swingAngle;

            // Legs move opposite each other
            leftLeg.localRotation =
                Quaternion.Euler(swing, 0, 0);

            rightLeg.localRotation =
                Quaternion.Euler(-swing, 0, 0);

            // Arms move opposite to legs
            leftArm.localRotation =
                Quaternion.Euler(-swing, 0, 0);

            rightArm.localRotation =
                Quaternion.Euler(swing, 0, 0);
        }
        else
        {
            // Return limbs to normal position
            leftArm.localRotation =
                Quaternion.Lerp(
                    leftArm.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 10f
                );

            rightArm.localRotation =
                Quaternion.Lerp(
                    rightArm.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 10f
                );

            leftLeg.localRotation =
                Quaternion.Lerp(
                    leftLeg.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 10f
                );

            rightLeg.localRotation =
                Quaternion.Lerp(
                    rightLeg.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 10f
                );
        }
    }
}