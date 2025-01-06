using System;
using System.Linq;
using UnityEngine;

public class WizardLevel : Wizard
{
    [Header("Level")]
    [SerializeField] private LevelInfos levelInfos;

    [Header("Stars")]
    [SerializeField] private GameObject star;
    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;

    public void ActiveWizardLevel(Action action, LevelInfos levelInfos)
    {
        // Set infos
        this.levelInfos = levelInfos;

        ActiveWizard(action);

        SetContents();
        SetStars();
    }

    private void SetContents()
    {
        title.text = "Niveau "+levelInfos.LevelId.ToString();
        content.text = levelInfos.Description;
    }

    private void SetStars()
    {
        AuthManager _auth = AuthManager.Instance;

        // R�cup�ration du niveau
        var _level = _auth.MyCurrentSubAccount.MyLevelDatasPlayer
            .FirstOrDefault(i => i.LevelId == levelInfos.LevelId);

        // Si le niveau est introuvable, d�sactiver toutes les �toiles et sortir
        if (_level == null)
        {
            SetStarsVisibility(false, false, false);
            return;
        }

        // Calcul du pourcentage de r�ussite
        var pourcentage = _level.PourcentagePass;

        Debug.Log($"Pourcentage de r�ussite : {pourcentage * 100}%");
        // Mettre � jour la visibilit� des �toiles en fonction du pourcentage
        SetStarsVisibility(
            pourcentage >= (1f / 3f),
            pourcentage >= (2f / 3f),
            pourcentage >= 1f
        );
    }

    /// <summary>
    /// Met � jour la visibilit� des �toiles.
    /// </summary>
    private void SetStarsVisibility(bool starVisible, bool star1Visible, bool star2Visible)
    {
        star.SetActive(starVisible);
        star1.SetActive(star1Visible);
        star2.SetActive(star2Visible);
    }
}