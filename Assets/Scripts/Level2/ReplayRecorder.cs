using System.Collections.Generic;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
    [System.Serializable]
    public class Frame
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;

        public Frame(Vector3 position, Quaternion rotation, float time)
        {
            this.position = position;
            this.rotation = rotation;
            this.time = time;
        }
    }

    public List<Frame> frames = new List<Frame>();

    [HideInInspector]
    public bool isRecording = true;

    private float timer = 0f;

    private void Update()
    {
        if (!isRecording)
            return;

        timer += Time.deltaTime;

        frames.Add(
            new Frame(
                transform.position,
                transform.rotation,
                timer
            )
        );
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void StartRecording()
    {
        isRecording = true;
    }

    public void ClearRecording()
    {
        frames.Clear();
        timer = 0f;
        isRecording = true;
        Debug.Log("RECORDING CLEARED");
    }

    public List<Frame> GetFramesCopy()
    {
        return new List<Frame>(frames);
    }
}
