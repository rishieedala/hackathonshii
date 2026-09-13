using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Master coordinator for LEVEL 3: CLONE TEAMWORK.
/// Manages player recording on Run 1, clone spawning and playback,
/// player reset back to spawn, Run 2 teamwork, and pit hazard respawns.
/// Strictly ZERO on-screen text during gameplay.
/// </summary>
public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }

    public enum LevelState
    {
        Run1_Recording,
        Run2_ReplayingAndPlaying,
        Completed
    }

    [Header("Level State")]
    [SerializeField] private LevelState currentState = LevelState.Run1_Recording;
    public LevelState CurrentState => currentState;

    [Header("Spawn Configuration")]
    public Vector3 spawnPosition = new Vector3(0f, 1.2f, 2f);
    public Quaternion spawnRotation = Quaternion.identity;

    [Header("References")]
    public GameObject clonePrefab;
    public Level3Button buttonA;
    public Level3Button buttonB;
    public Level3Door exitDoor;

    [Header("Hazards")]
    public float pitKillY = -3.5f;

    private PlayerController playerController;
    private CharacterController characterController;
    private ReplayRecorder playerRecorder;
    private GameObject activeClone;
    private CloneReplay activeCloneReplay;
    private AudioSource audioSource;
    private AudioClip resetSound;
    private AudioClip cloneSpawnSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        resetSound = CreateWhooshClip();
        cloneSpawnSound = CreateCloneChordClip();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Clean up any stray ghosts or clones from previous scenes/runs
        GhostReplay[] oldGhosts = FindObjectsByType<GhostReplay>(FindObjectsInactive.Include);
        for (int i = 0; i < oldGhosts.Length; i++)
            Destroy(oldGhosts[i].gameObject);

        CloneReplay[] oldClones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Include);
        for (int i = 0; i < oldClones.Length; i++)
            Destroy(oldClones[i].gameObject);

        // Auto-link buttons/door if not assigned in Inspector
        if (buttonA == null || buttonB == null)
        {
            Level3Button[] buttons = FindObjectsByType<Level3Button>(FindObjectsInactive.Include);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].isButtonA && buttonA == null) buttonA = buttons[i];
                else if (!buttons[i].isButtonA && buttonB == null) buttonB = buttons[i];
            }
        }

        if (exitDoor == null)
        {
            exitDoor = FindAnyObjectByType<Level3Door>();
        }

        // Setup Player
        InitializePlayer();

        currentState = LevelState.Run1_Recording;
    }

    private void InitializePlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            PlayerController foundPc = FindAnyObjectByType<PlayerController>();
            if (foundPc != null) playerObj = foundPc.gameObject;
        }

        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            characterController = playerObj.GetComponent<CharacterController>();
            playerRecorder = playerObj.GetComponent<ReplayRecorder>();

            if (playerRecorder == null)
            {
                playerRecorder = playerObj.AddComponent<ReplayRecorder>();
            }

            // Ensure player starts at spawn point
            ResetPlayerToSpawn();

            playerRecorder.ClearRecording();
            playerRecorder.StartRecording();
        }
    }

    private void Update()
    {
        // Check for manual level restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
            return;
        }

        // Pit Hazard Check
        if (playerController != null && playerController.transform.position.y < pitKillY)
        {
            OnPlayerFall();
        }
    }

    /// <summary>
    /// Triggered the instant the player steps onto Button A at the end of Path A.
    /// Stops recording, creates the clone, launches clone replay, and resets player to start.
    /// </summary>
    public void OnButtonAPressedByPlayer()
    {
        if (currentState != LevelState.Run1_Recording)
            return;

        if (playerRecorder == null)
            return;

        currentState = LevelState.Run2_ReplayingAndPlaying;

        // 1. Stop recording and capture frames
        playerRecorder.StopRecording();
        List<ReplayRecorder.Frame> recordedFrames = playerRecorder.GetFramesCopy();

        if (recordedFrames == null || recordedFrames.Count == 0)
        {
            Debug.LogWarning("Level3Manager: No frames captured during Run 1!");
            return;
        }

        // 2. Play audio feedback
        if (audioSource != null && cloneSpawnSound != null)
        {
            audioSource.PlayOneShot(cloneSpawnSound, 0.9f);
        }

        // 3. Spawn and configure the Clone
        SpawnClone(recordedFrames);

        // 4. Reset real player back to start point
        ResetPlayerToSpawn();

        Debug.Log("Level3Manager: Button A reached! Clone spawned and replaying. Player reset to start point for Run 2.");
    }

    public void OnButtonBPressed()
    {
        Debug.Log("Level3Manager: Button B activated by player!");
    }

    private void SpawnClone(List<ReplayRecorder.Frame> recordedFrames)
    {
        if (activeClone != null)
        {
            Destroy(activeClone);
        }

        Vector3 spawnPos = recordedFrames[0].position;
        Quaternion spawnRot = recordedFrames[0].rotation;

        if (clonePrefab != null)
        {
            activeClone = Instantiate(clonePrefab, spawnPos, spawnRot);
        }
        else
        {
            // Fallback: create clone capsule if prefab missing
            activeClone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            activeClone.name = "Clone";
            activeClone.transform.position = spawnPos;
            activeClone.transform.rotation = spawnRot;
            activeClone.tag = "Ghost";
        }

        activeCloneReplay = activeClone.GetComponent<CloneReplay>();
        if (activeCloneReplay == null)
        {
            activeCloneReplay = activeClone.AddComponent<CloneReplay>();
        }

        // Remove unnecessary components if present
        GhostReplay ghost = activeClone.GetComponent<GhostReplay>();
        if (ghost != null) Destroy(ghost);

        // Configure clone behavior
        activeCloneReplay.holdFinalPosition = true;
        activeCloneReplay.SetRecording(recordedFrames);
        activeCloneReplay.StartReplay();

        // Style the clone with a distinctive futuristic holographic cyan tint
        ApplyCloneStyling(activeClone);
    }

    private void ApplyCloneStyling(GameObject cloneObj)
    {
        Renderer[] renderers = cloneObj.GetComponentsInChildren<Renderer>();
        Color cloneColor = new Color(0f, 0.95f, 1f, 0.9f); // Brilliant Cyan
        Color cloneEmission = new Color(0f, 0.8f, 0.95f) * 1.5f;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = cloneColor;
            mat.SetColor("_EmissionColor", cloneEmission);
            mat.EnableKeyword("_EMISSION");
            renderers[i].material = mat;
        }

        // Add a gentle point light to illuminate the clone
        Light cloneLight = cloneObj.AddComponent<Light>();
        cloneLight.type = LightType.Point;
        cloneLight.color = cloneColor;
        cloneLight.intensity = 2.0f;
        cloneLight.range = 3.5f;
    }

    private void OnPlayerFall()
    {
        if (playerController != null && playerController.transform.position.z > 28f)
        {
            if (Level3RaceManager.Instance != null)
            {
                Level3RaceManager.Instance.OnPlayerFallInRace();
                return;
            }
        }

        if (audioSource != null && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound, 0.7f);
        }

        ResetPlayerToSpawn();

        if (currentState == LevelState.Run1_Recording)
        {
            // Clear and restart recording for a clean attempt
            if (playerRecorder != null)
            {
                playerRecorder.ClearRecording();
                playerRecorder.StartRecording();
            }
            if (buttonA != null)
            {
                buttonA.ForceActive(false);
            }
            Debug.Log("Level3Manager: Player fell during Run 1. Recording reset.");
        }
        else if (currentState == LevelState.Run2_ReplayingAndPlaying)
        {
            // During Run 2, player simply retries Path B while clone holds Button A
            Debug.Log("Level3Manager: Player fell during Run 2. Respawned at start.");
        }
    }

    public void ResetPlayerToSpawn()
    {
        if (playerController == null)
            return;

        if (characterController != null)
            characterController.enabled = false;

        playerController.transform.position = spawnPosition;
        playerController.transform.rotation = spawnRotation;
        Physics.SyncTransforms();

        if (characterController != null)
            characterController.enabled = true;

        playerController.ResetVelocity();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private AudioClip CreateWhooshClip()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            float noise = (Random.value * 2f - 1f) * 0.4f;
            float tone = Mathf.Sin(2 * Mathf.PI * 180f * t) * 0.3f;
            samples[i] = (noise + tone) * env;
        }

        AudioClip clip = AudioClip.Create("ResetWhoosh", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateCloneChordClip()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Major triad chord (C5, E5, G5)
        float f1 = 523.25f;
        float f2 = 659.25f;
        float f3 = 783.99f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float decay = Mathf.Exp(-t * 4.5f);
            float wave = (Mathf.Sin(2 * Mathf.PI * f1 * t) +
                          Mathf.Sin(2 * Mathf.PI * f2 * t) +
                          Mathf.Sin(2 * Mathf.PI * f3 * t)) / 3f;
            samples[i] = wave * decay * 0.6f;
        }

        AudioClip clip = AudioClip.Create("CloneSpawnChord", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
