using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UIElements.UxmlAttributeDescription;

[System.Serializable]
public class RoomResponse
{
    public string[] COLUMNS;
    public List<List<object>> DATA;
}

public class NetworkManager : MonoBehaviour
{
    public string leftUser = "";
    public string rightUser = "";
    public string uniqueNumber = "";

    // 유저 기본 정보
    public string lang = "";
    public string relation = "";
    public string leftColor = "";
    public string rightColor = "";
    public string leftName = "";
    public string rightName = "";
    public string uniqueId = "";

    // 완료 여부
    public string A1Finish = "";
    public string B1Finish = "";
    public string C1Finish = "";
    public string D1Finish = "";
    public string A2Finish = "";
    public string B2Finish = "";
    public string C2Finish = "";
    public string D2Finish = "";
    public string A3Finish = "";
    public string B3Finish = "";
    public string C3Finish = "";
    public string D3Finish = "";

    // 마음조각 갯수
    public string A1MindCount = "";
    public string B1MindCount = "";
    public string C1MindCount = "";
    public string D1MindCount = "";
    public string A2MindCount = "";
    public string B2MindCount = "";
    public string C2MindCount = "";
    public string D2MindCount = "";
    public string A3MindCount = "";
    public string B3MindCount = "";
    public string C3MindCount = "";
    public string D3MindCount = "";

    //사람이 둘다 들어갔는지 확인하는 코드
    public IEnumerator GetUserInsertData(string user1, string user2)
    {
        string UserInsertAPI = $"http://192.0.2.1:8500/api/enterRoom.cfm?code=C3&uid1={user1}&uid2={user2}";
        using (UnityWebRequest request = UnityWebRequest.Get(UserInsertAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다. 다시 요청합니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetRoomCheckData());
            }
        }

    }

    //유저가 방에 들어왔는지 확인
    public IEnumerator GetRoomCheckData()
    {
        string RoomCheckAPI = "http://192.0.2.1:8500/api/checkRoomState.cfm?code=C3";          //유저가 방에 왔는지 확인하는 Api
        using (UnityWebRequest request = UnityWebRequest.Get(RoomCheckAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다. 다시 요청합니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetRoomCheckData());
                yield break;
            }

            //응답 텍스트 가져오기
            string response = request.downloadHandler.text.Trim();

            if (response == "USING")
            {
                yield return StartCoroutine(GetUserData());
                Debug.Log("사람들 아이디 값을 가져오는데 성공하였습니다.");

            }
            else if (response == "EMPTY")
            {
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetRoomCheckData());
                Debug.Log("유저가 있는지 5초에 한번 확인합니다.");
            }
        }
    }

    //사람 아이디 값 가져오기
    public IEnumerator GetUserData()
    {
        string GetUserAPI = "http://192.0.2.1:8500/api/getCurrentRoomUser.cfm?code=C3";        //유저 정보 가져오기(사람 고유 번호 가져오기 / 데이터는 아님)

        using (UnityWebRequest request = UnityWebRequest.Get(GetUserAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다. 다시 요청합니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetUserData());
                yield break;
            }

            //응답 텍스트 가져오기
            string response = request.downloadHandler.text.Trim();

            string[] part = response.Split(',');
            if (part.Length < 3)
            {
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetRoomCheckData());
                yield break;
            }
            leftUser = part[0].Trim();
            rightUser = part[1].Trim();
            uniqueNumber = part[2].Trim();
        }
    }

    //유저 디테일 가져오되 언어, 관계, 왼쪽, 오른쪽 사람 이름과, 원의 색상, 지금까지 얻은 마음조각 갯수, 몇번째 플레이인지 확인
    public IEnumerator GetUserData_Detail(string person)
    {
        string UserDetail = $"http://192.0.2.1:8500/api/getUser.cfm?uid={person}";                      //유저의 디테일 값을 가져옴
        using (UnityWebRequest request = UnityWebRequest.Get($"{UserDetail}"))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다. 다시 요청합니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(GetUserData_Detail(person));
            }

            try
            {
                var json = request.downloadHandler.text;
                var data = JsonConvert.DeserializeObject<RoomResponse>(json);

                //칼럼 이름으로 인덱스 가져옴
                int GetIndex(string columnName) => System.Array.IndexOf(data.COLUMNS, columnName);
                string GetValue(string columnName) => data.DATA[0][GetIndex(columnName)]?.ToString() ?? "null";

                lang = GetValue("LANG");                                         //언어정보 가져오기(소문자 ko, en, jp인듯..)
                relation = GetValue("RELATION");                                 //관계 -> 관계를 통해서 지문이 바뀌지 않을까?
                leftColor = GetValue("COLOR_LEFT");                              //왼쪽 원 색상
                rightColor = GetValue("COLOR_RIGHT");                            //오른쪽 원 색상
                leftName = GetValue("RESERVATION_FIRST_NAME_LEFT");               //왼쪽 사람 이름
                rightName = GetValue("RESERVATION_FIRST_NAME_RIGHT");             //오른쪽 사람 이름
                uniqueId = GetValue("IDX_USER");                                 //유저 고유아이디값 가져오기

                //해당 부분은 끝난 부분을 체크해서 정보의 갯수로 3개가 있으면 이게 마지막, 정보가 2개 이하이면 다음으로 이동하라고 지시
                A1Finish = GetValue("END_A1");                                   //A1 끝남
                B1Finish = GetValue("END_B1");                                   //B1 끝남
                C1Finish = GetValue("END_C1");                                   //C1 끝남
                D1Finish = GetValue("END_D1");                                   //D1 끝남
                A2Finish = GetValue("END_A2");                                   //A2 끝남
                B2Finish = GetValue("END_B2");                                   //B2 끝남
                C2Finish = GetValue("END_C2");                                   //C2 끝남
                D2Finish = GetValue("END_D2");                                   //D2 끝남
                A3Finish = GetValue("END_A3");                                   //A3 끝남
                B3Finish = GetValue("END_B3");                                   //B3 끝남
                C3Finish = GetValue("END_C3");                                   //C3 끝남
                D3Finish = GetValue("END_D3");                                   //D3 끝남

                //해당부분은 마음조각 갯수 파악용
                A1MindCount = GetValue("PIECE_A1");                              //A1 마음조각
                B1MindCount = GetValue("PIECE_B1");                              //B1 마음조각
                C1MindCount = GetValue("PIECE_C1");                              //C1 마음조각
                D1MindCount = GetValue("PIECE_D1");                              //D1 마음조각
                A2MindCount = GetValue("PIECE_A2");                              //A2 마음조각
                B2MindCount = GetValue("PIECE_B2");                              //B2 마음조각
                C2MindCount = GetValue("PIECE_C2");                              //C2 마음조각
                D2MindCount = GetValue("PIECE_D2");                              //D2 마음조각
                A3MindCount = GetValue("PIECE_A3");                              //A3 마음조각
                B3MindCount = GetValue("PIECE_B3");                              //B3 마음조각
                C3MindCount = GetValue("PIECE_C3");                              //C3 마음조각
                D3MindCount = GetValue("PIECE_D3");                              //D3 마음조각
            }

            catch (Exception e)
            {
                Debug.Log($"파싱에 실패하였습니다. {e.Message}");
            }
        }
    }

    //함수 한번에 몰기
    public IEnumerator GetDetailAndStart(string person, string side)
    {
        yield return StartCoroutine(GetUserData_Detail(person));
        yield return StartCoroutine(UpdateStart(uniqueId, side));
    }

    //내가 받은 마음조각 갯수 업데이트 -> 서버로 보내는것
    public IEnumerator UpdateMindCount(int value)
    {
        string updateMindAPI = $"http://192.0.2.1:8500/api/updatePiece.cfm?idx_user={uniqueId}&code=C3&value={value}";
        using (UnityWebRequest request = UnityWebRequest.Get(updateMindAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateMindCount(value));
            }
        }
    }

    //문제 데이터를 서버로 전달함 -> 서버로 보내는것
    public IEnumerator UpdateQuestion(int questionsId, string side, string answerNum)
    {
        string updateQuestionAPI = $"http://192.0.2.1:8500/api/updateValue.cfm?idx_user={uniqueId}&q_no={questionsId}&side={side}&code=C3&value={answerNum}";
        using (UnityWebRequest request = UnityWebRequest.Get(updateQuestionAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateQuestion(questionsId, side, answerNum));

            }
        }
    }

    //서버로 게임시작 데이터 보냄
    public IEnumerator UpdateStart(string uniquePerson, string side)
    {
        string updateStartAPI = $"http://192.0.2.1:8500/api/updateTime.cfm?idx_user={uniquePerson}&option=start&side={side}&code=C3";
        using (UnityWebRequest request = UnityWebRequest.Get(updateStartAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateStart(uniquePerson, side));

            }
        }
    }

    //서버로 게임종료 데이터 보냄
    public IEnumerator UpdateEnd()
    {
        string updateEndAPI = $"http://192.0.2.1:8500/api/updateTime.cfm?idx_user={uniqueId}&option=end&code=C3";
        using (UnityWebRequest request = UnityWebRequest.Get(updateEndAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateEnd());

            }
        }
    }

    //사용자가 중간에 나가면 초기화 하는 로직
    public IEnumerator UpdateRestart()
    {
        string updateRestartAPI = $"http://192.0.2.1:8500/api/resetStart.cfm?idx_user={uniqueId}&code=C3";
        using (UnityWebRequest request = UnityWebRequest.Get(updateRestartAPI))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateRestart());
            }
        }
    }

    //방점유 초기화
    public IEnumerator ClearRoomInsertPerson()
    {
        string updateRestartAPI = $"http://192.0.2.1:8500/api/exitRoom.cfm?code=C3&idx_user={uniqueId}";
        using (UnityWebRequest request = UnityWebRequest.Get(updateRestartAPI))
        {

            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("서버 연결이 원활하지 않습니다.");
                yield return CoroutineData.GetWaitForSeconds(5f);
                StartCoroutine(UpdateRestart());
            }
        }
    }

    //C3에서 마지막 부분에 나올 대사용
    public int GetFinishCount()
    {
        string[] finished = { A1Finish, B1Finish, C1Finish, D1Finish, A2Finish, B2Finish, C2Finish, D2Finish, A3Finish, B3Finish, C3Finish, D3Finish };
        int count = 0;
        foreach (string finish in finished)
        {
            if (!string.IsNullOrEmpty(finish) && finish != "null") count++;
        }
        return count;
    }
}
