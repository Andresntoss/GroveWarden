using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;

    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeTime = 1.0f;

    private void Awake()
    {
        // Singleton simples
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // tenta recuperar o Image se não ligado no Inspetor
        if (fadePanel == null)
        {
            fadePanel = GetComponentInChildren<Image>(true);
            if (fadePanel == null)
                Debug.LogWarning("[FadeManager] fadePanel não atribuído e nenhum Image filho foi encontrado.");
        }
    }

    // Use StartCoroutine(FadeManager.instance.FadeAndPassDay()) para iniciar
    public IEnumerator FadeAndPassDay()
    {
        if (!EnsureFadePanel()) yield break;

        yield return StartCoroutine(FadeIn());

        // Espera pelo GameManager.instance (timeout para evitar loop infinito)
        float waitTimeout = 2.0f; // segundos
        float waited = 0f;
        while (GameManager.instance == null && waited < waitTimeout)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        if (GameManager.instance != null)
        {
            FindAnyObjectByType<TimeManager>()?.PassarParaProximoDia();
        }
        else
        {
            Debug.LogError("[FadeManager] GameManager.instance ainda é nulo depois de esperar " + waitTimeout + "s. Verifique se o GameManager está na cena e inicializando corretamente.");
        }

        yield return StartCoroutine(FadeOut());
    }

    public IEnumerator FadeAndTeleport(GameObject player, Vector3 targetPosition)
    {
        if (!EnsureFadePanel()) yield break;

        yield return StartCoroutine(FadeIn());

        if (player != null)
            player.transform.position = targetPosition;
        else
            Debug.LogError("[FadeManager] player nulo em FadeAndTeleport.");

        yield return StartCoroutine(FadeOut());
    }

    #region Helpers de Fade
    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        Color color = fadePanel.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeTime);
            fadePanel.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        Color color = fadePanel.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeTime));
            fadePanel.color = color;
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
    }

    private bool EnsureFadePanel()
    {
        if (fadePanel == null)
        {
            Debug.LogError("[FadeManager] fadePanel não atribuído. Arraste um Image no Inspetor ou certifique-se de que exista um Image filho.");
            return false;
        }
        return true;
    }
    #endregion
}
