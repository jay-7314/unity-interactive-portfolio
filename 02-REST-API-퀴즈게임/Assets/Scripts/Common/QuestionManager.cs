using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Question
{
    public List<TextData> Questions;
}
public class QuestionManager : MonoBehaviour
{
    public static Question question;
    public static HashSet<int> usedIds = new HashSet<int>();

    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Question.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            question = JsonUtility.FromJson<Question>(json);
        }
    }
}
