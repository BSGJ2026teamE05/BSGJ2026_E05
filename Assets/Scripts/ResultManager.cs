// ---------------------------------------------------------
// ResultManager.cs
// 作成日:  2026/4/17
// 作成者:  星野愛由
// 概要:　名前入力（文字数の制限）　※ランキング処理も含む
// ---------------------------------------------------------
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject nameEntryWindow;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI FinalScoreText;

    [Header("文字数制限")]
    private const int MAX_NameLength = 14;

    private int finalScore = 0;
    private int rankInIndex = -1;
    private const int MAX_RANKING = 5;

    private ResultSceneManager _resultSceneManager;

    private void Awake()
    {
        if (nameEntryWindow != null) nameEntryWindow.SetActive(false);

    }

    private void Start()
    {
        _resultSceneManager = FindAnyObjectByType<ResultSceneManager>();


        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(OnSubmitName);
            nameInputField.onValueChanged.AddListener(OnValueChanged);
        }
    }

    public bool CheckAndShowEntryWindow(int score)
    {
        finalScore = score;

        if (FinalScoreText != null) FinalScoreText.text = $"SCORE: {finalScore}";

        rankInIndex = GetRankInIndex(finalScore);
        Debug.Log("rankInIndex: " + rankInIndex);

        if (rankInIndex >= 0)
        {
            nameEntryWindow.SetActive(true);
            nameInputField.ActivateInputField();
            return true;
        }

        return false;
    }

    private int GetRankInIndex(int score)
    {
        for (int i = 0; i < MAX_RANKING; i++)
        {
            if (score > PlayerPrefs.GetInt("RankScore_" + i, 0)) return i;
        }
        return -1;
    }

    private void OnValueChanged(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (!string.IsNullOrEmpty(Input.compositionString)) return;

        string filteredText = Get_HalfWidth(text, MAX_NameLength);

        if (text != filteredText)
        {
            nameInputField.text = filteredText;
            nameInputField.caretPosition = filteredText.Length;
        }
    }

    private string Get_HalfWidth(string text, int maxLength)
    {
        string result = "";

        foreach (char c in text)
        {
            if ((c >= 0x20 && c <= 0x7E) || (c >= 0xFF61 && c <= 0xFF9F))
            {
                result += c;
                if (result.Length >= maxLength) break;
            }
        }

        return result;
    }

    private void OnSubmitName(string nameText)
    {
        string playerName = Get_HalfWidth(nameText, MAX_NameLength);
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "NoName";

        SaveRanking(playerName, finalScore, rankInIndex);
        nameEntryWindow.SetActive(false);

        // 名前入力完了をResultSceneManagerに通知
        if (_resultSceneManager != null)
        {
            _resultSceneManager.OnNameInputComplete();
        }
        else
        {
            Debug.LogWarning("_resultSceneManagerがnullです");
        }
    }

    private void SaveRanking(string newName, int newScore, int newRankIndex)
    {
        for (int i = MAX_RANKING - 1; i > newRankIndex; i--)
        {
            PlayerPrefs.SetInt("RankScore_" + i, PlayerPrefs.GetInt("RankScore_" + (i - 1), 0));
            PlayerPrefs.SetString("RankName_" + i, PlayerPrefs.GetString("RankName_" + (i - 1), "---"));
        }

        PlayerPrefs.SetInt("RankScore_" + newRankIndex, newScore);
        PlayerPrefs.SetString("RankName_" + newRankIndex, newName);
        PlayerPrefs.Save();
    }
}