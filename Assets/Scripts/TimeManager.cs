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
    public float tempoDia = 600f;       // 10 minutos (dia)
    public float tempoNoite = 600f;     // 10 minutos (noite)
    
    [Header("Elementos de UI")]
    public TextMeshProUGUI relogioTexto;
    public TextMeshProUGUI contadorDiaTexto;
    
    [Header("Efeitos Visuais")]
    public Light2D direcionalLuz; // A luz direcional na sua cena (sol/lua)
    public float intensidadeDia = 1f;
    public float intensidadeNoite = 0.3f;
    public Color corDia = Color.white;
    public Color corNoite = new Color(0.1f, 0.1f, 0.3f); // Um azul escuro
    
    // ----- Variáveis Privadas (para o sistema) -----
    private float tempoAtual;
    private int diaAtual = 1;

    // ----- Relógio e Horário do Jogo -----
    private int horas = 6;
    private int minutos = 0;
    
    // Flag para verificar se o tempo deve continuar correndo
    private bool podeAvançarTempo = true;
    
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
        // Apenas avança o tempo se a flag 'podeAvançarTempo' for verdadeira
        if (podeAvançarTempo)
        {
            // Aumenta o tempo a cada frame
            tempoAtual += Time.deltaTime;
        }

        // Converte o tempo do jogo para horas e minutos
        float tempoDecorrenteDia = (tempoAtual / tempoTotalDia) * 24f;
        
        // Adiciona 6 horas para começar o dia às 6 da manhã
        horas = 6 + (int)tempoDecorrenteDia;
        minutos = (int)((tempoDecorrenteDia - (int)tempoDecorrenteDia) * 60);

        // Verifica se a hora atual atingiu o limite de 2 da manhã (26h no ciclo de 24h)
        // Linha 47
        if (horas >= 26)
        {
            // Se sim, para o relógio e a luz
            podeAvançarTempo = false;
            // E define a hora final para 2 da manhã
            horas = 2;
            minutos = 0;
        }
        
        // Ajusta a transição de luz entre dia e noite
        float t = Mathf.Clamp01(tempoAtual / tempoDia);
        if (tempoAtual <= tempoDia)
        {
            // Transição para o dia
            direcionalLuz.intensity = Mathf.Lerp(intensidadeNoite, intensidadeDia, t);
            direcionalLuz.color = Color.Lerp(corNoite, corDia, t);
        }
        else
        {
            // Transição para a noite
            t = Mathf.Clamp01((tempoAtual - tempoDia) / tempoNoite);
            direcionalLuz.intensity = Mathf.Lerp(intensidadeDia, intensidadeNoite, t);
            direcionalLuz.color = Color.Lerp(corDia, corNoite, t);
        }
        
        // Atualiza a UI a cada frame
        AtualizarUI();
    }
    
    // Atualiza o texto do relógio e do contador de dias
    private void AtualizarUI()
    {
        // Formata a hora para HH:mm, usando o módulo para o ciclo 24h
        relogioTexto.text = string.Format("{0:00}:{1:00}", horas % 24, minutos);
        contadorDiaTexto.text = "Dia " + diaAtual.ToString();
    }
    
    // Atualiza a intensidade e cor da luz direcional
    private void AtualizarLuz()
    {
        if (direcionalLuz != null)
        {
            direcionalLuz.intensity = intensidadeDia;
            direcionalLuz.color = corDia;
        }
    }
    
    // Método público para ser chamado pela cama
    public void PassarParaProximoDia()
    {
        // Incrementa o contador de dias
        diaAtual++;
        
        // Reseta o tempo do ciclo
        tempoAtual = 0;
        
        // Permite que o tempo volte a correr
        podeAvançarTempo = true;
        
        // Reseta a hora e os minutos para 6 da manhã
        horas = 6;
        minutos = 0;
        
        // Atualiza UI e luz para o novo dia
        AtualizarUI();
        AtualizarLuz();
        
        Debug.Log("Novo dia começou: Dia " + diaAtual);
    }

    // Retorna a hora atual para que o personagem saiba a hora limite para ir para a cama
    public int GetHoraAtual()
    {
        return horas;
    }
}