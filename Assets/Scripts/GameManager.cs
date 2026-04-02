using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Intro, Playing, GameOver }
    public GameState State { get; private set; } = GameState.Intro;

    public bool IsPlaying  => State == GameState.Playing;
    public bool isGameOver => State == GameState.GameOver;

    [SerializeField] private GameObject      gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int   bestScore;
    private float startX;
    private float lastScoreX;
    private int   score;
    private Transform playerTransform;

    private GameObject      introOverlay;
    private TextMeshProUGUI introText;

    private const float UnitsPerPoint = 4f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance  = this;
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    void Start()
    {
        State = GameState.Intro;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Bootstrap systems
        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
            Camera.main.gameObject.AddComponent<CameraShake>();

        if (Camera.main != null && Camera.main.GetComponent<CameraFollow>() == null)
            Camera.main.gameObject.AddComponent<CameraFollow>();

        if (FindFirstObjectByType<CloudSpawner>() == null)
            new GameObject("CloudSpawner").AddComponent<CloudSpawner>();

        if (FindFirstObjectByType<WorldGenerator>() == null)
            new GameObject("WorldGenerator").AddComponent<WorldGenerator>();

        if (FindFirstObjectByType<ParallaxBackground>() == null)
            new GameObject("ParallaxBackground").AddComponent<ParallaxBackground>();

        // Disable old obstacle spawner
        var obs = FindFirstObjectByType<ObstacleSpawner>();
        if (obs != null) obs.enabled = false;

        // Disable the limited green background sprite
        var bgObj = GameObject.Find("ground");
        if (bgObj != null) bgObj.SetActive(false);

        // Also try "Ground" name variant
        var bgObj2 = GameObject.Find("Ground");
        if (bgObj2 != null && bgObj2.GetComponent<SpriteRenderer>() != null
            && bgObj2.GetComponent<BoxCollider2D>() == null)
            bgObj2.SetActive(false);

        // Kill any leftover score text UI from old scene setup
        foreach (var tmp in FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (tmp.text.StartsWith("Score"))
            {
                tmp.gameObject.SetActive(false);
            }
        }

        // Fix starting Ground: shrink to 8 units wide, set brown
        var ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null)
        {
            var sr = ground.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.color = new Color(0.45f, 0.27f, 0.13f); sr.sortingOrder = 2; }
            ground.transform.localScale = new Vector3(8f, ground.transform.localScale.y, 1f);
            ground.transform.position   = new Vector3(0f, ground.transform.position.y, 0f);
        }

        // Link camera to player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            CameraFollow.Instance?.SetTarget(playerTransform);
        }

        CreateIntroOverlay();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (State == GameState.Intro)
        {
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                StartGame();
            return;
        }

        if (State == GameState.Playing)
        {
            // Track score internally for best-score
            if (playerTransform != null)
            {
                float travelX = playerTransform.position.x - lastScoreX;
                if (travelX >= UnitsPerPoint)
                {
                    int pts = Mathf.FloorToInt(travelX / UnitsPerPoint);
                    lastScoreX += pts * UnitsPerPoint;
                    score += pts;
                }
            }
        }

        if (State == GameState.GameOver)
        {
            if (kb.rKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void StartGame()
    {
        State = GameState.Playing;
        score = 0;

        if (playerTransform != null)
        {
            startX     = playerTransform.position.x;
            lastScoreX = startX;
        }

        if (introOverlay != null)
            introOverlay.SetActive(false);
    }

    public void AddStomp()
    {
        if (State != GameState.Playing) return;
        score += 3;
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        if (finalScoreText != null)
            finalScoreText.text = "Distance: " + score
                + (score >= bestScore && score > 0 ? "  <size=28><color=#FFD700>NEW BEST!</color></size>" : "");

        var hint = gameOverPanel != null ? gameOverPanel.transform.Find("RestartHint") : null;
        if (hint != null)
        {
            var h = hint.GetComponent<TextMeshProUGUI>();
            if (h != null)
                h.text = "Best: " + bestScore + "\n\n<size=28>Press <b>R</b> or <b>ENTER</b> to Restart</size>";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        CameraShake.Instance?.Shake(0.4f, 0.18f);
    }

    public void SetUIReferences(GameObject panel, TextMeshProUGUI scoreTMP, TextMeshProUGUI finalTMP)
    {
        gameOverPanel  = panel;
        finalScoreText = finalTMP;
    }

    void CreateIntroOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        introOverlay = new GameObject("IntroPanel");
        introOverlay.transform.SetParent(canvas.transform, false);

        var img = introOverlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.45f);
        var rt = introOverlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var textGO = new GameObject("IntroText");
        textGO.transform.SetParent(introOverlay.transform, false);
        introText = textGO.AddComponent<TextMeshProUGUI>();

        string best = bestScore > 0 ? $"\n<size=28><color=#FFD700>Best: {bestScore}</color></size>" : "";
        introText.text = "<b>SUPER RUNNER</b>" + best
            + "\n\n<size=32>\u2190 \u2192 or A D  to run\n\u2191 or SPACE to jump\n\nStomp enemies!</size>"
            + "\n\n<size=36>Press <b>SPACE</b> to Start</size>";
        introText.fontSize  = 54;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color     = Color.white;

        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
    }
}
