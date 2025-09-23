using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("UI e Tempo")]
    public float fadeTime = 1.0f;
    public Image fadePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayCounterText;

    [Header("Configurações de Tempo")]
    public float tempoTotalDia = 1200f;
    public float duracaoDiaPleno = 480f;
    public float duracaoEntardecer = 240f;
    public float duracaoNoite = 480f;

    private float tempoAtual;
    private int diaAtual = 1;
    private int horas = 6;
    private int minutos = 0;
    private bool podeAvancarTempo = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
        if (mainCanvas != null)
        {
            DontDestroyOnLoad(mainCanvas.gameObject);
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            DontDestroyOnLoad(mainCamera.gameObject);
        }

        ResetTime();
    }

    void Update()
    {
        if (podeAvancarTempo)
        {
            tempoAtual += Time.deltaTime;
        }
        
        float segundosPorHora = tempoTotalDia / 24f;
        float tempoDecorrenteDia = tempoAtual / segundosPorHora;
        
        horas = 6 + (int)tempoDecorrenteDia;
        minutos = (int)((tempoDecorrenteDia - (int)tempoDecorrenteDia) * 60);

        if (horas >= 26)
        {
            podeAvancarTempo = false;
            horas = 2;
            minutos = 0;
        }
        
        AtualizarUI();
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void AtualizarUI()
    {
        if (clockText != null)
        {
            clockText.text = string.Format("{0:00}:{1:00}", horas % 24, minutos);
        }
        if (dayCounterText != null)
        {
            dayCounterText.text = "Dia " + diaAtual.ToString();
        }
    }
    
    private void ResetTime()
    {
        tempoAtual = 0;
        horas = 6;
        minutos = 0;
        diaAtual = 1;
        podeAvancarTempo = true;
    }
}