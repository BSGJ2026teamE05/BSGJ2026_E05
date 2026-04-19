// ---------------------------------------------------------
// TitleRanking.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TitleRanking : MonoBehaviour
{
    // [SerializeField] private int _numId;
    [SerializeField] private TextMeshProUGUI rankingText;
    [SerializeField] private Ranking ranking;

    void Start()
    {
        string rankingKey = "testRanking";

        // 直前のスコアと名前を取得
        int lastScore = PlayerPrefs.GetInt("LastScore", -1);
        string lastName = PlayerPrefs.GetString("LastName", "");

        int highlightIndex = -1;

        // 有効なスコアがある場合のみランキングを更新
        if (lastScore >= 0 && lastName != "")
        {
            highlightIndex = ranking.RankingUpdate(rankingKey, lastScore, lastName);

            // 使用済みフラグをリセット（タイトルに戻るたびに再登録しないように）
            PlayerPrefs.SetInt("LastScore", -1);
            PlayerPrefs.SetString("LastName", "");
            PlayerPrefs.Save();
        }

        // ランキング表示
        ranking.rankingText = rankingText;
        ranking.RankingLoad(rankingKey, highlightIndex);
    }
}
