using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject mainMenuPanel;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
    }
}
