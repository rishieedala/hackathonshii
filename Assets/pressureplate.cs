using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool activated = false;
    private readonly HashSet<Collider> occupants = new HashSet<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (IsTriggerEntity(other))
        {
            occupants.Add(other);
            activated = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsTriggerEntity(other))
        {
            occupants.Remove(other);
            activated = occupants.Count > 0;
        }
    }

    void Update()
    {
        // Clean up any references to destroyed or disabled occupants
        occupants.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        activated = occupants.Count > 0;
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