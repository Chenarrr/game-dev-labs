using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothSpeed = 8f;

    private Transform target;
    private float     fixedY;
    private float     fixedZ;
    private float     minX;       // camera never scrolls left of this

    void Awake()
    {
        Instance = this;
        fixedY   = transform.position.y;
        fixedZ   = transform.position.z;
        minX     = transform.position.x;
    }

    void Start()
    {
        // Always find and follow the player automatically
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) SetTarget(player.transform);
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        // Snap camera directly onto player immediately so there's no lag at start
        float startX = t.position.x;
        transform.position = new Vector3(startX, fixedY, fixedZ);
        minX = startX;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float desiredX = target.position.x;

        if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            // Mario rule: only scroll right during gameplay
            if (desiredX > minX) minX = desiredX;
            float newX = Mathf.Lerp(transform.position.x, minX, smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, fixedY, fixedZ);
        }
        else
        {
            // Always follow player freely before game starts
            float newX = Mathf.Lerp(transform.position.x, desiredX, smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, fixedY, fixedZ);
        }
    }
}
