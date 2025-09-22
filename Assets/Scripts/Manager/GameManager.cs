using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("UI e Transições")]
    public float fadeTime = 1.0f;
    public Image fadePanel;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayCounterText;

    [Header("Configurações de Tempo")]
    public float tempoTotalDia = 1200f;
    public float duracaoDiaPleno = 480f;
    public float duracaoEntardecer = 240f;
    public float duracaoNoite = 480f;
    
    [Header("Efeitos Visuais")]
    public Light2D direcionalLuz;
    public float intensidadeDia = 1f;
    public float intensidadeEntardecer = 0.5f;
    public float intensidadeNoite = 0.3f;
    public Color corDia = Color.white;
    public Color corEntardecer = new Color(1f, 0.7f, 0.4f);
    public Color corNoite = new Color(0.1f, 0.1f, 0.3f);

    private float tempoAtual;
    private int diaAtual = 1;
    private int horas = 6;
    private int minutos = 0;
    private bool podeAvancarTempo = true;
    private string _previousSceneName;

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

        if (horas >= 6 && horas < 14)
        {
            direcionalLuz.intensity = intensidadeDia;
            direcionalLuz.color = corDia;
        }
        else if (horas >= 14 && horas < 18)
        {
            float t = (horas - 14 + (minutos / 60f)) / (18 - 14);
            direcionalLuz.intensity = Mathf.Lerp(intensidadeDia, intensidadeEntardecer, t);
            direcionalLuz.color = Color.Lerp(corDia, corEntardecer, t);
        }
        else if (horas >= 18 || horas < 2)
        {
            float horasNoite = (horas >= 18) ? horas - 18 : horas + 6;
            float t = (horasNoite + (minutos / 60f)) / (26 - 18);
            direcionalLuz.intensity = Mathf.Lerp(intensidadeEntardecer, intensidadeNoite, t);
            direcionalLuz.color = Color.Lerp(corEntardecer, corNoite, t);
        }

        AtualizarUI();
    }

    // --- Métodos de Transição de Cena ---
    public void LoadScene(string sceneName)
    {
        _previousSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(FadeIn(sceneName));
    }
    
    public void LoadPreviousScene()
    {
        if (!string.IsNullOrEmpty(_previousSceneName))
        {
            StartCoroutine(FadeIn(_previousSceneName));
        }
    }

    private IEnumerator FadeIn(string sceneName)
    {
        fadePanel.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadePanel.color;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeTime);
            fadePanel.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadePanel.color;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeTime));
            fadePanel.color = color;
            yield return null;
        }
        
        fadePanel.gameObject.SetActive(false);
    }
    
    // --- Métodos de UI e Jogo ---
    public void ShowGameOverPanel()
    {
        // NOVO: Chama o UIManager para mostrar o painel
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowGameOverPanel();
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
        clockText.text = string.Format("{0:00}:{1:00}", horas % 24, minutos);
        dayCounterText.text = "Dia " + diaAtual.ToString();
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