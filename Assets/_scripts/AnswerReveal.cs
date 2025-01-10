using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerReveal : MonoBehaviour
{
    [SerializeField] private Image imageIsCorrect;
    [SerializeField] private TextMeshProUGUI myAnswer;
    [SerializeField] private TextMeshProUGUI theAnswer;


    public void SetAnswer(bool isCorrect, string myAnswer, string theAnswer)
    {
        imageIsCorrect.color = isCorrect ? Color.green : Color.red;
        this.myAnswer.text = myAnswer;
        this.theAnswer.text = theAnswer;
    }
}
