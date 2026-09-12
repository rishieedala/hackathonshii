using UnityEngine;

/// <summary>
/// A door that stays OPEN only while its pressure plate is activated (pressed by player or ghost).
/// When the plate is released, the door closes again immediately.
/// This forces the player to use a clone to hold the plate while they run through.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TimedDoor : MonoBehaviour
{
    [Tooltip("The pressure plate that controls this door.")]
    public PressurePlate doorPlate;

    private new Renderer renderer;
    private Collider col;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        col      = GetComponent<Collider>();
    }

    void Update()
    {
        if (doorPlate == null) return;

        bool open = doorPlate.activated;
        renderer.enabled = !open;  // hide door when open
        col.enabled      = !open;  // passable when open
    }
}
