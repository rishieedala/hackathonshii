using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public GameObject corpsePrefab;

    private bool isDead = false;
    public bool IsDead => isDead;

    private CharacterController controller;
    private PlayerController playerController;
    private Renderer[] playerRenderers;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        playerRenderers = GetComponentsInChildren<Renderer>();
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Remember where the player died
        Vector3 deathPosition = transform.position;
        Quaternion deathRotation = transform.rotation;

        // Create the physical corpse
        if (corpsePrefab != null)
        {
            GameObject corpse = Instantiate(corpsePrefab, deathPosition, deathRotation);
            SetLayerRecursively(corpse, 0); // Ensure corpse is on Default layer (0) so it's fully visible to camera
        }

        // Stop player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Stop CharacterController
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Hide the living player's body (keeps Camera active!)
        foreach (Renderer r in playerRenderers)
        {
            if (r != null)
            {
                r.enabled = false;
            }
        }

        Debug.Log("PLAYER DIED - CORPSE LEFT BEHIND");

        // Notify LoopManager to schedule loop restart
        LoopManager loopManager = FindAnyObjectByType<LoopManager>();
        if (loopManager != null)
        {
            loopManager.OnPlayerDeath();
        }
    }

    public void ResetPlayer(Vector3 resetPosition, Quaternion resetRotation)
    {
        isDead = false;

        // Temporarily disable CharacterController while updating position to prevent transform clamping
        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = resetPosition;
        transform.rotation = resetRotation;
        Physics.SyncTransforms();

        if (controller != null)
        {
            controller.enabled = true;
        }

        if (playerController != null)
        {
            playerController.ResetVelocity();
            playerController.enabled = true;
        }

        // Re-enable player model renderers
        foreach (Renderer r in playerRenderers)
        {
            if (r != null)
            {
                r.enabled = true;
            }
        }

        Debug.Log("PLAYER REVIVED AT START POINT");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
