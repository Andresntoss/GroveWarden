using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    public static GameClock instance;

    [Header("Configurações do Relógio")]
    public float timeScale = 1.0f;
    public TextMeshProUGUI clockText;

    private float _currentTime;

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
        
        // NOVO: A inicialização acontece apenas uma vez
        _currentTime = 0;
        UpdateClockDisplay();
    }

    void Update()
    {
        _currentTime += Time.deltaTime * timeScale;
        UpdateClockDisplay();
    }

    private void UpdateClockDisplay()
    {
        int minutes = Mathf.FloorToInt(_currentTime / 60);
        int hours = Mathf.FloorToInt(minutes / 60);
        minutes = minutes % 60;
        
        string timeString = string.Format("{0:00}:{1:00}", hours, minutes);
        if (clockText != null)
        {
            clockText.text = timeString;
        }
    }
}