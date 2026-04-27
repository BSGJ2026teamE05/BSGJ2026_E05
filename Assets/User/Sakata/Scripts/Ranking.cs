// ---------------------------------------------------------
// Ranking.cs
// 作成日:  2026/4/19
// 作成者:  坂田
// 概要:ランキング
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ranking : MonoBehaviour
{
    public TextMeshProUGUI rankingText;

    [SerializeField]
    private class RankingEntry
    {
        public string name;
        public int score;
    }

    // ─────────────────────────────────────────
    // Start：PlayerPrefsからスコアを取得してランキングを更新・表示
    // ─────────────────────────────────────────

    private void Start()
{
    Debug.Log("HasKey LastScore: " + PlayerPrefs.HasKey("LastScore"));
    Debug.Log("LastScore value: " + PlayerPrefs.GetInt("LastScore", -1));

    if (!PlayerPrefs.HasKey("LastScore"))
    {
        RankingLoad("Ranking");
        return;
    }

    int lastScore = PlayerPrefs.GetInt("LastScore", 0);
    string playerName = "Player";
    string rankingKey = "Ranking";

    int newRank = RankingUpdate(rankingKey, lastScore, playerName);

    // ★追加：保存直後のPlayerPrefsの中身を確認
    Debug.Log("Ranking保存後の生データ: " + PlayerPrefs.GetString(rankingKey, "なし"));
    Debug.Log("newRank: " + newRank);

    RankingLoad(rankingKey, newRank);

    // ★追加：rankingTextへの反映確認
    Debug.Log("rankingText.text: " + (rankingText != null ? rankingText.text : "nullです！"));

    PlayerPrefs.DeleteKey("LastScore");
    PlayerPrefs.Save();
}

    public int RankingUpdate(string rankingKey, int newScore, string newName)
    {
        // 既存データ読み込み（形式: "名前:スコア,名前:スコア,..."）
        string rawData = PlayerPrefs.GetString(rankingKey, "");
        RankingEntry[] entries = ParseEntries(rawData, 5);

        // 新エントリーを末尾に追加して一時配列を作る
        RankingEntry[] extended = new RankingEntry[entries.Length + 1];
        for (int i = 0; i < entries.Length; i++) extended[i] = entries[i];
        extended[entries.Length] = new RankingEntry { name = newName, score = newScore };

        // スコア降順でソート
        Array.Sort(extended, (a, b) => b.score.CompareTo(a.score));

        // 上位5件だけ保存
        int saveCount = Mathf.Min(extended.Length, 5);
        string[] parts = new string[saveCount];
        int newRankIndex = -1;

        for (int i = 0; i < saveCount; i++)
        {
            parts[i] = $"{extended[i].name}:{extended[i].score}";

            // 新エントリーの順位を記録（同スコア・同名で最初に見つかった位置）
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

    // ─────────────────────────────────────────
    // RankingLoad：ランキングを表示する
    // ─────────────────────────────────────────
    public void RankingLoad(string rankingKey, int highlightIndex = -1)
    {
        string rawData = PlayerPrefs.GetString(rankingKey, "");
        RankingEntry[] entries = ParseEntries(rawData, 5);

        string displayText = "";
        for (int i = 0; i < entries.Length; i++)
        {
            string line = $"{i + 1}:{entries[i].name} {entries[i].score}";
            if (i == highlightIndex)
            {
                line = $"<color=#FFD700>{line}</color>";
            }
            displayText += line + "\n";
        }

        if (rankingText != null)
        {
            rankingText.text = displayText;
        }

        Debug.Log(displayText);
    }

    // ─────────────────────────────────────────
    // ParseEntries：保存データをパースしてRankingEntry配列にする
    // ─────────────────────────────────────────
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
                // データが足りない場合はデフォルト値
                result[i] = new RankingEntry { name = "---", score = 0 };
            }
        }

        return result;
    }
}