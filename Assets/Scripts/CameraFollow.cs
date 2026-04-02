using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    private Transform target;
    private Vector3   origin;

    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private float aheadOffset = 3f;   // look a bit ahead of the player

    void Awake()
    {
        Instance = this;
        origin   = transform.position;
    }

    public void SetTarget(Transform t) => target = t;

    void LateUpdate()
    {
        if (target == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        float desiredX  = Mathf.Max(origin.x, target.position.x + aheadOffset);
        Vector3 desired = new Vector3(desiredX, origin.y, origin.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
