using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public float loopDuration = 24f;
    public Transform player;
    public GameObject ghostPrefab;

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
        completedLoops.Add(currentRecording);

        Debug.Log(
            "Loop saved! Total loops: " + completedLoops.Count
        );

        GameObject ghostObject = Instantiate(
            ghostPrefab,
            startPosition,
            startRotation
        );
        ghostObject.tag = "Ghost";

        GhostReplay ghost = ghostObject.GetComponent<GhostReplay>();
        if (ghost != null)
        {
            ghost.SetRecording(currentRecording);
        }

        // Restart all active ghosts to sync with new loop
        GhostReplay[] allGhosts = FindObjectsByType<GhostReplay>(FindObjectsSortMode.None);
        foreach (var g in allGhosts)
        {
            g.ResetReplay();
        }

        player.position = startPosition;
        player.rotation = startRotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.position = startPosition;
            rb.rotation = startRotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        currentRecording = new LoopRecording();

        timer = loopDuration;
    }
public float GetTimeLeft()
{
    return timer;
}
}