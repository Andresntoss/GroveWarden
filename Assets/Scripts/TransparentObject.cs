using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] private float _transparenceValue = 0.7f;
    [SerializeField] private float _transparenceFadeTime = .4f;
    
    private SpriteRenderer _spriteRender;
    private Coroutine _fadeCoroutine; // Variável para gerenciar a coroutine

    void Awake()
    {
        _spriteRender = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Linha 16
        // Usa a tag em vez de GetComponent para melhor performance
        if (collision.gameObject.CompareTag("Player"))
        {
            // Linha 18
            // Para a coroutine atual antes de iniciar uma nova
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTree(_transparenceFadeTime, _spriteRender.color.a, _transparenceValue));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Linha 25
        // Usa a tag em vez de GetComponent para melhor performance
        if (collision.gameObject.CompareTag("Player"))
        {
            // Linha 27
            // Para a coroutine atual antes de iniciar uma nova
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTree(_transparenceFadeTime, _spriteRender.color.a, 1f));
        }
    }

    private IEnumerator FadeTree(float _fadeTime, float _startValue, float _targetTransparency)
    {
        float _timeElapsed = 0;
        while (_timeElapsed < _fadeTime)
        {
            _timeElapsed += Time.deltaTime;
            float _newAlpha = Mathf.Lerp(_startValue, _targetTransparency, _timeElapsed / _fadeTime);
            _spriteRender.color = new Color(_spriteRender.color.r, _spriteRender.color.g, _spriteRender.color.b, _newAlpha);
            yield return null;
        }
    }
}