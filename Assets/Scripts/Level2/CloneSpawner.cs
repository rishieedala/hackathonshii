using System.Collections.Generic;
using UnityEngine;

public class CloneSpawner : MonoBehaviour
{
    public GameObject clonePrefab;
    public ReplayRecorder recorder;
    public GameObject corpsePrefab;

    public void SpawnClone(List<ReplayRecorder.Frame> recordedFrames = null)
    {
        if (clonePrefab == null)
        {
            Debug.LogError("CloneSpawner: clonePrefab reference missing!");
            return;
        }

        List<ReplayRecorder.Frame> framesToUse = recordedFrames;
        if (framesToUse == null && recorder != null)
        {
            framesToUse = recorder.GetFramesCopy();
        }

        if (framesToUse == null || framesToUse.Count == 0)
        {
            Debug.LogWarning("CloneSpawner: No frames recorded to spawn clone with.");
            return;
        }

        Vector3 spawnPos = framesToUse[0].position;
        Quaternion spawnRot = framesToUse[0].rotation;

        GameObject clone = Instantiate(clonePrefab, spawnPos, spawnRot);
        CloneReplay replay = clone.GetComponent<CloneReplay>();
        if (replay == null)
        {
            replay = clone.AddComponent<CloneReplay>();
        }

        if (corpsePrefab != null && replay.corpsePrefab == null)
        {
            replay.corpsePrefab = corpsePrefab;
        }

        replay.SetRecording(framesToUse);
        replay.StartReplay();

        Debug.Log("CLONE SPAWNED WITH " + framesToUse.Count + " FRAMES");
    }
}
