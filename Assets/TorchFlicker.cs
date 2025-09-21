using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    Light torchLight;
    public float minIntensity = 1.5f;
    public float maxIntensity = 2.5f;
    public float flickerSpeed = 0.1f;

    void Start()
    {
        torchLight = GetComponent<Light>();
    }

    void Update()
    {
        torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * flickerSpeed, 0));
    }
}