using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;
    public bool stayActivated = false;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.7f, 0.9f, 0.9f);
    public Color activeColor = new Color(0.2f, 1f, 0.3f);

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Vector3 initialLocalPos;
    private Renderer plateRenderer;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        plateRenderer = GetComponent<Renderer>();
        UpdateVisuals(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsTriggerEntity(other))
        {
            occupants.Add(other);
            activated = true;
            UpdateVisuals(true);
            Debug.Log("PRESSURE PLATE PRESSED!");
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
        if (!stayActivated)
        {
            occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            activated = occupants.Count > 0;
            UpdateVisuals(activated);
        }

        // Smoothly sink or rise plate
        Vector3 targetPos = initialLocalPos + (activated ? new Vector3(0, -0.06f, 0) : Vector3.zero);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 10f);
    }

    private void UpdateVisuals(bool isActive)
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = isActive ? activeColor : normalColor;
            if (plateRenderer.material.HasProperty("_EmissionColor"))
            {
                plateRenderer.material.EnableKeyword("_EMISSION");
                plateRenderer.material.SetColor("_EmissionColor", isActive ? activeColor * 2f : Color.black);
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
