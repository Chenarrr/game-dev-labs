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
    // kept for backward compat with existing checks
    [HideInInspector] public bool isGameOver => State == GameState.GameOver;

    [SerializeField] private GameObject      gameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int score = 0;

    // intro overlay created at runtime
    private GameObject introOverlay;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        State = GameState.Intro;

        if (scoreText != null)
        {
            scoreText.text = "Score: 0";
            scoreText.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

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
        }
    }

    // Called by Obstacle when it exits the screen (player dodged it)
    public void AddScore()
    {
        if (State != GameState.Playing) return;
        score++;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        foreach (GameObject obs in GameObject.FindGameObjectsWithTag("Obstacle"))
            Destroy(obs);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
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

        // Semi-transparent dark background
        var img = introOverlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        var rt = introOverlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Title text
        var textGO = new GameObject("IntroText");
        textGO.transform.SetParent(introOverlay.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "DODGE OBSTACLES\n\n<size=36>Press <b>SPACE</b> or <b>ENTER</b> to Start</size>";
        tmp.fontSize = 64;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
    }
}
