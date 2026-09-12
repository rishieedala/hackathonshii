using UnityEngine;

public class GhostReplay : MonoBehaviour
{
    private LoopRecording recording;
    private float replayTime;

    public void SetRecording(LoopRecording newRecording)
    {
        recording = newRecording;
        replayTime = 0f;
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

        PlayerFrame frame = GetFrameAtTime(replayTime);

        transform.position = frame.position;
        transform.rotation = frame.rotation;
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