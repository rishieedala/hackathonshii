using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;
    public bool stayActivated = true;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.3f, 0.8f, 0.9f);
    public Color activeColor = new Color(0.2f, 1f, 0.3f);

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Vector3 initialLocalPos;
    private Renderer plateRenderer;
    private Material plateMaterial;
    private GameObject cachedPlayer;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        plateRenderer = GetComponent<Renderer>();
        if (plateRenderer != null)
        {
            plateMaterial = plateRenderer.material;
        }

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

            ExitDoor door = FindAnyObjectByType<ExitDoor>();
            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }

    public void ResetPlate()
    {
        activated = false;
        occupants.Clear();
        UpdateVisuals(false);
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
                CheckOccupancy();
            }
        }
    }

    void Update()
    {
        // Robust spatial check: detects Player, Ghost, Clone, or Corpse on top of the plate
        bool someoneOnPlate = CheckSpatialOverlap();

        if (someoneOnPlate)
        {
            TriggerPlate();
        }
        else if (!stayActivated)
        {
            occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            if (occupants.Count == 0)
            {
                if (activated)
                {
                    activated = false;
                    UpdateVisuals(false);
                }
            }
        }

        // Smoothly sink down when activated
        Vector3 targetPos = initialLocalPos + (activated ? new Vector3(0, -0.07f, 0) : Vector3.zero);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 12f);
    }

    private void CheckOccupancy()
    {
        occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        if (occupants.Count == 0 && !CheckSpatialOverlap())
        {
            activated = false;
            UpdateVisuals(false);
        }
    }

    private bool CheckSpatialOverlap()
    {
        Vector3 platePos = transform.position;

        // Check living player
        if (cachedPlayer == null)
        {
            cachedPlayer = GameObject.FindWithTag("Player");
        }

        if (cachedPlayer != null && IsWithinPlateBounds(cachedPlayer.transform.position, platePos))
        {
            return true;
        }

        // Check any active Ghost (Level 1)
        GhostReplay[] ghosts = FindObjectsByType<GhostReplay>(FindObjectsInactive.Exclude);
        foreach (var g in ghosts)
        {
            if (g != null && IsWithinPlateBounds(g.transform.position, platePos))
            {
                return true;
            }
        }

        // Check any active Clone (Level 2)
        CloneReplay[] clones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
        foreach (var c in clones)
        {
            if (c != null && IsWithinPlateBounds(c.transform.position, platePos))
            {
                return true;
            }
        }

        // Check any Corpse (Level 2)
        GameObject[] corpses = GameObject.FindGameObjectsWithTag("Corpse");
        foreach (var corpse in corpses)
        {
            if (corpse != null && IsWithinPlateBounds(corpse.transform.position, platePos))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsWithinPlateBounds(Vector3 entityPos, Vector3 platePos)
    {
        Vector3 diff = entityPos - platePos;
        // Check horizontal radius (within 1.3m) and vertical standing height (from slightly below plate surface to 2.2m above)
        return (Mathf.Abs(diff.x) < 1.3f && Mathf.Abs(diff.z) < 1.3f && diff.y >= -0.3f && diff.y < 2.2f);
    }

    private void UpdateVisuals(bool isActive)
    {
        if (plateMaterial != null)
        {
            plateMaterial.color = isActive ? activeColor : normalColor;
            if (plateMaterial.HasProperty("_EmissionColor"))
            {
                plateMaterial.EnableKeyword("_EMISSION");
                plateMaterial.SetColor("_EmissionColor", isActive ? activeColor * 3.5f : normalColor * 0.5f);
            }
        }
    }

    private bool IsTriggerEntity(Collider col)
    {
        if (col == null) return false;
        if (col.CompareTag("Player") || col.CompareTag("Ghost") || col.CompareTag("Clone") || col.CompareTag("Corpse")) return true;
        if (col.attachedRigidbody != null && (col.attachedRigidbody.CompareTag("Player") || col.attachedRigidbody.CompareTag("Ghost") || col.attachedRigidbody.CompareTag("Clone") || col.attachedRigidbody.CompareTag("Corpse"))) return true;
        if (col.transform.root != null && (col.transform.root.CompareTag("Player") || col.transform.root.CompareTag("Ghost") || col.transform.root.CompareTag("Clone") || col.transform.root.CompareTag("Corpse"))) return true;
        return false;
    }
}

