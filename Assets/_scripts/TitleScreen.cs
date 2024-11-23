using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    private CanvasAuthManager _canvasAuthManager;
    [SerializeField] private CanvasMainManager _canvasManager;

    void Awake()
    {
        gameObject.SetActive(true);
    }

    private void Start()
    {
        _canvasAuthManager = CanvasAuthManager.Instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _canvasAuthManager.ActiveAuthentification(() =>
            {
                _canvasManager.gameObject.SetActive(true);
            });
            gameObject.SetActive(false);
        }
    }
}
