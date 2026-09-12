using UnityEngine;

public class LaserDeath : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if player touched the laser
        PlayerDeath playerDeath = other.GetComponent<PlayerDeath>();
        if (playerDeath == null)
        {
            playerDeath = other.GetComponentInParent<PlayerDeath>();
        }

        if (playerDeath != null)
        {
            playerDeath.Die();
            return;
        }

        // Check if clone touched the laser
        CloneReplay clone = other.GetComponent<CloneReplay>();
        if (clone == null)
        {
            clone = other.GetComponentInParent<CloneReplay>();
        }

        if (clone != null)
        {
            clone.Die();
            return;
        }
    }
}
