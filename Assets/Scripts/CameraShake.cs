using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private float shakeDuration;
    private float shakeMagnitude;

    void Awake() => Instance = this;

    public void Shake(float duration, float magnitude)
    {
        shakeDuration  = duration;
        shakeMagnitude = magnitude;
    }

    void Update()
    {
        if (CameraFollow.Instance == null) return;

        if (shakeDuration > 0f)
        {
            CameraFollow.Instance.shakeOffset = (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            CameraFollow.Instance.shakeOffset = Vector3.zero;
        }
    }
}
