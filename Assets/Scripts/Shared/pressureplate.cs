using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.3f, 0.8f, 0.9f);
    public Color activeColor  = new Color(0.2f, 1f,   0.3f);

    private Vector3      initialLocalPos;
    private Renderer     plateRenderer;
    private Material     plateMaterial;
    private GameObject   cachedPlayer;
    private GhostReplay[] cachedGhosts = new GhostReplay[0];
    private CloneReplay[] cachedClones = new CloneReplay[0];

    // Cached once to avoid per-frame UnityException when "Corpse" tag is not defined in Tag Manager
    private static bool? corpseTagExists = null;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        plateRenderer = GetComponent<Renderer>();
        if (plateRenderer != null) plateMaterial = plateRenderer.material;
        cachedPlayer = GameObject.FindWithTag("Player");
        RefreshEntityCaches();
        UpdateVisuals(false);
    }

    void Update()
    {
        bool someoneOnPlate = CheckSpatialOverlap();
        if (someoneOnPlate != activated)
        {
            activated = someoneOnPlate;
            UpdateVisuals(activated);
            Debug.Log(activated ? "PressurePlate: ACTIVATED" : "PressurePlate: DEACTIVATED");
        }
        Vector3 targetLocalPos = initialLocalPos + (activated ? new Vector3(0, -0.07f, 0) : Vector3.zero);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * 12f);
    }

    public void ResetPlate()
    {
        activated = false;
        UpdateVisuals(false);
        transform.localPosition = initialLocalPos;
        RefreshEntityCaches();
        Debug.Log("PressurePlate: Reset");
    }

    public void RefreshEntityCaches()
    {
        if (cachedPlayer == null) cachedPlayer = GameObject.FindWithTag("Player");
        cachedGhosts = FindObjectsByType<GhostReplay>(FindObjectsInactive.Exclude);
        cachedClones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
    }

    private float nextCacheRefreshTime = 0f;

    private bool CheckSpatialOverlap()
    {
        Vector3 platePos = transform.position;
        if (cachedPlayer == null) cachedPlayer = GameObject.FindWithTag("Player");
        if (cachedPlayer != null && IsWithinPlateBounds(cachedPlayer.transform.position, platePos)) return true;

        if (Time.time >= nextCacheRefreshTime || cachedClones == null || cachedClones.Length == 0)
        {
            nextCacheRefreshTime = Time.time + 0.2f;
            cachedGhosts = FindObjectsByType<GhostReplay>(FindObjectsInactive.Exclude);
            cachedClones = FindObjectsByType<CloneReplay>(FindObjectsInactive.Exclude);
        }

        if (cachedGhosts != null)
        {
            for (int i = 0; i < cachedGhosts.Length; i++)
            {
                var g = cachedGhosts[i];
                if (g != null && IsWithinPlateBounds(g.transform.position, platePos)) return true;
            }
        }

        if (cachedClones != null)
        {
            for (int i = 0; i < cachedClones.Length; i++)
            {
                var c = cachedClones[i];
                if (c != null && IsWithinPlateBounds(c.transform.position, platePos)) return true;
            }
        }

        // Corpses (Level 2) — guard against "Corpse" tag not existing in Tag Manager
        if (corpseTagExists != false)
        {
            try
            {
                GameObject[] corpses = GameObject.FindGameObjectsWithTag("Corpse");
                corpseTagExists = true;
                foreach (var corpse in corpses)
                    if (corpse != null && IsWithinPlateBounds(corpse.transform.position, platePos)) return true;
            }
            catch (UnityException)
            {
                // Tag not defined in this project — disable corpse checks permanently
                corpseTagExists = false;
            }
        }
        return false;
    }

    private bool IsWithinPlateBounds(Vector3 entityPos, Vector3 platePos)
    {
        Vector3 diff = entityPos - platePos;
        return (Mathf.Abs(diff.x) < 1.6f && Mathf.Abs(diff.z) < 1.6f && diff.y >= -0.5f && diff.y < 2.5f);
    }

    private void UpdateVisuals(bool isActive)
    {
        if (plateMaterial == null) return;
        plateMaterial.color = isActive ? activeColor : normalColor;
        if (plateMaterial.HasProperty("_EmissionColor"))
        {
            plateMaterial.EnableKeyword("_EMISSION");
            plateMaterial.SetColor("_EmissionColor", isActive ? activeColor * 3.5f : normalColor * 0.5f);
        }
    }
}
