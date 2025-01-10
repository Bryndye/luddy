using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WizardLevel : Wizard
{
    [Header("Level")]
    [SerializeField] private LevelInfos levelInfos;

    [Header("Stars")]
    [SerializeField] private Color activeStar;
    [SerializeField] private Color desactiveStar;
    [SerializeField] private Image star;
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;

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

        // Récupération du niveau
        var _level = _auth.MyCurrentSubAccount.MyLevelDatasPlayer
            .FirstOrDefault(i => i.LevelId == levelInfos.LevelId);

        // Si le niveau est introuvable, désactiver toutes les étoiles et sortir
        if (_level == null)
        {
            SetStarsVisibility(false, false, false);
            return;
        }

        // Calcul du pourcentage de réussite
        var pourcentage = _level.PourcentagePass;

        Debug.Log($"Pourcentage de réussite : {pourcentage * 100}%");
        // Mettre à jour la visibilité des étoiles en fonction du pourcentage
        SetStarsVisibility(
            pourcentage >= (1f / 3f),
            pourcentage >= (2f / 3f),
            pourcentage >= 1f
        );
    }

    /// <summary>
    /// Met à jour la visibilité des étoiles.
    /// </summary>
    private void SetStarsVisibility(bool starVisible, bool star1Visible, bool star2Visible)
    {
        star.color = starVisible == true ? activeStar : desactiveStar;
        star1.color = star1Visible == true ? activeStar : desactiveStar;
        star2.color = star2Visible == true ? activeStar : desactiveStar;
    }
}