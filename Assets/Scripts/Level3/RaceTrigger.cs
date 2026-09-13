using UnityEngine;

/// <summary>
/// Trigger component for the "Race Your Clone" section in Level 3.
/// Used for both the Race Start Button and the Race Finish Line.
/// </summary>
public class RaceTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        StartButton,
        FinishLine
    }

    [Header("Configuration")]
    public TriggerType triggerType = TriggerType.StartButton;

    [Header("Visuals (for Start Button)")]
    public Transform buttonTop;
    public Renderer buttonRenderer;
    public Light buttonLight;
    public Color readyColor = new Color(0.1f, 0.8f, 1f);   // Bright Neon Cyan
    public Color activeColor = new Color(0.1f, 1f, 0.4f);  // Emerald Green

    private Vector3 initialTopPos;
    private Vector3 pressedTopPos;
    private Material runtimeMat;
    private bool isPressed = false;
    private Transform cachedPlayer;

    private void Awake()
    {
        if (triggerType == TriggerType.StartButton)
        {
            if (buttonTop != null)
            {
                initialTopPos = buttonTop.localPosition;
                pressedTopPos = initialTopPos - new Vector3(0f, 0.08f, 0f);
            }
            if (buttonRenderer != null)
            {
                runtimeMat = buttonRenderer.material;
            }
            SetVisualState(false);
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) cachedPlayer = playerObj.transform;
    }

    private void Update()
    {
        if (triggerType == TriggerType.StartButton)
        {
            // Spatial check for stepping onto start button
            if (cachedPlayer == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) cachedPlayer = p.transform;
            }

            if (cachedPlayer != null && !isPressed)
            {
                Vector3 pPos = cachedPlayer.position;
                Vector3 myPos = transform.position;
                float xzDist = Vector2.Distance(new Vector2(pPos.x, pPos.z), new Vector2(myPos.x, myPos.z));
                float yDiff = pPos.y - myPos.y;

                if (xzDist < 1.3f && yDiff >= -0.4f && yDiff <= 2.0f)
                {
                    OnStartActivated();
                }
            }

            // Animate button depression smoothly
            if (buttonTop != null)
            {
                Vector3 target = isPressed ? pressedTopPos : initialTopPos;
                buttonTop.localPosition = Vector3.Lerp(buttonTop.localPosition, target, Time.deltaTime * 10f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.StartButton)
        {
            if (other.CompareTag("Player") && !isPressed)
            {
                OnStartActivated();
            }
        }
        else if (triggerType == TriggerType.FinishLine)
        {
            if (other.CompareTag("Player"))
            {
                if (Level3RaceManager.Instance != null)
                {
                    Level3RaceManager.Instance.OnFinishLineCrossed(true);
                }
            }
            else if (other.GetComponent<CloneReplay>() != null || other.GetComponentInParent<CloneReplay>() != null)
            {
                if (Level3RaceManager.Instance != null)
                {
                    Level3RaceManager.Instance.OnFinishLineCrossed(false);
                }
            }
        }
    }

    private void OnStartActivated()
    {
        isPressed = true;
        SetVisualState(true);

        if (Level3RaceManager.Instance != null)
        {
            Level3RaceManager.Instance.OnStartButtonPressed();
        }
    }

    public void ResetStartButton()
    {
        isPressed = false;
        SetVisualState(false);
    }

    private void SetVisualState(bool active)
    {
        Color c = active ? activeColor : readyColor;
        if (runtimeMat != null)
        {
            runtimeMat.color = c;
            runtimeMat.SetColor("_EmissionColor", c * (active ? 2.5f : 1.2f));
            runtimeMat.EnableKeyword("_EMISSION");
        }
        if (buttonLight != null)
        {
            buttonLight.color = c;
            buttonLight.intensity = active ? 2.5f : 1.5f;
        }
    }

    private void OnDestroy()
    {
        if (runtimeMat != null) Destroy(runtimeMat);
    }
}
