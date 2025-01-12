using UnityEngine;

public class Parameter : MonoBehaviour
{
    private CanvasAuthManager canvasAuthManager;
    [SerializeField] private CurrentProfilUI currentProfilUI;

    private void Start()
    {
        canvasAuthManager = CanvasAuthManager.Instance;
    }

    public void ActiveSelectionProfilUI()
    {
        canvasAuthManager.ActiveSelectionProfil(() =>
        {
            canvasAuthManager.MyProfilsUI.gameObject.SetActive(false);
            canvasAuthManager.gameObject.SetActive(false);
            currentProfilUI.SetContent();
            gameObject.SetActive(false);
        });
    }
}
