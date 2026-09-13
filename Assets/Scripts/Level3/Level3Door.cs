using System.Collections;
using UnityEngine;

/// <summary>
/// Dual-activation exit gate for Level 3: Clone Teamwork.
/// Opens smoothly when both Button A and Button B are activated.
/// Features dual glowing status indicators and smooth vertical sliding animation.
/// </summary>
public class Level3Door : MonoBehaviour
{
    [Header("Linked Buttons")]
    public Level3Button buttonA;
    public Level3Button buttonB;

    [Header("Door Movement")]
    public Transform doorMovingPanel;
    public float openHeight = 3.6f;
    public float openSpeed = 4f;

    [Header("Indicators")]
    public Renderer indicatorRendererA;
    public Renderer indicatorRendererB;
    public Light indicatorLightA;
    public Light indicatorLightB;

    [Header("Indicator Colors")]
    public Color lockedColor = new Color(1f, 0.15f, 0.2f); // Ruby Red
    public Color unlockedColor = new Color(0.05f, 1f, 0.45f); // Emerald Green

    public bool IsOpen { get; private set; } = false;

    private Vector3 closedPos;
    private Vector3 openPos;
    private Material matA;
    private Material matB;
    private AudioSource audioSource;
    private AudioClip doorOpenClip;
    private bool hasPlayedOpenSound = false;

    private void Awake()
    {
        Transform targetTransform = doorMovingPanel != null ? doorMovingPanel : transform;
        closedPos = targetTransform.position;
        openPos = closedPos + new Vector3(0f, openHeight, 0f);

        if (indicatorRendererA != null)
            matA = indicatorRendererA.material;
        if (indicatorRendererB != null)
            matB = indicatorRendererB.material;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.6f;
        }

        doorOpenClip = CreateDoorHumClip();

        UpdateIndicators(false, false);
    }

    private void Update()
    {
        bool aActive = buttonA != null && buttonA.IsActive;
        bool bActive = buttonB != null && buttonB.IsActive;

        UpdateIndicators(aActive, bActive);

        bool shouldOpen = aActive && bActive;

        if (shouldOpen && !IsOpen)
        {
            IsOpen = true;
            if (!hasPlayedOpenSound && audioSource != null && doorOpenClip != null)
            {
                hasPlayedOpenSound = true;
                audioSource.PlayOneShot(doorOpenClip, 0.9f);
            }
            Debug.Log("Level3Door: BOTH BUTTONS ACTIVE - DOOR OPENING!");
        }
        else if (!shouldOpen && IsOpen)
        {
            IsOpen = false;
            hasPlayedOpenSound = false;
        }

        Transform targetTransform = doorMovingPanel != null ? doorMovingPanel : transform;
        Vector3 targetPos = IsOpen ? openPos : closedPos;
        targetTransform.position = Vector3.Lerp(targetTransform.position, targetPos, Time.deltaTime * openSpeed);
    }

    private void UpdateIndicators(bool aActive, bool bActive)
    {
        // Indicator A
        Color colA = aActive ? unlockedColor : lockedColor;
        if (matA != null)
        {
            matA.color = colA;
            matA.SetColor("_EmissionColor", colA * (aActive ? 3f : 0.8f));
            matA.EnableKeyword("_EMISSION");
        }
        if (indicatorLightA != null)
        {
            indicatorLightA.color = colA;
            indicatorLightA.intensity = aActive ? 2.5f : 0.8f;
        }

        // Indicator B
        Color colB = bActive ? unlockedColor : lockedColor;
        if (matB != null)
        {
            matB.color = colB;
            matB.SetColor("_EmissionColor", colB * (bActive ? 3f : 0.8f));
            matB.EnableKeyword("_EMISSION");
        }
        if (indicatorLightB != null)
        {
            indicatorLightB.color = colB;
            indicatorLightB.intensity = bActive ? 2.5f : 0.8f;
        }
    }

    private AudioClip CreateDoorHumClip()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            float freq = Mathf.Lerp(160f, 320f, t / duration);
            float wave = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f + Mathf.Sin(2 * Mathf.PI * (freq * 1.5f) * t) * 0.2f;
            samples[i] = wave * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("DoorOpenSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (matA != null) Destroy(matA);
        if (matB != null) Destroy(matB);
    }
}
