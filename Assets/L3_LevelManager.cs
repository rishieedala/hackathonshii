using UnityEngine;

/// <summary>
/// Orchestrates all Level 3 puzzle logic.
///
/// Puzzle flow summary:
/// LOOP 1  — Player walks 12m → invisible trigger fires → Ghost1 spawns, player resets.
/// LOOP 2  — Ghost1 walks to 12m trigger → OnClonePassedResetZone() fires:
///              • LaserWall1 becomes visible + solid.
///              • Plate1 (pre-placed, inactive) is activated (player steps on it → Laser1 permanently off).
///           Player crosses Laser1 zone → 5-sec hidden timer → Plate2 appears.
///           Player steps on Plate2 → Laser2 permanently off.
///           Player steps on DoorPlate → door opens WHILE standing on it. Can't run through + hold.
///           Loop ends → Ghost2 spawns.
/// LOOP 3  — Ghost2 (replicates loop 2) stands on DoorPlate → door stays open.
///           Player, already past all obstacles, runs through → WIN.
/// </summary>
public class L3_LevelManager : MonoBehaviour
{
    [Header("Laser Walls")]
    public LaserWall laser1;   // invisible at start; activated when clone passes reset zone
    public LaserWall laser2;   // visible red laser at start; disabled by Plate2

    [Header("Plates (pre-placed in scene, start INACTIVE)")]
    public GameObject plate1Object;  // activated when clone passes reset zone
    public PressurePlate plate1;     // reference for laser1 to watch
    public GameObject plate2Object;  // spawned after 5-sec timer
    public PressurePlate plate2;     // reference for laser2 to watch

    [Header("Hidden Timer")]
    public float plate2SpawnDelay = 5f;
    private float plate2Timer = -1f;
    private bool plate2Revealed = false;

    // State flags (idempotent)
    private bool laser1Activated = false;
    private bool timerStarted    = false;

    void Update()
    {
        // Tick the hidden 5-second timer for Plate2
        if (timerStarted && !plate2Revealed && plate2Timer > 0f)
        {
            plate2Timer -= Time.deltaTime;
            if (plate2Timer <= 0f)
            {
                RevealPlate2();
            }
        }

        // Wire plate1 → laser1 and plate2 → laser2 (LaserWall.Update also does this,
        // but we keep references consistent after objects activate)
        if (laser1 != null && plate1 != null)
            laser1.controlledByPlate = plate1;

        if (laser2 != null && plate2 != null)
            laser2.controlledByPlate = plate2;
    }

    /// <summary>
    /// Called by LoopResetTrigger every time a ghost passes through the 12m zone.
    /// Idempotent — only activates Laser1 + Plate1 once.
    /// </summary>
    public void OnClonePassedResetZone()
    {
        if (laser1Activated) return;
        laser1Activated = true;

        Debug.Log("[L3] Clone passed reset zone — activating Laser1 and Plate1");

        if (laser1 != null)
            laser1.Activate();

        if (plate1Object != null)
            plate1Object.SetActive(true);
    }

    /// <summary>
    /// Called by L3_CrossZone when the player walks through the Laser1 position.
    /// Starts the hidden 5-second countdown.
    /// </summary>
    public void OnPlayerCrossedLaser1Zone()
    {
        if (timerStarted) return;
        timerStarted = true;
        plate2Timer  = plate2SpawnDelay;
        Debug.Log("[L3] Player crossed Laser1 zone — 5-second hidden timer started");
    }

    private void RevealPlate2()
    {
        plate2Revealed = true;
        Debug.Log("[L3] Hidden timer expired — Plate2 revealed");

        if (plate2Object != null)
            plate2Object.SetActive(true);
    }
}
