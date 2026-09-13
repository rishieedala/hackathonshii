using System.Collections;
using UnityEngine;

/// <summary>
/// Interactive button on a sturdy pedestal for Level 3: Clone Teamwork.
/// Features spatial and trigger detection, emissive color shifting,
/// smooth depression animation, and synthesized chime sound.
/// </summary>
public class Level3Button : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("True for Button A (left path), False for Button B (right path)")]
    public bool isButtonA = true;

    [Tooltip("If true, once pressed the button stays permanently active.")]
    public bool latchOnPress = true;

    [Header("Visual Components")]
    public Transform buttonTop;
    public Renderer buttonRenderer;
    public Light buttonLight;

    [Header("Colors")]
    public Color inactiveColor = new Color(1f, 0.15f, 0.2f); // Vibrant Ruby Red
    public Color activeColor = new Color(0.05f, 1f, 0.45f);   // Vibrant Emerald Green

    public bool IsActive { get; private set; } = false;

    private Vector3 initialTopLocalPos;
    private Vector3 pressedTopLocalPos;
    private Material runtimeMaterial;
    private AudioSource audioSource;
    private AudioClip chimeClip;
    private Transform cachedPlayer;

    private void Awake()
    {
        if (buttonTop != null)
        {
            initialTopLocalPos = buttonTop.localPosition;
            pressedTopLocalPos = initialTopLocalPos - new Vector3(0f, 0.08f, 0f);
        }

        if (buttonRenderer != null)
        {
            runtimeMaterial = buttonRenderer.material;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
        }

        chimeClip = CreateChimeClip(isButtonA ? 587.33f : 783.99f); // D5 or G5 note

        UpdateVisuals(false, true);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            cachedPlayer = playerObj.transform;
    }

    private void Update()
    {
        bool detectedOccupant = CheckOccupancy();

        if (detectedOccupant)
        {
            if (!IsActive)
            {
                ActivateButton();
            }
        }
        else if (!latchOnPress && IsActive)
        {
            DeactivateButton();
        }

        // Animate button depression smoothly
        if (buttonTop != null)
        {
            Vector3 targetPos = IsActive ? pressedTopLocalPos : initialTopLocalPos;
            buttonTop.localPosition = Vector3.Lerp(buttonTop.localPosition, targetPos, Time.deltaTime * 12f);
        }
    }

    private bool CheckOccupancy()
    {
        // 1. Check Player
        if (cachedPlayer == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) cachedPlayer = p.transform;
        }

        if (cachedPlayer != null && IsEntityOnButton(cachedPlayer.position))
            return true;

        // 2. Check Clone(s)
        CloneReplay[] clones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
        for (int i = 0; i < clones.Length; i++)
        {
            if (clones[i] != null && IsEntityOnButton(clones[i].transform.position))
                return true;
        }

        return false;
    }

    private bool IsEntityOnButton(Vector3 entityPos)
    {
        Vector3 myPos = transform.position;
        float xzDist = Vector2.Distance(new Vector2(entityPos.x, entityPos.z), new Vector2(myPos.x, myPos.z));
        float yDiff = entityPos.y - myPos.y;

        // Tolerant spatial box: within 1.4m horizontally and between -0.4m to +2.0m vertically
        return (xzDist < 1.4f && yDiff >= -0.4f && yDiff <= 2.2f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CloneReplay>() != null || other.GetComponentInParent<CloneReplay>() != null)
        {
            if (!IsActive)
            {
                ActivateButton();
            }
        }
    }

    private void ActivateButton()
    {
        IsActive = true;
        UpdateVisuals(true, false);

        if (audioSource != null && chimeClip != null)
        {
            audioSource.PlayOneShot(chimeClip, 0.85f);
        }

        // Notify Level3Manager
        if (Level3Manager.Instance != null)
        {
            if (isButtonA)
            {
                Level3Manager.Instance.OnButtonAPressedByPlayer();
            }
            else
            {
                Level3Manager.Instance.OnButtonBPressed();
            }
        }

        Debug.Log($"Level3Button [{(isButtonA ? "A" : "B")}] ACTIVATED!");
    }

    private void DeactivateButton()
    {
        IsActive = false;
        UpdateVisuals(false, false);
    }

    public void ForceActive(bool active)
    {
        IsActive = active;
        UpdateVisuals(active, false);
    }

    private void UpdateVisuals(bool active, bool immediate)
    {
        Color targetColor = active ? activeColor : inactiveColor;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.color = targetColor;
            runtimeMaterial.SetColor("_EmissionColor", targetColor * (active ? 2.5f : 0.8f));
            runtimeMaterial.EnableKeyword("_EMISSION");
        }

        if (buttonLight != null)
        {
            buttonLight.color = targetColor;
            buttonLight.intensity = active ? 3.0f : 1.2f;
            buttonLight.range = active ? 6.0f : 4.0f;
        }
    }

    /// <summary>
    /// Generates a synthesized bell chime clip in code without external assets.
    /// </summary>
    private AudioClip CreateChimeClip(float frequency)
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float decay = Mathf.Exp(-t * 8f);
            // Fundamental tone + overtone harmonic
            float tone1 = Mathf.Sin(2 * Mathf.PI * frequency * t);
            float tone2 = Mathf.Sin(2 * Mathf.PI * frequency * 2.01f * t) * 0.4f;
            float tone3 = Mathf.Sin(2 * Mathf.PI * frequency * 3.02f * t) * 0.2f;
            samples[i] = (tone1 + tone2 + tone3) * decay * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Chime_" + frequency, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
