using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;
    public bool stayActivated = true;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.7f, 0.9f, 0.9f);
    public Color activeColor = new Color(0.2f, 1f, 0.3f);

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Vector3 initialLocalPos;
    private Renderer plateRenderer;
    private GameObject cachedPlayer;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        plateRenderer = GetComponent<Renderer>();
        cachedPlayer = GameObject.FindWithTag("Player");
        UpdateVisuals(false);
    }

    public void TriggerPlate()
    {
        if (!activated)
        {
            activated = true;
            UpdateVisuals(true);
            Debug.Log("PRESSURE PLATE ACTIVATED! BLAST DOOR UNLOCKED!");

            // Direct notification to ExitDoor
            ExitDoor door = FindAnyObjectByType<ExitDoor>();
            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsTriggerEntity(other))
        {
            occupants.Add(other);
            TriggerPlate();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (IsTriggerEntity(other) && !activated)
        {
            TriggerPlate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsTriggerEntity(other))
        {
            occupants.Remove(other);
            if (!stayActivated)
            {
                activated = occupants.Count > 0;
                UpdateVisuals(activated);
            }
        }
    }

    void Update()
    {
        // Proximity / bounding box fail-safe check for player standing on plate
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindWithTag("Player");
        }

        if (cachedPlayer != null && !activated)
        {
            Vector3 diff = cachedPlayer.transform.position - transform.position;
            // Player is horizontally on top of plate and vertically within standing reach
            if (Mathf.Abs(diff.x) < 1.3f && Mathf.Abs(diff.z) < 1.3f && diff.y >= -0.3f && diff.y < 2.0f)
            {
                TriggerPlate();
            }
        }

        if (!stayActivated)
        {
            occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            activated = occupants.Count > 0;
            UpdateVisuals(activated);
        }

        // Smoothly sink down when activated
        Vector3 targetPos = initialLocalPos + (activated ? new Vector3(0, -0.07f, 0) : Vector3.zero);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 12f);
    }

    private void UpdateVisuals(bool isActive)
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = isActive ? activeColor : normalColor;
            if (plateRenderer.material.HasProperty("_EmissionColor"))
            {
                plateRenderer.material.EnableKeyword("_EMISSION");
                plateRenderer.material.SetColor("_EmissionColor", isActive ? activeColor * 3.5f : Color.black);
            }
        }
    }

    private bool IsTriggerEntity(Collider col)
    {
        if (col == null) return false;
        if (col.CompareTag("Player") || col.CompareTag("Ghost")) return true;
        if (col.attachedRigidbody != null && (col.attachedRigidbody.CompareTag("Player") || col.attachedRigidbody.CompareTag("Ghost"))) return true;
        if (col.transform.root != null && (col.transform.root.CompareTag("Player") || col.transform.root.CompareTag("Ghost"))) return true;
        return false;
    }
}
