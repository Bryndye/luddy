using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerReveal : MonoBehaviour
{
    [SerializeField] private Image imageIsCorrect;
    [SerializeField] private TextMeshProUGUI myAnswer;
    [SerializeField] private TextMeshProUGUI theAnswer;
    [SerializeField] private Color correct;
    [SerializeField] private Color error;

    public void SetAnswer(bool isCorrect, string myAnswer, string theAnswer)
    {
        imageIsCorrect = GetComponent<Image>();
        imageIsCorrect.color = isCorrect ? correct : error;
        this.myAnswer.text = myAnswer;
        this.theAnswer.text = theAnswer;
    }
}
