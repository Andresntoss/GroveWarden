using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI de Game Over")]
    public GameObject gameOverPanel;

    void Update()
    {
        // Se o jogo estiver pausado e o jogador pressionar 'E'
        if (Time.timeScale == 0f && Input.GetKeyDown(KeyCode.E))
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
    }
}