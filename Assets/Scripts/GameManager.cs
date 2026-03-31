using UnityEngine;
using UnityEngine.SceneManagement;
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
            if (Input.GetKeyDown(KeyCode.R))
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

        // Stop and clear all active obstacles
        foreach (GameObject obs in GameObject.FindGameObjectsWithTag("Obstacle"))
            Destroy(obs);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Called by editor setup script to wire UI references
    public void SetUIReferences(GameObject panel, TextMeshProUGUI scoreTMP, TextMeshProUGUI finalTMP)
    {
        gameOverPanel = panel;
        scoreText = scoreTMP;
        finalScoreText = finalTMP;
    }
}
