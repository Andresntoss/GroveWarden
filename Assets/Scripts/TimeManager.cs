using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    // ----- Variáveis Públicas (para ajustar no Inspector) -----
    [Header("Configurações de Tempo")]
    public float tempoTotalDia = 1200f; // 20 minutos em segundos (20 * 60)
    public float duracaoDiaPleno = 480f; // 8 horas (das 6h às 14h) em segundos
    public float duracaoEntardecer = 240f; // 4 horas (das 14h às 18h) em segundos
    public float duracaoNoite = 480f; // 8 horas (das 18h às 2h) em segundos
    
    [Header("Elementos de UI")]
    public TextMeshProUGUI relogioTexto;
    public TextMeshProUGUI contadorDiaTexto;
    
    [Header("Efeitos Visuais")]
    public Light2D direcionalLuz; // A luz direcional na sua cena (sol/lua)
    public float intensidadeDia = 1f;
    public float intensidadeEntardecer = 0.5f; // Nova intensidade para o entardecer
    public float intensidadeNoite = 0.3f;
    public Color corDia = Color.white;
    public Color corEntardecer = new Color(1f, 0.7f, 0.4f); // Uma cor alaranjada
    public Color corNoite = new Color(0.1f, 0.1f, 0.3f); // Um azul escuro
    
    // ----- Variáveis Privadas (para o sistema) -----
    private float tempoAtual;
    private int diaAtual = 1;

    // ----- Relógio e Horário do Jogo -----
    private int horas = 6;
    private int minutos = 0;
    
    // Flag para verificar se o tempo deve continuar correndo
    private bool podeAvancarTempo = true;
    
    void Start()
    {
        // Define o tempo inicial do jogo para 6 da manhã
        tempoAtual = 0;
        horas = 6;
        minutos = 0;
        
        // Garante que a UI e a luz estão com os valores iniciais corretos
        AtualizarUI();
        AtualizarLuz();
    }
    
    void Update()
    {
        if (podeAvancarTempo)
        {
            tempoAtual += Time.deltaTime;
        }

        // Converte o tempo do jogo para horas e minutos
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
        
        // --- Nova Lógica de Transição ---
        if (horas >= 6 && horas < 14) // Dia Pleno (6h - 14h)
        {
            direcionalLuz.intensity = intensidadeDia;
            direcionalLuz.color = corDia;
        }
        else if (horas >= 14 && horas < 18) // Entardecer (14h - 18h)
        {
            float t = (horas - 14 + (minutos / 60f)) / (18 - 14); // Normaliza o tempo de 0 a 1
            direcionalLuz.intensity = Mathf.Lerp(intensidadeDia, intensidadeEntardecer, t);
            direcionalLuz.color = Color.Lerp(corDia, corEntardecer, t);
        }
        else if (horas >= 18 || horas < 2) // Noite (18h - 2h do próximo dia)
        {
            float horasNoite = (horas >= 18) ? horas - 18 : horas + 6; // Calcula horas relativas à noite
            float t = (horasNoite + (minutos / 60f)) / (26 - 18); // Normaliza o tempo de 0 a 1
            direcionalLuz.intensity = Mathf.Lerp(intensidadeEntardecer, intensidadeNoite, t);
            direcionalLuz.color = Color.Lerp(corEntardecer, corNoite, t);
        }
        
        AtualizarUI();
    }
    
    // As funções AtualizarUI, AtualizarLuz e PassarParaProximoDia não precisam de alteração
    private void AtualizarUI()
    {
        relogioTexto.text = string.Format("{0:00}:{1:00}", horas % 24, minutos);
        contadorDiaTexto.text = "Dia " + diaAtual.ToString();
    }
    
    private void AtualizarLuz()
    {
        if (direcionalLuz != null)
        {
            direcionalLuz.intensity = intensidadeDia;
            direcionalLuz.color = corDia;
        }
    }
    
    public void PassarParaProximoDia()
    {
        diaAtual++;
        tempoAtual = 0;
        podeAvancarTempo = true;
        horas = 6;
        minutos = 0;
        AtualizarUI();
        AtualizarLuz();
        Debug.Log("Novo dia começou: Dia " + diaAtual);
    }

    public int GetHoraAtual()
    {
        return horas;
    }
}