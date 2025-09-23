using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-100)] // tenta garantir que o GameManager acorde antes de outros scripts
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI e Fade")]
    public float fadeTime = 1.0f;
    public Image fadePanel;
    public GameObject gameOverPanel;

    [Header("UI de HUD (opcional)")]
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayCounterText;

    private void Awake()
    {
        // Singleton robusto
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[GameManager] Outra instância detectada — destruindo esta.");
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Avisos se referências não atribuídas
        if (fadePanel == null) Debug.LogWarning("[GameManager] fadePanel não atribuído no Inspetor.");
        if (clockText == null) Debug.LogWarning("[GameManager] clockText não atribuído no Inspetor.");
        if (dayCounterText == null) Debug.LogWarning("[GameManager] dayCounterText não atribuído no Inspetor.");
    }

    // --- Game Over ---
    
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("[GameManager] gameOverPanel não atribuído!");
        }
    }
    //private void Update() // Ao Morrer Reinicia o jogo ao apertar E
    //{
        //if (gameOverPanel != null && gameOverPanel.activeSelf) // Verifica se o gameOverPanel está ativo
    //    {
    //    if (Input.GetKeyDown(KeyCode.E))
    //    {
    //        RestartGame();
    //    }
    //  }
    //}
    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Atualização de HUD opcional ---
    public void AtualizarUI(string horaTexto = null, string diaTexto = null)
    {
        if (clockText != null && horaTexto != null)
        {
            clockText.text = horaTexto;
        }

        if (dayCounterText != null && diaTexto != null)
        {
            dayCounterText.text = diaTexto;
        }
    }
}
