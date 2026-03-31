using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public bool isGameOver = false;

    [SerializeField] private GameObject gameOverPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void SetGameOverPanel(GameObject panel)
    {
        gameOverPanel = panel;
    }
}
