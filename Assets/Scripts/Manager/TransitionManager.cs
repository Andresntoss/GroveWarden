using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Cinemachine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;
    
    [Header("Configurações de Transição")]
    public float fadeTime = 1.0f;
    public Image fadePanel;

    private string _previousSceneName;

    public void LoadScene(string sceneName)
    {
        // NOVO: Armazena o nome da cena atual como a cena anterior
        _previousSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(FadeIn(sceneName));
    }
    
    // NOVO: Método para voltar para a cena anterior
    public void LoadPreviousScene()
    {
        if (!string.IsNullOrEmpty(_previousSceneName))
        {
            StartCoroutine(FadeIn(_previousSceneName));
        }
        else
        {
            Debug.LogError("Cena anterior não encontrada!");
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
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerController.instance != null)
        {
            GameObject startPoint = GameObject.Find("StartPoint");
            if (startPoint != null)
            {
                PlayerController.instance.transform.position = startPoint.transform.position;
            }
            
            CinemachineCamera virtualCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            if (virtualCamera != null)
            {
                virtualCamera.Follow = PlayerController.instance.transform;
                virtualCamera.LookAt = PlayerController.instance.transform;
            }
        }

        StartCoroutine(FadeOut());
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
}