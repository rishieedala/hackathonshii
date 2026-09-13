using System.Collections.Generic;
using UnityEngine;

public class CloneReplay : MonoBehaviour
{
    public List<ReplayRecorder.Frame> frames = new List<ReplayRecorder.Frame>();
    public GameObject corpsePrefab;

    [Header("Limbs for Animation")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;
    public float swingSpeed = 8f;
    public float swingAngle = 30f;

    private float replayTime = 0f;
    private bool isReplaying = false;
    private bool isDead = false;
    private float animationTime = 0f;
    private Vector3 previousPosition;

    private void Awake()
    {
        SetLayerRecursively(gameObject, 0); // Ensure Default layer (0) so clone is visible
        AutoFindLimbs();
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

    public void SetRecording(List<ReplayRecorder.Frame> recordedFrames)
    {
        frames = new List<ReplayRecorder.Frame>(recordedFrames);
        replayTime = 0f;
        isDead = false;
        isReplaying = true;

        if (frames.Count > 0)
        {
            transform.position = frames[0].position;
            transform.rotation = frames[0].rotation;
            previousPosition = transform.position;
        }
    }

    public void StartReplay()
    {
        if (frames != null && frames.Count > 0)
        {
            replayTime = 0f;
            isReplaying = true;
            previousPosition = transform.position;
        }
    }

    private void Update()
    {
        if (!isReplaying || isDead || frames == null || frames.Count == 0)
            return;

        replayTime += Time.deltaTime;
        float totalTime = frames[frames.Count - 1].time;

        if (replayTime >= totalTime)
        {
            // Clone reached the end of the recording (where player died on laser)
            transform.position = frames[frames.Count - 1].position;
            transform.rotation = frames[frames.Count - 1].rotation;
            Die();
            return;
        }

        ApplyFrame(replayTime);

        // Calculate speed to drive limb animations
        float speed = (transform.position - previousPosition).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
        previousPosition = transform.position;
        AnimateLimbs(speed);
    }

    private void ApplyFrame(float time)
    {
        for (int i = 0; i < frames.Count - 1; i++)
        {
            ReplayRecorder.Frame current = frames[i];
            ReplayRecorder.Frame next = frames[i + 1];

            if (time >= current.time && time <= next.time)
            {
                float t = Mathf.InverseLerp(current.time, next.time, time);
                transform.position = Vector3.Lerp(current.position, next.position, t);
                transform.rotation = Quaternion.Slerp(current.rotation, next.rotation, t);
                return;
            }
        }

        transform.position = frames[frames.Count - 1].position;
        transform.rotation = frames[frames.Count - 1].rotation;
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

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isReplaying = false;

        // Spawn physical corpse at death position if corpsePrefab is assigned
        if (corpsePrefab != null)
        {
            GameObject corpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
            SetLayerRecursively(corpse, 0);
        }

        Debug.Log("CLONE DIED - CORPSE REMAINS");
        Destroy(gameObject);
    }
}
