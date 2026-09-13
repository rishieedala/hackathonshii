using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level 2 only. Manages the clone/corpse/death loop system.
/// Freely edit this file for Level 2 — it has no effect on Level 1.
/// </summary>
public class LoopManager2 : MonoBehaviour
{
    public static LoopManager2 Instance { get; private set; }

    [Header("Level 2 Clone & Death Settings")]
    public float loopDuration = 10f;
    public Transform player;
    public CloneSpawner cloneSpawner;
    public ReplayRecorder recorder;
    public PlayerDeath playerDeath;
    public Transform startPoint;
    public float deathResetDelay = 1.2f;

    private float timer;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isResetting = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = loopDuration;

        if (player == null)
        {
            GameObject playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null) player = playerGo.transform;
        }

        if (player != null)
        {
            startPosition = (startPoint != null) ? startPoint.position : player.position;
            startRotation = (startPoint != null) ? startPoint.rotation : player.rotation;

            if (playerDeath == null) playerDeath = player.GetComponent<PlayerDeath>();
            if (recorder == null)   recorder   = player.GetComponent<ReplayRecorder>();
        }

        if (cloneSpawner == null)
        {
            cloneSpawner = GetComponent<CloneSpawner>();
            if (cloneSpawner == null) cloneSpawner = FindAnyObjectByType<CloneSpawner>();
        }
    }

    private void Update()
    {
        // Level 2 does NOT use a forced auto-loop timer —
        // loops reset when the player dies or manually triggers a reset zone.
    }

    // Called when the player dies (Level 2)
    public void OnPlayerDeath()
    {
        if (isResetting) return;

        if (recorder != null) recorder.StopRecording();

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
        Debug.Log("LoopManager2 (Level 2): RESET");

        List<ReplayRecorder.Frame> recordedFrames = null;
        if (recorder != null) recordedFrames = recorder.GetFramesCopy();

        // Destroy any living clones (keeps corpses intact)
        CloneReplay[] livingClones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
        foreach (var c in livingClones)
            if (c != null) Destroy(c.gameObject);

        // Spawn clone with recorded frames
        if (cloneSpawner != null && recordedFrames != null && recordedFrames.Count > 0)
            cloneSpawner.SpawnClone(recordedFrames);

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

        // Clear and restart recording
        if (recorder != null)
        {
            recorder.ClearRecording();
            recorder.StartRecording();
        }

        timer = loopDuration;
    }

    public float GetTimeLeft()
    {
        return timer;
    }
}
