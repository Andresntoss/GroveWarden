using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersistor : MonoBehaviour
{
    public static ScenePersistor instance;

    [Header("Objetos a Persistir")]
    public GameObject player;
    public GameObject mainCanvas;
    public GameObject transitionManager;
    public GameObject mainCamera;
    public GameObject gameClock; // NOVO: Objeto do relógio

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
            return;
        }

        PersistObjects();
    }

    private void PersistObjects()
    {
        if (player != null)
        {
            DontDestroyOnLoad(player);
        }
        if (mainCanvas != null)
        {
            DontDestroyOnLoad(mainCanvas);
        }
        if (transitionManager != null)
        {
            DontDestroyOnLoad(transitionManager);
        }
        if (mainCamera != null)
        {
            DontDestroyOnLoad(mainCamera);
        }
        // NOVO: Persiste o objeto do relógio
        if (gameClock != null)
        {
            DontDestroyOnLoad(gameClock);
        }
    }
}