using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#region Class
[System.Serializable]
public class CountryData
{
    public string countryBackgroundImg;     // countryBackground배경 변경
    public List<CountryList> countryLists;  // 해당 국가를 클릭하면 해당 동영상과 시간이 나옴
}

[System.Serializable]
public class CountryList
{
    public string id;
    public string CountryMovie;
}
#endregion

public class CountryJsonManager : MonoBehaviour
{
    public static CountryData country;
    public static Dictionary<string, CountryList> countryDic;

    private void Awake()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Settings/Country.json").Replace("\\", "/");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            country = JsonUtility.FromJson<CountryData>(json);

            countryDic = new Dictionary<string, CountryList>();
            for (int i = 0; i < country.countryLists.Count; i++)
            {
                var entry = country.countryLists[i];
                countryDic[entry.id] = entry;
            }
        }
    }
    #region CheckTimer
    public static (string movie, string time) CountryInfo(string id)
    {
        var data = countryDic[id];
        string movie = data.CountryMovie;
        string time = LocalTime(id);
        return (movie, time);
    }

    public static string LocalTime(string id)
    {
        string timeZoneId = GetTimeZoneId(id);
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return localTime.ToString("HH:mm:ss");
    }

    public static string GetTimeInfo(string id, string displayName)
    {
        TimeZoneInfo koreaTz = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
        DateTime koreaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, koreaTz);

        string timeZoneId = GetTimeZoneId(id);
        TimeZoneInfo targetTz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        DateTime targetTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, targetTz);

        TimeSpan diff = targetTime - koreaTime;
        int hours = Mathf.Abs((int)Math.Round(diff.TotalHours));
        string diffText = hours > 0 ? $"{hours}시간 시차" : "시차없음";

        string koreaStr = $"대구광역시 현재 시간 : {FormatFullTime(koreaTime)}";
        string targetStr = $"{displayName} 현재 시간 : {FormatFullTime(targetTime)}";

        return $"{koreaStr}\n{targetStr}\n{diffText}";
    }

    private static string GetTimeZoneId(string id)
    {
        switch (id)
        {
            case "London": return "GMT Standard Time";
            case "NewYork": return "Eastern Standard Time";
            case "Paris": return "Romance Standard Time";
            case "Cairo": return "Egypt Standard Time";
            case "Sydney": return "AUS Eastern Standard Time";
            case "Hanoi": return "SE Asia Standard Time";
            case "Riopde": return "E. South America Standard Time";
            default: return "Korea Standard Time";
        }
    }
    private static string FormatFullTime(DateTime dt)
    {
        string day = dt.ToString("dddd", new System.Globalization.CultureInfo("ko-KR"));
        string ampm = dt.ToString("tt", new System.Globalization.CultureInfo("ko-KR"));
        return $"{day} {ampm} {dt:hh시 mm분 ss초}";
    }
    #endregion
}
