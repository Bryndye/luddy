using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    private CanvasAuthManager _canvasAuthManager;
    [SerializeField] private CanvasMainManager _canvasManager;
    private bool _initialized = false;

    void Awake()
    {
        ActiveTitleScreen();
    }

    private void Start()
    {
        _canvasAuthManager = CanvasAuthManager.Instance;
    }

    void Update()
    {
        if (!_initialized && Input.GetKeyDown(KeyCode.Mouse0))
        {
            _canvasAuthManager.ActiveAuthentification(() =>
            {
                _canvasManager.gameObject.SetActive(true);
            });
            gameObject.SetActive(false);
        }
    }

    // La fonction a appelé pour revenir à l'écran titrew
    public void ActiveTitleScreen()
    {
        _initialized = false;
        gameObject.SetActive(true);
    }
}