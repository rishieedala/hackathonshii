using UnityEngine;

[System.Serializable]
public class PlayerFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    public PlayerFrame(float time, Vector3 position, Quaternion rotation)
    {
        this.time = time;
        this.position = position;
        this.rotation = rotation;
    }
}