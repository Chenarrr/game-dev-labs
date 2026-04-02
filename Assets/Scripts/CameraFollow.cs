using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothTime   = 0.18f;   // lower = snappier
    [SerializeField] private float lookAheadX   = 1.8f;    // units ahead of player
    [SerializeField] private float verticalOffset = 0.8f;  // keep player slightly below center

    private Transform target;
    private float     fixedZ      = -10f;
    private float     minX;                                 // camera never scrolls left of start
    private Vector3   velocity    = Vector3.zero;
    private bool      initialized = false;

    void Awake()
    {
        Instance = this;
        fixedZ   = transform.position.z;
    }

    void Start()
    {
        FindAndSetTarget();
    }

    void FindAndSetTarget()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) SetTarget(player.transform);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        // Snap instantly to player on first set
        Vector3 snap = new Vector3(t.position.x, t.position.y + verticalOffset, fixedZ);
        transform.position = snap;
        minX        = snap.x;
        initialized = true;
    }

    void LateUpdate()
    {
        // Retry finding target if missing
        if (target == null) { FindAndSetTarget(); return; }
        if (!initialized)   { SetTarget(target);  return; }

        bool playing = GameManager.Instance != null && GameManager.Instance.IsPlaying;

        // Desired X: follow player + look-ahead in movement direction
        float velX      = target.GetComponent<Rigidbody2D>()?.linearVelocity.x ?? 0f;
        float ahead      = Mathf.Sign(velX) * lookAheadX * Mathf.Clamp01(Mathf.Abs(velX) / 4f);
        float desiredX   = target.position.x + ahead;
        float desiredY   = target.position.y + verticalOffset;

        // Mario rule: never scroll left during gameplay
        if (playing)
        {
            desiredX = Mathf.Max(desiredX, minX);
            if (desiredX > minX) minX = desiredX;
        }

        Vector3 desired  = new Vector3(desiredX, desiredY, fixedZ);
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref velocity, smoothTime);
    }
}
