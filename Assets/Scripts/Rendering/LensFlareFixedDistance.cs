using UnityEngine;

public class LensFlareFixedDistance : MonoBehaviour
{
    private float size;
    public LensFlare flare;
    void Start()
    {
        if (flare == null)
            flare = GetComponent<LensFlare>();


        size = flare.brightness;
    }

    void Update()
    {
        float ratio = Mathf.Sqrt(Vector3.Distance(transform.position, Camera.main.transform.position));
        flare.brightness = size / ratio;
    }
}