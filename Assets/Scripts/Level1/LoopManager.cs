using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level 1 only. Manages the 10-second compulsory ghost-replay loop.
/// Do NOT modify this file for Level 2 work — use LoopManager2.cs instead.
/// </summary>
public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance { get; private set; }

    [Header("Level 1 Ghost Settings")]
    public float loopDuration = 10f;
    public Transform player;
    public GameObject ghostPrefab;
    public Transform startPoint;

    private float timer;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private LoopRecording currentRecording;
    private GameObject currentGhost;
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
        }

        currentRecording = new LoopRecording();
    }

    private void Update()
    {
        if (isResetting) return;

        timer -= Time.deltaTime;
        RecordPlayerGhost();

        if (timer <= 0f)
        {
            ResetLoop();
        }
    }

    public void ResetLoop()
    {
        if (isResetting) return;
        isResetting = true;

        Debug.Log("LoopManager (Level 1): RESET");

        timer = 0f;

        // Destroy old ghost
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }

        // Reset pressure plates
        PressurePlate[] plates = FindObjectsByType<PressurePlate>(FindObjectsInactive.Exclude);
        foreach (var plate in plates)
            if (plate != null) plate.ResetPlate();

        // Spawn new ghost from the just-finished recording
        if (currentRecording != null && currentRecording.frames.Count > 0)
        {
            currentGhost = Instantiate(ghostPrefab, startPosition, startRotation);
            currentGhost.tag = "Ghost";

            GhostReplay ghost = currentGhost.GetComponent<GhostReplay>();
            if (ghost != null)
            {
                ghost.SetRecording(currentRecording);
                ghost.ResetReplay();
            }
            else
            {
                Debug.LogWarning("LoopManager: GhostReplay component missing on ghost prefab!");
            }
        }

        // Reset player
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.ResetVelocity();

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = startPosition;
            player.rotation = startRotation;
            if (cc != null) cc.enabled = true;

            Physics.SyncTransforms();
        }

        // Start fresh recording
        currentRecording = new LoopRecording();
        timer = loopDuration;

        isResetting = false;
        Debug.Log("LoopManager (Level 1): Loop reset complete.");
    }

    private void RecordPlayerGhost()
    {
        if (player == null) return;

        float currentTime = loopDuration - timer;
        if (currentTime < 0f) currentTime = 0f;

        currentRecording.frames.Add(new PlayerFrame(currentTime, player.position, player.rotation));
    }

    public float GetTimeLeft()
    {
        return timer;
    }
}
