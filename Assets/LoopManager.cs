using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public float loopDuration = 24f;
    public Transform player;
    public GameObject ghostPrefab;

    [Tooltip("Max simultaneous ghosts. Set to 1 for Level 1, 2 for Level 2.")]
    public int maxGhosts = 1;

    private float timer;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private LoopRecording currentRecording;

    // Queue tracks active ghosts oldest-first
    private readonly Queue<GameObject> activeGhosts = new Queue<GameObject>();

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
        // If at capacity, destroy the oldest ghost
        if (activeGhosts.Count >= maxGhosts)
        {
            GameObject oldest = activeGhosts.Dequeue();
            if (oldest != null)
                Destroy(oldest);
        }

        completedLoops.Add(currentRecording);

        Debug.Log("Loop saved! Active ghosts: " + (activeGhosts.Count + 1) + "/" + maxGhosts);

        GameObject ghostObject = Instantiate(
            ghostPrefab,
            startPosition,
            startRotation
        );
        ghostObject.tag = "Ghost";
        activeGhosts.Enqueue(ghostObject);

        GhostReplay ghost = ghostObject.GetComponent<GhostReplay>();
        if (ghost != null)
        {
            ghost.SetRecording(currentRecording);
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