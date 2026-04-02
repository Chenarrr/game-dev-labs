using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originPos;
    private float   shakeDuration;
    private float   shakeMagnitude;

    void Awake()
    {
        Instance  = this;
        originPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration  = duration;
        shakeMagnitude = magnitude;
    }

    void Update()
    {
        if (shakeDuration > 0f)
        {
            transform.localPosition = originPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originPos;
        }
    }
}
