using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Questions_Txt : MonoBehaviour
{
    [SerializeField] Text questionsTxt;
    HashSet<string> usedIds = new HashSet<string>();
    [SerializeField] NetworkManager networkManager;

    public int selectedId = -1;

    private void OnEnable()
    {
        QuestionsText();
    }
    public void QuestionsText()
    {
        List<TextData> available = QuestionManager.question.Questions.FindAll(q => !usedIds.Contains(q.id));

        if (available.Count == 0)
        {
            usedIds.Clear();
            available = QuestionManager.question.Questions;
        }

        TextData selected = available[Random.Range(0, available.Count)];
        usedIds.Add(selected.id);
        selectedId = int.Parse(selected.id);
        LoadManager.Instance.LoadText(selected, questionsTxt, networkManager.lang);
    }
}
