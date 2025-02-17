using System.Collections.Generic;
using System.Linq;
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

    public void SetAnswer(bool isCorrect, List<string> myAnswers)
    {
        imageIsCorrect = GetComponent<Image>();
        imageIsCorrect.color = isCorrect ? correct : error;
        myAnswer.text = "";

        string additionnal = ",";
        foreach (var item in myAnswers)
        {
            myAnswer.text += item;
            if (myAnswers.Count > 1 && myAnswers.Last() != item)
            {
                myAnswer.text += additionnal;
            }
        }
        //this.theAnswer.text = theAnswer;
    }
}
