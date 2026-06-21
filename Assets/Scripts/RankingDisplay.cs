// ---------------------------------------------------------
// RankingDisplay.cs
// 作成日:  2026/4/20
// 作成者:  星野愛由
// 概要:　保存されたランキングデータを読み込み、UIに表示する
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankingEntryUI
{
    public TextMeshProUGUI rankText;   // 順位を表示するテキスト
    public TextMeshProUGUI nameText;   // 名前を表示するテキスト
    public TextMeshProUGUI scoreText;  // スコアを表示するテキスト
}

public class RankingDisplay : MonoBehaviour
{
    [Header("ランキングUI設定（1位〜5位まで順番に登録してください）")]
    [SerializeField] private List<RankingEntryUI> rankingUIList = new List<RankingEntryUI>();

    private const int MAX_RANKING = 5; // 表示する最大件数

    // ============================================================================
    // 処理：起動時にランキングを表示する
    // ============================================================================
    private void Start()
    {
        RefreshDisplay();
    }


    // ============================================================================
    // RefreshDisplay：データを読み込んで各テキストを更新する
    // ============================================================================
    public void RefreshDisplay()
    {
        int displayCount = Mathf.Min(rankingUIList.Count, MAX_RANKING); // 登録されているUIの数と最大件数（5）の少ない方に合わせてループ（エラー防止）

        for (int i = 0; i < displayCount; i++)
        {
            string playerName = PlayerPrefs.GetString("RankName_" + i, "---"); // PlayerPrefsから保存された名前とスコアを取得
            int playerScore = PlayerPrefs.GetInt("RankScore_" + i, 0);

            if (rankingUIList[i].rankText != null) rankingUIList[i].rankText.text = (i + 1).ToString() + "位"; // 順位のテキストを更新
            if (rankingUIList[i].nameText != null) rankingUIList[i].nameText.text = playerName; // 名前のテキストを更新
            if (rankingUIList[i].scoreText != null) rankingUIList[i].scoreText.text = playerScore.ToString(); // スコアのテキストを更新
        }
    }

    // ============================================================================
    // 処理：ランキングを初期化（デバッグ用）
    // 　　※インスペクターからスクリプトを右クリックし、Reset Ranking Dataで実行
    // ============================================================================
    [ContextMenu("Reset Ranking Data")]
    public void ResetRankingData()
    {
        for (int i = 0; i < MAX_RANKING; i++)
        {
            PlayerPrefs.DeleteKey("RankName_" + i);
            PlayerPrefs.DeleteKey("RankScore_" + i);
        }
        PlayerPrefs.Save();
        RefreshDisplay(); // リセット後、すぐに表示を更新
    }
}
