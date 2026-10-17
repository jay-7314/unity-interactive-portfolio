using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public Text displayText; // 결과 표시할 Text
    void Start()
    {
        Debug.Log($"ㅇ index in jongsung: {IndexOf(jongsung_index, 'ㅇ')}");
    }
    private string _result = "";

    // 초성 19개
    private char[] chosung_index = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
    // 중성 21개
    private char[] joongsung_index = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
    // 종성 28개 (0번은 종성 없음)
    private char[] jongsung_index = { ' ', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

    private char[] Jcombo_index = { 'ㄳ', 'ㄵ', 'ㄶ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅄ' };
    private string[] Jcombo = { "ㄱㅅ", "ㄴㅈ", "ㄴㅎ", "ㄹㄱ", "ㄹㅁ", "ㄹㅂ", "ㄹㅅ", "ㄹㅌ", "ㄹㅍ", "ㄹㅎ", "ㅂㅅ" };

    private char[] Mcombo_index = { 'ㅘ', 'ㅙ', 'ㅚ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅢ' };
    private string[] Mcombo = { "ㅗㅏ", "ㅗㅐ", "ㅗㅣ", "ㅜㅓ", "ㅜㅔ", "ㅜㅣ", "ㅡㅣ" };


    public void OnClickHangul(string input)
    {
        string next = AppendHangul(_result, input[0]);
        Debug.Log($"_result: '{_result}' / next: '{next}'");

        if (next.Length > 10) return;
        _result = next;
        displayText.text = _result;
    }

    public void OnClickSpace()
    {
        if (_result.Length >= 11) return;
        _result += " ";
        displayText.text = _result;
    }

    public void OnClickBackspace()
    {
        _result = DeleteLast(_result);
        displayText.text = _result;
    }

 
    private string AppendHangul(string current, char input)
    {
        if (current.Length == 0 || !IsKorean(current[current.Length - 1]))
        {
            return current + input;
        }

        char lastChar = current[current.Length - 1];
        char jm = IsJaOrMo(input);
        string base_str = current.Substring(0, current.Length - 1);

        char chosung = ' ', joongsung = ' ', jongsung = ' ';
        int cho_idx = -1, joong_idx = -1, jong_idx = -1;

        if (IsJaOrMo(lastChar) == 'J')
        {
            chosung = lastChar;
        }
        else if (IsJaOrMo(lastChar) == 'M')
        {
            joongsung = lastChar;
        }
        else
        {
            int uni = lastChar - 44032;
            jong_idx = uni % 28;
            joong_idx = Mathf.FloorToInt(uni / 28f) % 21;
            cho_idx = Mathf.FloorToInt(Mathf.FloorToInt(uni / 28f) / 21f);
            chosung = chosung_index[cho_idx];
            joongsung = joongsung_index[joong_idx];
            jongsung = jongsung_index[jong_idx];
        }

        string result = "";
        int key_idx = 0;

        if (lastChar == chosung && jm == 'J') // 1. 자음 + 자음
        {
            string combo = "" + chosung + input;
            key_idx = IndexOf(Jcombo, combo);
            if (key_idx != -1)
                return base_str + Jcombo_index[key_idx];
            else
                return current + input;
        }
        else if (lastChar == chosung && jm == 'M') // 2. 자음 + 모음
        {
            key_idx = IndexOf(joongsung_index, input);
            int comboIdx = IndexOf(Jcombo_index, lastChar);

            if (comboIdx != -1)
            {
                if (lastChar == 'ㄲ' || lastChar == 'ㄸ' || lastChar == 'ㅃ' || lastChar == 'ㅆ' || lastChar == 'ㅉ')
                {
                    int cho_i = IndexOf(chosung_index, lastChar);
                    return base_str + (char)(((cho_i * 21) + key_idx) * 28 + 44032);
                }
                else
                {
                    char ja1 = Jcombo[comboIdx][0];
                    char ja2 = Jcombo[comboIdx][1];
                    int cho_i = IndexOf(chosung_index, ja2);
                    return base_str + ja1 + (char)(((cho_i * 21) + key_idx) * 28 + 44032);
                }
            }
            else
            {
                int cho_i = IndexOf(chosung_index, lastChar);
                return base_str + (char)(((cho_i * 21) + key_idx) * 28 + 44032);
            }
        }
        else if (lastChar == joongsung && jm == 'J') // 3. 모음 + 자음
        {
            return current + input;
        }
        else if (lastChar == joongsung && jm == 'M') // 4. 모음 + 모음
        {
            string combo = "" + joongsung + input;
            key_idx = IndexOf(Mcombo, combo);
            if (key_idx != -1)
                return base_str + Mcombo_index[key_idx];
            else
                return current + input;
        }
        else if (jongsung != ' ' && jm == 'J') // 5. 종성 있음 + 자음
        {
            string combo = "" + jongsung + input;
            key_idx = IndexOf(Jcombo, combo);
            if (key_idx != -1)
            {
                cho_idx = IndexOf(chosung_index, chosung);
                joong_idx = IndexOf(joongsung_index, joongsung);
                jong_idx = IndexOf(jongsung_index, Jcombo_index[key_idx]);
                return base_str + (char)(((cho_idx * 21) + joong_idx) * 28 + jong_idx + 44032);
            }
            else
                return current + input;
        }
        else if (jongsung != ' ' && jm == 'M') // 6. 종성 있음 + 모음
        {
            int comboIdx = IndexOf(Jcombo_index, jongsung);
            cho_idx = IndexOf(chosung_index, chosung);
            joong_idx = IndexOf(joongsung_index, joongsung);
            int mo_idx = IndexOf(joongsung_index, input);

            if (comboIdx != -1)
            {
                char ja1 = Jcombo[comboIdx][0];
                char ja2 = Jcombo[comboIdx][1];
                int jong_i = IndexOf(jongsung_index, ja1);
                int cho_i2 = IndexOf(chosung_index, ja2);
                return base_str
                    + (char)(((cho_idx * 21) + joong_idx) * 28 + jong_i + 44032)
                    + (char)(((cho_i2 * 21) + mo_idx) * 28 + 44032);
            }
            else
            {
                int cho_i2 = IndexOf(chosung_index, jongsung);
                return base_str
                    + (char)(((cho_idx * 21) + joong_idx) * 28 + 44032)
                    + (char)(((cho_i2 * 21) + mo_idx) * 28 + 44032);
            }
        }
        else if (jongsung == ' ' && jm == 'J') // 7. 종성 없음 + 자음
        {
            Debug.Log($"case7 진입! chosung:{chosung} joongsung:{joongsung} jongsung:'{jongsung}' input:{input}");
            cho_idx = IndexOf(chosung_index, chosung);
            joong_idx = IndexOf(joongsung_index, joongsung);
            jong_idx = IndexOf(jongsung_index, input);
            Debug.Log($"jong_idx: {jong_idx}");
            if (jong_idx != -1)
                return base_str + (char)(((cho_idx * 21) + joong_idx) * 28 + jong_idx + 44032);
            else
                return current + input;
        }
        else if (jongsung == ' ' && jm == 'M') // 8. 종성 없음 + 모음
        {
            cho_idx = IndexOf(chosung_index, chosung);
            string combo = "" + joongsung + input;
            key_idx = IndexOf(Mcombo, combo);
            if (key_idx != -1)
            {
                joong_idx = IndexOf(joongsung_index, Mcombo_index[key_idx]);
                return base_str + (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
            }
            else
                return current + input;
        }

        return current + input;
    }

    private string DeleteLast(string current)
    {
        if (current.Length == 0) return current;

        char last = current[current.Length - 1];
        string base_str = current.Substring(0, current.Length - 1);

        if (IsJaOrMo(last) == 'J' || IsJaOrMo(last) == 'M')
            return base_str;

        // 한글 음절 범위 벗어나면 그냥 삭제
        if (last < 0xAC00 || last > 0xD7AF)
            return base_str;

        int uni = last - 44032;
        int jong_idx = uni % 28;
        int joong_idx = Mathf.FloorToInt(uni / 28f) % 21;
        int cho_idx = Mathf.FloorToInt(Mathf.FloorToInt(uni / 28f) / 21f);

        // 인덱스 범위 벗어나면 그냥 삭제
        if (cho_idx < 0 || cho_idx >= chosung_index.Length ||
            joong_idx < 0 || joong_idx >= joongsung_index.Length ||
            jong_idx < 0 || jong_idx >= jongsung_index.Length)
            return base_str;

        if (jong_idx != 0)
        {
            char jongChar = jongsung_index[jong_idx];
            int comboIdx = IndexOf(Jcombo_index, jongChar);
            if (comboIdx != -1)
            {
                char ja1 = Jcombo[comboIdx][0];
                int jong_i = IndexOf(jongsung_index, ja1);
                if (jong_i == -1) return base_str;
                return base_str + (char)(((cho_idx * 21) + joong_idx) * 28 + jong_i + 44032);
            }
            else
            {
                return base_str + (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
            }
        }
        else
        {
            return base_str + chosung_index[cho_idx];
        }
    }

    private char IsJaOrMo(char c)
    {
        if (c >= 12593 && c <= 12622) return 'J';
        if (c >= 12623 && c <= 12643) return 'M';
        return ' ';
    }

    private bool IsKorean(char c)
    {
        return (c >= 12593 && c <= 12622) || (c >= 12623 && c <= 12643) || (c >= 0xAC00 && c <= 0xD7AF);
    }

    private int IndexOf<T>(T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].Equals(value)) return i;
        return -1;
    }
}
