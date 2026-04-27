// ---------------------------------------------------------
// TitleRanking.cs
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleRanking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankingText;

    private const string RankingKey = "testRanking";

    private class RankingEntry
    {
        public string name;
        public int score;
    }

    void Start()
    {
        int lastScore = PlayerPrefs.GetInt("LastScore", -1);
        string lastName = PlayerPrefs.GetString("LastName", "NoName");

        int highlightIndex = -1;

        // スコアが有効なら登録（LastNameが空でも "NoName" で登録）
        if (lastScore >= 0)
        {
            highlightIndex = RankingUpdate(RankingKey, lastScore, lastName);

            PlayerPrefs.SetInt("LastScore", -1);
            PlayerPrefs.DeleteKey("LastName");
            PlayerPrefs.Save();
        }

        RankingLoad(RankingKey, highlightIndex);
    }

    public int RankingUpdate(string rankingKey, int newScore, string newName)
    {
        string rawData = PlayerPrefs.GetString(rankingKey, "");
        RankingEntry[] entries = ParseEntries(rawData, 5);

        RankingEntry[] extended = new RankingEntry[entries.Length + 1];
        for (int i = 0; i < entries.Length; i++) extended[i] = entries[i];
        extended[entries.Length] = new RankingEntry { name = newName, score = newScore };

        Array.Sort(extended, (a, b) => b.score.CompareTo(a.score));

        int saveCount = Mathf.Min(extended.Length, 5);
        string[] parts = new string[saveCount];
        int newRankIndex = -1;

        for (int i = 0; i < saveCount; i++)
        {
            parts[i] = $"{extended[i].name}:{extended[i].score}";
            if (newRankIndex == -1
                && extended[i].score == newScore
                && extended[i].name == newName)
            {
                newRankIndex = i;
            }
        }

        PlayerPrefs.SetString(rankingKey, string.Join(",", parts));
        PlayerPrefs.Save();
        return newRankIndex;
    }

    public void RankingLoad(string rankingKey, int highlightIndex = -1)
    {
        string rawData = PlayerPrefs.GetString(rankingKey, "");
        RankingEntry[] entries = ParseEntries(rawData, 5);

        string displayText = "";
        for (int i = 0; i < entries.Length; i++)
        {
            string line = $"{i + 1}:{entries[i].name} {entries[i].score}";
            if (i == highlightIndex)
                line = $"<color=#FFD700>{line}</color>";
            displayText += line + "\n";
        }

        if (rankingText != null)
            rankingText.text = displayText;

        Debug.Log(displayText);
    }

    private RankingEntry[] ParseEntries(string rawData, int topN)
    {
        RankingEntry[] result = new RankingEntry[topN];
        string[] pairs = (rawData != "") ? rawData.Split(',') : new string[0];

        for (int i = 0; i < topN; i++)
        {
            if (i < pairs.Length)
            {
                string[] kv = pairs[i].Split(':');
                result[i] = new RankingEntry
                {
                    name = kv.Length > 0 ? kv[0] : "---",
                    score = kv.Length > 1 && int.TryParse(kv[1], out int s) ? s : 0
                };
            }
            else
            {
                result[i] = new RankingEntry { name = "---", score = 0 };
            }
        }
        return result;
    }
}