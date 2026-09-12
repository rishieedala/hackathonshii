using UnityEngine;

public class LaserVisual : MonoBehaviour
{
    [Header("Laser Glow & Lighting")]
    public Color laserColor = new Color(1f, 0.05f, 0.05f, 1f);
    public float baseIntensity = 2.5f;
    public float pulseSpeed = 4f;

    private Light laserLight;
    private Material laserMat;

    void Start()
    {
        // Add a pulsing point light to cast real-time red light on surroundings
        laserLight = GetComponent<Light>();
        if (laserLight == null)
        {
            laserLight = gameObject.AddComponent<Light>();
        }

        laserLight.type = LightType.Point;
        laserLight.color = laserColor;
        laserLight.range = 10f;
        laserLight.intensity = baseIntensity;

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            laserMat = r.material;
            laserMat.EnableKeyword("_EMISSION");
            laserMat.SetColor("_EmissionColor", laserColor * 4f);
        }
    }

    void Update()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.4f + 1f;

        if (laserLight != null)
        {
            laserLight.intensity = baseIntensity * pulse;
        }

        if (laserMat != null)
        {
            laserMat.SetColor("_EmissionColor", laserColor * (3.5f * pulse));
        }
    }
}
