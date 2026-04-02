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
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int   score    = 0;
    private int   bestScore;

    private GameObject     introOverlay;
    private TextMeshProUGUI introText;

    // score pop animation
    private float  scorePop      = 0f;
    private Vector3 scoreBaseScale;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    void Start()
    {
        State = GameState.Intro;

        if (scoreText != null)
        {
            scoreBaseScale = scoreText.transform.localScale;
            scoreText.text = "Score: 0";
            scoreText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Attach CameraShake to the main camera if not already present
        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
            Camera.main.gameObject.AddComponent<CameraShake>();

        // Force ground platform to brown so it's always visible over the green background
        var ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null)
        {
            var sr = ground.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color        = new Color(0.45f, 0.27f, 0.13f);
                sr.sortingOrder = 2;
            }
        }

        // Bootstrap cloud spawner
        if (FindFirstObjectByType<CloudSpawner>() == null)
        {
            var go = new GameObject("CloudSpawner");
            go.AddComponent<CloudSpawner>();
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

        if (State == GameState.GameOver)
        {
            if (kb.rKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Score pop-down animation
        if (scorePop > 0f && scoreText != null)
        {
            scorePop = Mathf.MoveTowards(scorePop, 0f, Time.deltaTime * 8f);
            scoreText.transform.localScale = scoreBaseScale * (1f + scorePop * 0.3f);
        }
    }

    void StartGame()
    {
        State = GameState.Playing;
        score = 0;

        if (introOverlay != null)
            introOverlay.SetActive(false);

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = "Score: 0";
            scoreText.color = Color.white;
        }
    }

    public void AddScore()
    {
        if (State != GameState.Playing) return;
        score++;

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
            // colour shifts as score climbs
            scoreText.color = score < 10 ? Color.white
                            : score < 20 ? new Color(1f, 0.95f, 0.4f)   // yellow
                            : score < 35 ? new Color(1f, 0.6f, 0.2f)    // orange
                                         : new Color(1f, 0.35f, 0.35f); // red
            scorePop = 1f;
        }
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        // Save best score
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        foreach (GameObject obs in GameObject.FindGameObjectsWithTag("Obstacle"))
            Destroy(obs);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score + (score >= bestScore ? "  <size=28><color=#FFD700>NEW BEST!</color></size>" : "");

        // Show best score in the restart hint
        var hint = gameOverPanel != null
            ? gameOverPanel.transform.Find("RestartHint")
            : null;
        if (hint != null)
        {
            var hintTMP = hint.GetComponent<TextMeshProUGUI>();
            if (hintTMP != null)
                hintTMP.text = "Best: " + bestScore + "\n\n<size=28>Press <b>R</b> or <b>ENTER</b> to Restart</size>";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Camera shake
        CameraShake.Instance?.Shake(0.4f, 0.18f);
    }

    public void SetUIReferences(GameObject panel, TextMeshProUGUI scoreTMP, TextMeshProUGUI finalTMP)
    {
        gameOverPanel  = panel;
        scoreText      = scoreTMP;
        finalScoreText = finalTMP;
    }

    // ── Intro overlay ────────────────────────────────────────────────────────
    void CreateIntroOverlay()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        introOverlay = new GameObject("IntroPanel");
        introOverlay.transform.SetParent(canvas.transform, false);

        var img = introOverlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.55f);
        var rt = introOverlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var textGO = new GameObject("IntroText");
        textGO.transform.SetParent(introOverlay.transform, false);
        introText = textGO.AddComponent<TextMeshProUGUI>();

        string best = bestScore > 0 ? $"\n<size=28><color=#FFD700>Best: {bestScore}</color></size>" : "";
        introText.text = "<b>DODGE OBSTACLES</b>" + best +
                         "\n\n<size=32>← → or A D  to move\n↑ or SPACE to jump</size>" +
                         "\n\n<size=36>Press <b>SPACE</b> to Start</size>";
        introText.fontSize = 58;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color = Color.white;
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
    }
}
