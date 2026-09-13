using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates the "RACE YOUR CLONE" section in Level 3.
/// Manages First Run recording, Ghost Clone playback with ghost trails,
/// real-time AHEAD/BEHIND tracking, victory door unlocking, and seamless retries.
/// </summary>
public class Level3RaceManager : MonoBehaviour
{
    public static Level3RaceManager Instance { get; private set; }

    public enum RaceState
    {
        Idle,
        FirstRun_Recording,
        PreparingRace,
        Racing,
        PlayerWon,
        CloneWon
    }

    [Header("State")]
    [SerializeField] private RaceState currentState = RaceState.Idle;
    public RaceState CurrentState => currentState;

    [Header("Race Waypoints")]
    public Vector3 raceStartPosition = new Vector3(0f, 1.4f, 32f);
    public Quaternion raceStartRotation = Quaternion.identity;

    [Header("References")]
    public GameObject clonePrefab;
    public RaceTrigger startButton;
    public RaceTrigger finishLine;
    public Transform finalDoorMovingPanel;
    public float finalDoorOpenHeight = 4.0f;

    private List<ReplayRecorder.Frame> firstRunFrames;
    private GameObject raceClone;
    private CloneReplay raceCloneReplay;

    private PlayerController playerController;
    private CharacterController characterController;
    private ReplayRecorder playerRecorder;
    private AudioSource audioSource;

    private bool finalDoorOpened = false;
    private Vector3 doorClosedPos;
    private Vector3 doorOpenPos;

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

        if (finalDoorMovingPanel != null)
        {
            doorClosedPos = finalDoorMovingPanel.position;
            doorOpenPos = doorClosedPos + new Vector3(0f, finalDoorOpenHeight, 0f);
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            playerController = p.GetComponent<PlayerController>();
            characterController = p.GetComponent<CharacterController>();
            playerRecorder = p.GetComponent<ReplayRecorder>();
            if (playerRecorder == null) playerRecorder = p.AddComponent<ReplayRecorder>();
        }
    }

    private void Update()
    {
        if (playerController == null) FindPlayer();



        // Animate final door opening
        if (finalDoorOpened && finalDoorMovingPanel != null)
        {
            finalDoorMovingPanel.position = Vector3.Lerp(finalDoorMovingPanel.position, doorOpenPos, Time.deltaTime * 3.5f);
        }

        // Fall check for Race Arena (Z > 28)
        if (playerController != null && playerController.transform.position.z > 28f && playerController.transform.position.y < -3.5f)
        {
            OnPlayerFallInRace();
        }
    }

    /// <summary>
    /// Triggered when the player steps on the Race Start button.
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (currentState == RaceState.Idle)
        {
            // Begin First Run
            currentState = RaceState.FirstRun_Recording;
            if (playerRecorder != null)
            {
                playerRecorder.ClearRecording();
                playerRecorder.StartRecording();
            }

            ShowBanner("GO! REACH THE FINISH LINE!", 2.5f, new Color(0.1f, 1f, 0.4f));
            PlayTone(659.25f, 0.35f); // E5 tone
            Debug.Log("Level3RaceManager: First Run started! Recording player movement...");
        }
        else if (currentState == RaceState.CloneWon)
        {
            // Retry the race against the existing clone recording
            StartCoroutine(PrepareAndLaunchRaceRoutine());
        }
    }

    /// <summary>
    /// Triggered when Player or Clone touches the Finish Line trigger.
    /// </summary>
    public void OnFinishLineCrossed(bool isPlayer)
    {
        if (currentState == RaceState.FirstRun_Recording && isPlayer)
        {
            // Completed first run!
            if (playerRecorder == null) return;
            playerRecorder.StopRecording();
            firstRunFrames = playerRecorder.GetFramesCopy();

            if (firstRunFrames == null || firstRunFrames.Count < 5)
            {
                Debug.LogWarning("Level3RaceManager: Recording too short!");
                return;
            }

            Debug.Log($"Level3RaceManager: First Run recorded with {firstRunFrames.Count} frames. Preparing Race...");
            StartCoroutine(PrepareAndLaunchRaceRoutine());
        }
        else if (currentState == RaceState.Racing)
        {
            if (isPlayer)
            {
                // PLAYER WINS!
                currentState = RaceState.PlayerWon;
                ShowBanner("YOU WIN! YOU BEAT YOUR CLONE!", 6.0f, new Color(0.1f, 1f, 0.5f));
                PlayVictoryChime();

                finalDoorOpened = true;

                if (raceCloneReplay != null)
                {
                    raceCloneReplay.holdFinalPosition = true;
                }

                Debug.Log("Level3RaceManager: PLAYER WON THE RACE! Final door opened!");
            }
            else
            {
                // CLONE REACHED FIRST!
                currentState = RaceState.CloneWon;
                ShowBanner("CLONE WON! STEP ON START TO RETRY", 4.0f, new Color(1f, 0.4f, 0.2f));
                PlayFailBuzzer();

                // Teleport player back to start after a brief moment
                StartCoroutine(ResetPlayerToStartAfterDelay(1.5f));

                Debug.Log("Level3RaceManager: Clone reached finish first. Player can retry!");
            }
        }
    }

    private IEnumerator PrepareAndLaunchRaceRoutine()
    {
        currentState = RaceState.PreparingRace;

        // 1. Reset real player to Race Start
        TeleportPlayerToRaceStart();

        // 2. Spawn and configure the Race Clone
        SpawnRaceClone();

        // 3. Short countdown
        ShowBanner("READY...", 1.0f, Color.yellow);
        PlayTone(440f, 0.2f);
        yield return new WaitForSeconds(1.0f);

        ShowBanner("SET...", 1.0f, Color.yellow);
        PlayTone(440f, 0.2f);
        yield return new WaitForSeconds(1.0f);

        // 4. GO! Launch Race!
        currentState = RaceState.Racing;
        ShowBanner("GO! BEAT YOUR PREVIOUS RUN!", 2.0f, new Color(0.1f, 1f, 0.4f));
        PlayTone(880f, 0.4f);

        if (raceCloneReplay != null)
        {
            raceCloneReplay.SetRecording(firstRunFrames);
            raceCloneReplay.StartReplay();
        }
    }

    private void SpawnRaceClone()
    {
        if (raceClone != null)
        {
            Destroy(raceClone);
        }

        Vector3 spawnPos = firstRunFrames[0].position;
        Quaternion spawnRot = firstRunFrames[0].rotation;

        if (clonePrefab != null)
        {
            raceClone = Instantiate(clonePrefab, spawnPos, spawnRot);
        }
        else
        {
            raceClone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            raceClone.transform.position = spawnPos;
            raceClone.transform.rotation = spawnRot;
        }

        raceClone.name = "RaceClone";
        raceClone.tag = "Ghost";

        raceCloneReplay = raceClone.GetComponent<CloneReplay>();
        if (raceCloneReplay == null) raceCloneReplay = raceClone.AddComponent<CloneReplay>();

        GhostReplay ghost = raceClone.GetComponent<GhostReplay>();
        if (ghost != null) Destroy(ghost);

        raceCloneReplay.holdFinalPosition = true;

        // Apply distinct Ghost styling (Translucent Hologram Cyan)
        Renderer[] rends = raceClone.GetComponentsInChildren<Renderer>();
        Color ghostColor = new Color(0f, 0.9f, 1f, 0.85f);
        for (int i = 0; i < rends.Length; i++)
        {
            Material m = new Material(Shader.Find("Standard"));
            m.color = ghostColor;
            m.SetColor("_EmissionColor", ghostColor * 1.6f);
            m.EnableKeyword("_EMISSION");
            rends[i].material = m;
        }

        // Add subtle, elegant Ghost Trail
        TrailRenderer trail = raceClone.GetComponent<TrailRenderer>();
        if (trail == null) trail = raceClone.AddComponent<TrailRenderer>();
        trail.time = 0.5f;
        trail.startWidth = 0.45f;
        trail.endWidth = 0.05f;
        trail.autodestruct = false;
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = new Color(0f, 0.95f, 1f, 0.45f);
        trail.material = trailMat;
    }

    private void TeleportPlayerToRaceStart()
    {
        if (playerController == null) return;

        if (characterController != null) characterController.enabled = false;
        playerController.transform.position = raceStartPosition;
        playerController.transform.rotation = raceStartRotation;
        Physics.SyncTransforms();
        if (characterController != null) characterController.enabled = true;

        playerController.ResetVelocity();
    }

    private IEnumerator ResetPlayerToStartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TeleportPlayerToRaceStart();
        if (startButton != null) startButton.ResetStartButton();
    }

    public void OnPlayerFallInRace()
    {
        TeleportPlayerToRaceStart();

        if (currentState == RaceState.FirstRun_Recording)
        {
            if (playerRecorder != null)
            {
                playerRecorder.ClearRecording();
                playerRecorder.StartRecording();
            }
            ShowBanner("FELL OFF! RUN 1 RESTARTED", 2.0f, Color.white);
        }
        else if (currentState == RaceState.Racing)
        {
            currentState = RaceState.CloneWon;
            ShowBanner("FELL OFF! STEP ON START TO RETRY", 2.5f, Color.white);
            if (startButton != null) startButton.ResetStartButton();
        }
    }

    private void ShowBanner(string msg, float duration, Color col)
    {
        // Zero on-screen text: Keep audio cues, animations, and console logs only
        Debug.Log($"[Level3Race] {msg}");
    }

    private void PlayTone(float freq, float duration)
    {
        if (audioSource == null) return;
        int sampleRate = 44100;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.5f;
        }
        AudioClip clip = AudioClip.Create("Tone_" + freq, count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, 0.8f);
    }

    private void PlayVictoryChime()
    {
        if (audioSource == null) return;
        int sampleRate = 44100;
        float duration = 1.2f;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];

        // Major triad fanfare
        float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.50f };
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 2.5f);
            float wave = 0f;
            for (int f = 0; f < freqs.Length; f++)
            {
                wave += Mathf.Sin(2 * Mathf.PI * freqs[f] * t);
            }
            samples[i] = (wave / freqs.Length) * env * 0.7f;
        }
        AudioClip clip = AudioClip.Create("VictoryFanfare", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, 0.9f);
    }

    private void PlayFailBuzzer()
    {
        if (audioSource == null) return;
        int sampleRate = 44100;
        float duration = 0.5f;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            float wave = Mathf.Sin(2 * Mathf.PI * 130f * t) * 0.4f + Mathf.Sin(2 * Mathf.PI * 125f * t) * 0.4f;
            samples[i] = wave * env * 0.5f;
        }
        AudioClip clip = AudioClip.Create("FailBuzzer", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, 0.7f);
    }
}
