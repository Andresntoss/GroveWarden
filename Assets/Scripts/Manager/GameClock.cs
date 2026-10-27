using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    public static GameClock instance;

    [Header("Configurações do Relógio")]
    public float timeScale = 1.0f;
    public TextMeshProUGUI clockText;

    private float _currentTime;
    public float SaveTime => _currentTime;
    public void RestoreTime(float value) { _currentTime = Mathf.Max(0,value); UpdateClockDisplay(); }

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