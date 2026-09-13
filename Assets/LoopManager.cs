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

    // Level 1 recording for the current loop
    private LoopRecording currentRecording;

    // Only ONE ghost is allowed in Level 1.
    // Always represents the immediately previous loop.
    private GameObject currentGhost;

    // Shared re-entry guard. Prevents ResetLoop() from being called twice
    // on the same frame (e.g. timer hits 0 across two consecutive frames).
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
        // ── LEVEL 1: Ghost Replay Mode ──────────────────────────────────────
        if (ghostPrefab != null)
        {
            if (!isResetting)
            {
                timer -= Time.deltaTime;

                // Record every frame while the loop is running
                RecordPlayerGhost();

                if (timer <= 0f)
                {
                    ResetLoop();
                }
            }
        }
    }

    // ── Level 2: Called when the player dies ────────────────────────────────
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
        // ── LEVEL 1: Ghost Replay Mode ──────────────────────────────────────
        if (ghostPrefab != null)
        {
            if (isResetting)
                return;

            isResetting = true;

            Debug.Log("LoopManager: LEVEL 1 RESET");

            // ── 1. Snap timer to exactly 0 so Update skips recording ────────
            timer = 0f;

            // ── 2. Destroy the OLD ghost (previous loop's ghost) ────────────
            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }

            // ── 3. Reset all pressure plates to inactive ────────────────────
            PressurePlate[] plates = FindObjectsByType<PressurePlate>(FindObjectsInactive.Exclude);
            foreach (var plate in plates)
            {
                if (plate != null)
                {
                    plate.ResetPlate();
                }
            }

            // ── 4. Spawn the NEW ghost from ONLY the recording that just finished
            if (currentRecording != null && currentRecording.frames.Count > 0)
            {
                currentGhost = Instantiate(
                    ghostPrefab,
                    startPosition,
                    startRotation
                );

                currentGhost.tag = "Ghost";

                GhostReplay ghost = currentGhost.GetComponent<GhostReplay>();

                if (ghost != null)
                {
                    ghost.SetRecording(currentRecording);
                    ghost.ResetReplay();
                }
                else
                {
                    Debug.LogWarning("LoopManager: GhostReplay component not found on Ghost prefab!");
                }
            }

            // ── 5. The plate's self-healing cache in CheckSpatialOverlap() will
            //      automatically detect the new ghost on the next Update() frame.
            //      No explicit cache refresh call needed here.

            // ── 6. Teleport/reset the Player to the starting position ────────
            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.ResetVelocity();
                }

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // Disable CharacterController before teleporting to prevent clamping
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                }

                player.position = startPosition;
                player.rotation = startRotation;

                if (cc != null)
                {
                    cc.enabled = true;
                }

                Physics.SyncTransforms();
            }

            // ── 7. Clear the old recording and begin a new one ───────────────
            currentRecording = new LoopRecording();

            // ── 8. Restart the 10-second timer ──────────────────────────────
            timer = loopDuration;

            isResetting = false;

            Debug.Log("LoopManager: Loop reset complete. One ghost active (previous loop only).");

            return; // ← Do NOT fall through to Level 2 path
        }

        // ── LEVEL 2: Clone & Corpse Puzzle Mode ─────────────────────────────
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

    // ── Private Helpers ─────────────────────────────────────────────────────

    private void RecordPlayerGhost()
    {
        if (player == null)
            return;

        // Record elapsed time from start of this loop
        float currentTime = loopDuration - timer;

        if (currentTime < 0f)
            currentTime = 0f;

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
