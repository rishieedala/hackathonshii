using UnityEngine;

public class GhostReplay : MonoBehaviour
{
    private LoopRecording recording;
    private float replayTime;
    private Rigidbody rb;

    void Awake()
    {
        gameObject.tag = "Ghost";
        SetLayerRecursively(gameObject, 0); // Ensure ghost is on Default layer (0) so it's visible to camera

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
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
    }

    public void ResetReplay()
    {
        replayTime = 0f;
        ApplyFrame(0f);
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
                float t = Mathf.InverseLerp(
                    current.time,
                    next.time,
                    time
                );

                Vector3 position = Vector3.Lerp(
                    current.position,
                    next.position,
                    t
                );

                Quaternion rotation = Quaternion.Slerp(
                    current.rotation,
                    next.rotation,
                    t
                );

                return new PlayerFrame(
                    time,
                    position,
                    rotation
                );
            }
        }

        return recording.frames[recording.frames.Count - 1];
    }
}