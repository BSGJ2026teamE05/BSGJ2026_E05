//// ---------------------------------------------------------
//// AlphaGameManager.cs
//// 作成日:  2026/4/10
//// 作成者:  坂田
//// 概要: ゲームマネジャー作成
//// ---------------------------------------------------------
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using TMPro;
//using System.Collections; // コルーチンを使用するために必要

//public class AlphaGameManager : MonoBehaviour
//{
//    // シングルトン：どのスクリプトからも AlphaGameManager.instance でアクセス可能
//    public static AlphaGameManager instance;

//    [SerializeField] private int score = 0;             // 現在のスコア
//    [SerializeField] private float timeLimit = 60f;     // 制限時間（秒）
//    [SerializeField] private float gameSceneDelay = 3f; // UI表示からシーン遷移までの待機時間（秒）

//    [SerializeField] private string playerName = "Player"; //ランキング用の名前

//    [SerializeField] private string clearSceneName = "SakataProtoResultScene"; // クリア時の遷移先シーン名
//    [SerializeField] private string overSceneName = "SakataProtoResultScene"; // ゲームオーバー時の遷移先シーン名

//    [SerializeField] private GameObject gameOverUI;  // ゲームオーバー時に表示するUI
//    [SerializeField] private GameObject gameClearUI; // ゲームクリア時に表示するUI

//    public TextMeshProUGUI scoreText; // スコアを表示するテキスト
//    public TextMeshProUGUI timerText; // 残り時間を表示するテキスト

//    private bool isGameActive = true; // ゲームが進行中かどうかのフラグ

//    // ─────────────────────────────────────────
//    // Awake：インスタンスの重複を防ぐシングルトン処理
//    // ─────────────────────────────────────────
//    void Awake()
//    {
//        if (instance == null) instance = this;  // 初回のみ自身をインスタンスとして登録
//        else Destroy(gameObject);               // 2つ目以降は破棄して重複を防ぐ
//    }

//    // ─────────────────────────────────────────
//    // Start：ゲーム開始時のUI初期表示
//    // ─────────────────────────────────────────
//    private void Start()
//    {
//        scoreText.text = "Score: 0";                                  // スコアの初期表示
//        timerText.text = "Time: " + Mathf.CeilToInt(timeLimit);      // タイマーの初期表示
//    }

//    // ─────────────────────────────────────────
//    // Update：毎フレームのタイマー更新処理
//    // ─────────────────────────────────────────
//    void Update()
//    {
//        if (!isGameActive) return; // ゲームが終了していたら処理しない

//        if (timeLimit > 0)
//        {
//            timeLimit -= Time.deltaTime;                                   // 経過時間を減算
//            timerText.text = "Time: " + Mathf.CeilToInt(timeLimit);      // タイマーUIを更新（小数点以上に切り上げ）
//        }
//        else
//        {
//            GameOver(); // 制限時間が0になったらゲームオーバー
//        }


//    }

//    // ─────────────────────────────────────────
//    // AddScore：スコアを加算してUIを更新する
//    // 引数 amount：加算するスコアの値
//    // ─────────────────────────────────────────
//    public void AddScore(int amount)
//    {
//        score += amount;                      // スコアに加算
//        scoreText.text = "Score: " + score;  // スコアUIを更新
//    }

//    // ─────────────────────────────────────────
//    // GameClear：ゲームクリア処理の開始
//    // ─────────────────────────────────────────
//    public void GameClear()
//    {
//        if (!isGameActive) return; // 二重呼び出しを防ぐ
//        isGameActive = false;
//        StartCoroutine(GameClearCoroutine()); // クリア演出コルーチンを開始
//    }

//    // ─────────────────────────────────────────
//    // GameOver：ゲームオーバー処理の開始
//    // ─────────────────────────────────────────
//    public void GameOver()
//    {
//        if (!isGameActive) return; // 二重呼び出しを防ぐ
//        isGameActive = false;
//        StartCoroutine(GameOverCoroutine()); // ゲームオーバー演出コルーチンを開始
//    }

//    // ─────────────────────────────────────────
//    // GameOverCoroutine：ゲームオーバーUIを表示して数秒後にシーン遷移
//    // ─────────────────────────────────────────
//    private IEnumerator GameOverCoroutine()
//    {
//        PlayerPrefs.SetInt("LastScore", score);
//        PlayerPrefs.SetString("LastName", playerName);
//        PlayerPrefs.Save();

//        gameOverUI.SetActive(true);                      // ゲームオーバーUIを表示
//        yield return new WaitForSeconds(gameSceneDelay); // 指定秒数待機

//        WingTransition transition = FindAnyObjectByType<WingTransition>();

//        transition.PlayTransitionIn(() =>
//        {
//            SceneManager.LoadScene(clearSceneName);
//        });
//        //SceneManager.LoadScene(overSceneName);           // ゲームオーバーシーンへ遷移
//    }

//    // ─────────────────────────────────────────
//    // GameClearCoroutine：ゲームクリアUIを表示して数秒後にシーン遷移
//    // ─────────────────────────────────────────
//    private IEnumerator GameClearCoroutine()
//    {
//        PlayerPrefs.SetInt("LastScore", score);
//        PlayerPrefs.SetString("LastName", playerName);
//        PlayerPrefs.Save();

//        gameClearUI.SetActive(true);                     // ゲームクリアUIを表示
//        yield return new WaitForSeconds(gameSceneDelay); // 指定秒数待機
//        WingTransition transition = FindAnyObjectByType<WingTransition>();

//        transition.PlayTransitionIn(() =>
//        {
//            SceneManager.LoadScene(clearSceneName);
//        });

//        //SceneManager.LoadScene(clearSceneName);          // クリアシーンへ遷移
//    }
//}

// ---------------------------------------------------------
// AlphaGameManager.cs
// 作成日:  2026/4/10
// 作成者:  坂田
// 概要: ゲームマネジャー作成
// ---------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class AlphaGameManager : MonoBehaviour
{
    public static AlphaGameManager instance;

    [SerializeField] private int score = 0;
    [SerializeField] private float gameSceneDelay = 3f;
    [SerializeField] private string playerName = "Player";

    [SerializeField] private string clearSceneName = "SakataProtoResultScene";
    [SerializeField] private string overSceneName = "SakataProtoResultScene";

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText; // 不要なら削除してOK

    // ─── 追加 ───────────────────────────────────
    [Header("天使ゲージ")]
    [SerializeField] private AngelGageUI _angelGageUI;

    [Header("ゲージ減少速度（/秒）"), Range(0f, 20f)]
    [SerializeField] private float _drainSpeed = 5f;
    // ────────────────────────────────────────────

    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        scoreText.text = "Score: 0";
        // timerText はゲージ値の表示に転用、不要なら削除
        UpdateGageText();
    }

    void Update()
    {
        if (!isGameActive) return;

        // タイムリミットの代わりにゲージを毎フレーム減少
        _angelGageUI.SubAngleGage(_drainSpeed * Time.deltaTime);

        // ゲージ表示を更新
        UpdateGageText();

        // ゲージが0になったらゲームオーバー
        if (_angelGageUI.CurrentGage <= 0f)
        {
            GameOver();
        }
    }

    // timerText をゲージ値表示に転用（不要なら削除してOK）
    private void UpdateGageText()
    {
        if (timerText != null)
            timerText.text = "Gage: " + Mathf.CeilToInt(_angelGageUI.CurrentGage);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    public void GameClear()
    {
        if (!isGameActive) return;
        isGameActive = false;
        StartCoroutine(GameClearCoroutine());
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetString("LastName", playerName);
        PlayerPrefs.Save();

        gameOverUI.SetActive(true);
        yield return new WaitForSeconds(gameSceneDelay);

        WingTransition transition = FindAnyObjectByType<WingTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(overSceneName);
        });
    }

    private IEnumerator GameClearCoroutine()
    {
        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetString("LastName", playerName);
        PlayerPrefs.Save();

        gameClearUI.SetActive(true);
        yield return new WaitForSeconds(gameSceneDelay);

        WingTransition transition = FindAnyObjectByType<WingTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(clearSceneName);
        });
    }
}