using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothTime    = 0.12f;
    [SerializeField] private float lookAheadDist = 2f;
    [SerializeField] private float yOffset       = 1.5f;

    private Transform   target;
    private Rigidbody2D targetRb;
    private float       fixedZ = -10f;
    private float       currentLookAhead;
    private Vector3     velocity;
    private bool        ready;

    // Shake offset — set by CameraShake
    [HideInInspector] public Vector3 shakeOffset;

    void Awake()
    {
        Instance = this;
        fixedZ   = transform.position.z;
    }

    void Start()
    {
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
            cam.clearFlags      = CameraClearFlags.SolidColor;
        }

        if (target == null) AutoFind();
    }

    void AutoFind()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) SetTarget(p.transform);
    }

    public void SetTarget(Transform t)
    {
        target   = t;
        targetRb = t.GetComponent<Rigidbody2D>();
        transform.position = new Vector3(t.position.x, t.position.y + yOffset, fixedZ);
        ready = true;
    }

    void LateUpdate()
    {
        if (!ready) { AutoFind(); return; }
        if (target == null) { AutoFind(); return; }

        // Smooth look-ahead based on horizontal velocity
        float velX = targetRb != null ? targetRb.linearVelocity.x : 0f;
        float targetLook = Mathf.Sign(velX) * lookAheadDist * Mathf.Clamp01(Mathf.Abs(velX) / 5f);
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLook, 6f * Time.deltaTime);

        float desiredX = target.position.x + currentLookAhead;
        float desiredY = target.position.y + yOffset;

        Vector3 desired = new Vector3(desiredX, desiredY, fixedZ);
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref velocity, smoothTime) + shakeOffset;
    }
}
