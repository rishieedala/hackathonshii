using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance { get; private set; }

    [Header("Level 1 Ghost Settings")]
    public float loopDuration = 10f;
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

    // Called when the player dies (Level 2)
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

        // LEVEL 1: Ghost Replay Mode
        if (ghostPrefab != null)
        {
            completedLoops.Add(currentRecording);

            GameObject ghostObject = Instantiate(ghostPrefab, startPosition, startRotation);
            ghostObject.tag = "Ghost";

            GhostReplay ghost = ghostObject.GetComponent<GhostReplay>();
            if (ghost != null)
            {
                ghost.SetRecording(currentRecording);
            }

            GhostReplay[] allGhosts = FindObjectsByType<GhostReplay>(FindObjectsInactive.Exclude);
            foreach (var g in allGhosts)
            {
                if (g != null)
                {
                    g.ResetReplay();
                }
            }

            // Reset player position and velocities
            if (player != null)
            {
                player.position = startPosition;
                player.rotation = startRotation;

                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.ResetVelocity();
                }

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.position = startPosition;
                    rb.rotation = startRotation;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.position = startPosition;
                    player.rotation = startRotation;
                    cc.enabled = true;
                }

                Physics.SyncTransforms();
            }

            // In Level 1, reset pressure plate so ghost must reach and step on it in the new loop
            PressurePlate[] plates = FindObjectsByType<PressurePlate>(FindObjectsInactive.Exclude);
            foreach (var p in plates)
            {
                if (p != null)
                {
                    p.ResetPlate();
                }
            }

            currentRecording = new LoopRecording();
            timer = loopDuration;
            return;
        }

        // LEVEL 2: Clone & Corpse Puzzle Mode
        if (cloneSpawner != null || recorder != null)
        {
            List<ReplayRecorder.Frame> recordedFrames = null;
            if (recorder != null)
            {
                recordedFrames = recorder.GetFramesCopy();
            }

            // Clean up any previously living clones that didn't die yet (keeps corpses intact!)
            CloneReplay[] livingClones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
            foreach (var c in livingClones)
            {
                if (c != null)
                {
                    Destroy(c.gameObject);
                }
            }

            // Spawn clone with recorded actions
            if (cloneSpawner != null && recordedFrames != null && recordedFrames.Count > 0)
            {
                cloneSpawner.SpawnClone(recordedFrames);
            }

            // Reset player
            if (playerDeath != null)
            {
                playerDeath.ResetPlayer(startPosition, startRotation);
            }
            else if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.ResetVelocity();

                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.position = startPosition;
                player.rotation = startRotation;
                Physics.SyncTransforms();

                if (cc != null) cc.enabled = true;
            }

            // Clear and restart recording for new loop
            if (recorder != null)
            {
                recorder.ClearRecording();
                recorder.StartRecording();
            }

            timer = loopDuration;
        }
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
