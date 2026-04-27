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

[System.Serializable]
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

    // ─────────────────────────────────────────
    // Start：起動時にランキングを表示する
    // ─────────────────────────────────────────
    private void Start()
    {
        RefreshDisplay();
    }

    // ─────────────────────────────────────────
    // RefreshDisplay：データを読み込んで各テキストを更新する
    // ─────────────────────────────────────────
    public void RefreshDisplay()
    {
        // 登録されているUIの数と最大件数（5）の少ない方に合わせてループ（エラー防止）
        int displayCount = Mathf.Min(rankingUIList.Count, MAX_RANKING);

        for (int i = 0; i < displayCount; i++)
        {
            // PlayerPrefsから保存された名前とスコアを取得
            string playerName = PlayerPrefs.GetString("RankName_" + i, "---");
            int playerScore = PlayerPrefs.GetInt("RankScore_" + i, 0);

            // ① 順位のテキストを更新（例として "1位" のように "位" をつけています）
            if (rankingUIList[i].rankText != null)
            {
                rankingUIList[i].rankText.text = (i + 1).ToString() + "位";
            }

            // ② 名前のテキストを更新
            if (rankingUIList[i].nameText != null)
            {
                rankingUIList[i].nameText.text = playerName;
            }

            // ③ スコアのテキストを更新
            if (rankingUIList[i].scoreText != null)
            {
                rankingUIList[i].scoreText.text = playerScore.ToString();
            }
        }
    }

    // ─────────────────────────────────────────
    // ResetRankingData：ランキングを初期化したい場合（デバッグ用）
    // インスペクターからこのスクリプトを右クリックし、「Reset Ranking Data」で実行
    // ─────────────────────────────────────────
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
        Debug.Log("ランキングデータをリセットしました。");
    }
}