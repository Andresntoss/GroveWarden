using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance; // O singleton do script
    
    [Header("Configurações de Transição")]
    public float fadeTime = 1.0f;
    public Image fadePanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // O script não será destruído ao carregar uma nova cena
        }
        else
        {
            Destroy(gameObject); // Garante que só há uma instância
        }
    }

    // Este método é chamado para carregar uma nova cena
    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // Fade in (escurece a tela)
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

        // Carrega a cena
        SceneManager.LoadScene(sceneName);

        // Fade out (clareia a tela)
        elapsedTime = 0f;
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