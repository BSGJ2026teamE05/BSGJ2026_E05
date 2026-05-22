
// ---------------------------------------------------------
// TitleRanking.cs
// ---------------------------------------------------------
using System;
using UnityEngine;
using TMPro;

public class TitleRanking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankingText;
    private const int MAX_RANKING = 5;

    void Start()
    {
        RankingLoad();
    }

    public void RankingLoad()
    {
        string displayText = "";

        for (int i = 0; i < MAX_RANKING; i++)
        {
            string name = PlayerPrefs.GetString("RankName_" + i, "---");
            int score = PlayerPrefs.GetInt("RankScore_" + i, 0);
            displayText += $"{i + 1}:{name} {score}\n";
        }

        if (rankingText != null)
            rankingText.text = displayText;

        Debug.Log(displayText);
    }
}