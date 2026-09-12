using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance { get; private set; }

    [Header("Level 1 Ghost Settings")]
    public float loopDuration = 24f;
    public Transform player;
    public GameObject ghostPrefab;

    [Header("Level 2 Clone & Death Settings")]
    public CloneSpawner cloneSpawner;
    public ReplayRecorder recorder;
    public PlayerDeath playerDeath;
    public Transform startPoint;
    public float deathResetDelay = 1.2f;

    private float timer;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Level 1 data
    private LoopRecording currentRecording;
    public List<LoopRecording> completedLoops = new List<LoopRecording>();

    private bool isResetting = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = loopDuration;

        // Auto-find references if not assigned in Inspector
        if (player == null)
        {
            GameObject playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                player = playerGo.transform;
            }
        }

        if (player != null)
        {
            startPosition = (startPoint != null) ? startPoint.position : player.position;
            startRotation = (startPoint != null) ? startPoint.rotation : player.rotation;

            if (playerDeath == null)
            {
                playerDeath = player.GetComponent<PlayerDeath>();
            }

            if (recorder == null)
            {
                recorder = player.GetComponent<ReplayRecorder>();
            }
        }

        if (cloneSpawner == null)
        {
            cloneSpawner = GetComponent<CloneSpawner>();
            if (cloneSpawner == null)
            {
                cloneSpawner = FindAnyObjectByType<CloneSpawner>();
            }
        }

        currentRecording = new LoopRecording();
    }

    private void Update()
    {
        // Support manual loop restart with R key
        if (Input.GetKeyDown(KeyCode.R) && !isResetting)
        {
            ResetLoop();
            return;
        }

        // Level 1 Ghost Mode (timer-based loop)
        if (ghostPrefab != null)
        {
            timer -= Time.deltaTime;
            RecordPlayerGhost();

            if (timer <= 0f)
            {
                ResetLoop();
            }
        }
    }

    // Called when the player dies (e.g. from laser in Level 2)
    public void OnPlayerDeath()
    {
        if (isResetting)
            return;

        if (recorder != null)
        {
            recorder.StopRecording();
        }

        StartCoroutine(DeathResetCoroutine());
    }

    private IEnumerator DeathResetCoroutine()
    {
        isResetting = true;
        yield return new WaitForSeconds(deathResetDelay);
        ResetLoop();
        isResetting = false;
    }

    public void ResetLoop()
    {
        Debug.Log("RESETTING LOOP");

        // LEVEL 2: Clone & Corpse Puzzle Mode
        if (cloneSpawner != null || recorder != null)
        {
            List<ReplayRecorder.Frame> recordedFrames = null;
            if (recorder != null)
            {
                recordedFrames = recorder.GetFramesCopy();
            }

            // Spawn the clone playing back the previous attempt
            if (cloneSpawner != null && recordedFrames != null && recordedFrames.Count > 0)
            {
                cloneSpawner.SpawnClone(recordedFrames);
            }

            // Reset the player back to start
            if (playerDeath != null)
            {
                playerDeath.ResetPlayer(startPosition, startRotation);
            }
            else if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.position = startPosition;
                player.rotation = startRotation;
                Physics.SyncTransforms();

                if (cc != null) cc.enabled = true;
            }

            // Restart recorder for the new loop
            if (recorder != null)
            {
                recorder.ClearRecording();
                recorder.StartRecording();
            }

            timer = loopDuration;
            return;
        }

        // LEVEL 1: Ghost Replay Mode
        completedLoops.Add(currentRecording);

        if (ghostPrefab != null)
        {
            GameObject ghostObject = Instantiate(ghostPrefab, startPosition, startRotation);
            ghostObject.tag = "Ghost";

            GhostReplay ghost = ghostObject.GetComponent<GhostReplay>();
            if (ghost != null)
            {
                ghost.SetRecording(currentRecording);
            }

            GhostReplay[] allGhosts = FindObjectsByType<GhostReplay>(FindObjectsSortMode.None);
            foreach (var g in allGhosts)
            {
                g.ResetReplay();
            }
        }

        if (player != null)
        {
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
        }

        currentRecording = new LoopRecording();
        timer = loopDuration;
    }

    private void RecordPlayerGhost()
    {
        if (player == null)
            return;

        float currentTime = loopDuration - timer;
        PlayerFrame frame = new PlayerFrame(
            currentTime,
            player.position,
            player.rotation
        );

        currentRecording.frames.Add(frame);
    }

    public float GetTimeLeft()
    {
        return timer;
    }
}
