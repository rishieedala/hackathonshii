using UnityEngine;

public class GhostReplay : MonoBehaviour
{
    [Header("Limbs for Animation")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;
    public float swingSpeed = 8f;
    public float swingAngle = 30f;

    private LoopRecording recording;
    private float replayTime;
    private Rigidbody rb;
    private float animationTime = 0f;
    private Vector3 previousPosition;

    void Awake()
    {
        gameObject.tag = "Ghost";
        SetLayerRecursively(gameObject, 0); // Ensure ghost is on Default layer so it is visible

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        AutoFindLimbs();

        // Disable PlayerLimbAnimation if present so GhostReplay controls limbs directly
        PlayerLimbAnimation pla = GetComponentInChildren<PlayerLimbAnimation>();
        if (pla != null)
        {
            pla.enabled = false;
        }
    }

    private void AutoFindLimbs()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren)
        {
            if (leftArm == null && t.name == "LeftArm") leftArm = t;
            if (rightArm == null && t.name == "RightArm") rightArm = t;
            if (leftLeg == null && t.name == "LeftLeg") leftLeg = t;
            if (rightLeg == null && t.name == "RightLeg") rightLeg = t;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void SetRecording(LoopRecording newRecording)
    {
        recording = newRecording;
        replayTime = 0f;
        ApplyFrame(0f);
        previousPosition = transform.position;
    }

    public void ResetReplay()
    {
        replayTime = 0f;
        ApplyFrame(0f);
        previousPosition = transform.position;
    }

    void Update()
    {
        if (recording == null || recording.frames.Count == 0)
            return;

        replayTime += Time.deltaTime;

        if (replayTime >= recording.frames[recording.frames.Count - 1].time)
        {
            replayTime = recording.frames[recording.frames.Count - 1].time;
        }

        ApplyFrame(replayTime);

        // Animate limbs based on movement speed
        float speed = (transform.position - previousPosition).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
        previousPosition = transform.position;
        AnimateLimbs(speed);
    }

    private void AnimateLimbs(float speed)
    {
        if (speed > 0.1f)
        {
            animationTime += Time.deltaTime * swingSpeed;
            float swing = Mathf.Sin(animationTime) * swingAngle;

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(swing, 0, 0);
        }
        else
        {
            if (leftArm != null) leftArm.localRotation = Quaternion.Lerp(leftArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Lerp(rightArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Lerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Lerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    private void ApplyFrame(float time)
    {
        if (recording == null || recording.frames.Count == 0)
            return;

        PlayerFrame frame = GetFrameAtTime(time);

        transform.position = frame.position;
        transform.rotation = frame.rotation;

        if (rb != null)
        {
            rb.position = frame.position;
            rb.rotation = frame.rotation;
        }
    }

    PlayerFrame GetFrameAtTime(float time)
    {
        for (int i = 0; i < recording.frames.Count - 1; i++)
        {
            PlayerFrame current = recording.frames[i];
            PlayerFrame next = recording.frames[i + 1];

            if (time >= current.time && time <= next.time)
            {
                float t = Mathf.InverseLerp(current.time, next.time, time);
                Vector3 position = Vector3.Lerp(current.position, next.position, t);
                Quaternion rotation = Quaternion.Slerp(current.rotation, next.rotation, t);
                return new PlayerFrame(time, position, rotation);
            }
        }

        return recording.frames[recording.frames.Count - 1];
    }
}
