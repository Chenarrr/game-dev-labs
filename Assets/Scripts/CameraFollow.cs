using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothTime    = 0.15f;
    [SerializeField] private float lookAheadDist = 1.5f;

    private Transform   target;
    private Rigidbody2D targetRb;
    private float       fixedZ = -10f;
    private float       minX;
    private float       currentLookAhead;
    private Vector3     velocity;
    private bool        ready;

    void Awake()
    {
        Instance = this;
        fixedZ   = transform.position.z;
    }

    void Start()
    {
        // Set sky blue background — works with URP 2D
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

        // Snap camera to player immediately — use player Y so nothing is off screen
        transform.position = new Vector3(t.position.x, t.position.y + 1.5f, fixedZ);
        minX  = t.position.x;
        ready = true;
    }

    void LateUpdate()
    {
        if (!ready) { AutoFind(); return; }
        if (target == null) { AutoFind(); return; }

        bool playing = GameManager.Instance != null && GameManager.Instance.IsPlaying;

        // Smooth look-ahead
        float velX = targetRb != null ? targetRb.linearVelocity.x : 0f;
        float targetLook = Mathf.Sign(velX) * lookAheadDist * Mathf.Clamp01(Mathf.Abs(velX) / 6f);
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLook, 4f * Time.deltaTime);

        float desiredX = target.position.x + currentLookAhead;
        float desiredY = target.position.y + 1.5f;  // keep player slightly below center

        if (playing)
        {
            if (desiredX > minX) minX = desiredX;
            desiredX = minX;
        }

        Vector3 desired = new Vector3(desiredX, desiredY, fixedZ);
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref velocity, smoothTime);
    }
}
