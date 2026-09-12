using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public float loopDuration = 24f;
    public Transform player;

    private float timer;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private LoopRecording currentRecording;

    public List<LoopRecording> completedLoops = new List<LoopRecording>();

    void Start()
    {
        timer = loopDuration;

        startPosition = player.position;
        startRotation = player.rotation;

        currentRecording = new LoopRecording();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        RecordPlayer();

        if (timer <= 0f)
        {
            ResetLoop();
        }
    }

    void RecordPlayer()
    {
        float currentTime = loopDuration - timer;

        PlayerFrame frame = new PlayerFrame(
            currentTime,
            player.position,
            player.rotation
        );

        currentRecording.frames.Add(frame);
    }

    void ResetLoop()
    {
        // Save this loop
        completedLoops.Add(currentRecording);

        Debug.Log(
            "Loop saved! Total loops: " + completedLoops.Count
        );

        // Reset player
        player.position = startPosition;
        player.rotation = startRotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Start recording a new loop
        currentRecording = new LoopRecording();

        timer = loopDuration;
    }
}