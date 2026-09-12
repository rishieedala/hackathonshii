using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            activated = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            activated = false;
        }
    }
}