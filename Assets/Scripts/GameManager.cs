using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int score = 0;
    private float scoreTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (isGameOver)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        scoreTimer += Time.deltaTime;
        if (scoreTimer >= 1f)
        {
            score++;
            scoreTimer -= 1f;
            if (scoreText != null)
                scoreText.text = "Score: " + score;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

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
}
