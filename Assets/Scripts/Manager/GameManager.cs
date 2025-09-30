using System.Collections;
using System.Collections.Generic; // NOVO: Adicionado para PlotManager list
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// NOTA: Removido using Unity.Cinemachine para evitar erros de compilação
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
    // public Light2D direcionalLuz; // REMOVIDO: Se não estiver usando URP/pacote

    private float tempoAtual;
    private int diaAtual = 1;
    private int horas = 6;
    private int minutos = 0;
    private bool podeAvancarTempo = true;
    
    // NOVO: Lista para rastrear os módulos de plantio (agora no GameManager)
    private List<PlotManager> activePlotManagers = new List<PlotManager>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
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
    
    // --- LÓGICA DE DIA E FAZENDA ---
    
    // NOVO: Método chamado pelo BedInteraction para avançar o dia
    public void PassarParaProximoDia()
    {
        NotifyPlotsToAdvance(); // 1. Faz as plantas crescerem
        
        diaAtual++;              // 2. Incrementa o dia
        ResetTime();             // 3. Reseta o relógio
    }
    
    // NOVO: Método que notifica todos os canteiros (chamado por PassarParaProximoDia)
    public void NotifyPlotsToAdvance()
    {
        foreach (PlotManager plot in activePlotManagers)
        {
            plot.PassDay();
        }
        Debug.Log("GameManager: Notificou todos os PlotManagers para avançar o dia.");
    }
    
    // NOVO: Método para os PlotManagers se registrarem
    public void RegisterPlotManager(PlotManager plotManager)
    {
        if (!activePlotManagers.Contains(plotManager))
        {
            activePlotManagers.Add(plotManager);
            Debug.Log($"PlotManager registrado. Total de módulos ativos: {activePlotManagers.Count}");
        }
    }

    // --- MÉTODOS DE UI E JOGO ---
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
        podeAvancarTempo = true;
    }
}