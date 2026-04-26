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
    [SerializeField] private GameObject nameEntryWindow;       // 名前入力ウィンドウの親オブジェクト
    [SerializeField] private TMP_InputField nameInputField;    // 名前を入力するインプットフィールド
    [SerializeField] private TextMeshProUGUI FinalScoreText;     // 最終スコアテキスト

    [Header("文字数制限")]
    private const int MAX_NameLength = 14;  // 半角基準での最大文字数（半角14、全角7）

    private int finalScore = 0;             // プレイヤーの最終スコア
    private int rankInIndex = -1;           // ランクインした順位（0〜4）
    private const int MAX_RANKING = 5;      // ランキングの最大保存数


    private void Start()
    {
        if (nameEntryWindow != null) nameEntryWindow.SetActive(false);

        /* ランキング表へ登録 */
        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(OnSubmitName);
            nameInputField.onValueChanged.AddListener(OnValueChanged); // リアルタイムの文字チェック用
        }
    }


    /* ====================================================================================
       処理：スコアを受け取り判定する
       ==================================================================================== */
    public void CheckAndShowEntryWindow(int score)
    {
        finalScore = score;

        if (FinalScoreText != null) FinalScoreText.text = $"SCORE: {finalScore}"; // 最終スコアを表示}

        rankInIndex = GetRankInIndex(finalScore);

        if (rankInIndex >= 0)
        {
            nameEntryWindow.SetActive(true);
            nameInputField.ActivateInputField(); // ウィンドウ表示時にすぐ入力できる状態にする
        }
    }

    private int GetRankInIndex(int score)
    {
        for (int i = 0; i < MAX_RANKING; i++)
        {
            if (score > PlayerPrefs.GetInt("RankScore_" + i, 0)) return i;
        }
        return -1;
    }

    /* ====================================================================================
       処理：入力した文字の「数」「全角かどうか」をチェック
       ==================================================================================== */
    private void OnValueChanged(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 日本語を変換している最中（IME入力中）はチェックを一時停止する
        if (!string.IsNullOrEmpty(Input.compositionString)) return;

        // 半角のみ抽出して14文字以内にカットした文字列を取得
        string filteredText = Get_HalfWidth(text, MAX_NameLength);

        // 全角が含まれていた、または14文字を超えていた場合は上書きする
        if (text != filteredText)
        {
            nameInputField.text = filteredText;
            nameInputField.caretPosition = filteredText.Length; // カーソルを末尾に戻す
        }
    }

    /* ====================================================================================
       処理：半角文字（ASCIIおよび半角カタカナ）のみを抽出し、制限内の文字列を返す
       ==================================================================================== */
    private string Get_HalfWidth(string text, int maxLength)
    {
        string result = "";

        foreach (char c in text)
        {
            // 半角英数字・記号(0x20〜0x7E) または 半角カタカナ(0xFF61〜0xFF9F) の場合のみ許可
            if ((c >= 0x20 && c <= 0x7E) || (c >= 0xFF61 && c <= 0xFF9F))
            {
                result += c;
                if (result.Length >= maxLength) break; // 制限文字数に達したらそこで終了
            }
        }

        return result;
    }

    /* ====================================================================================
       処理：名前入力が完了した（Enterキーが押された）ときに実行
       ==================================================================================== */
    private void OnSubmitName(string nameText) // nameText：InputFieldに入力されていた文字列
    {
        // プレイヤーが入力した文字を、ここで指定文字数（全角7文字分）にカットする！
        string playerName = Get_HalfWidth(nameText, MAX_NameLength);

        // 入力された名前を取得（空白の場合は「NoName」）

        if (string.IsNullOrWhiteSpace(playerName)) playerName = "NoName";
        SaveRanking(playerName, finalScore, rankInIndex); // ランキングデータを保存
        nameEntryWindow.SetActive(false);
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