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

    [Header("天使ゲージ")]
    [SerializeField] private AngelGageUI angelGageUI;

    [Header("ゲージ減少速度（/秒）"), Range(0f, 20f)]
    [SerializeField] private float drainSpeed = 5f;

    private bool isGameActive = true;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        scoreText.text = "Score: 0";
    }

    void Update()
    {
        if (!isGameActive) return;

        // タイムリミットの代わりにゲージを毎フレーム減少
        angelGageUI.SubAngleGage(drainSpeed * Time.deltaTime);

        // ゲージが0になったらゲームオーバー
        if (angelGageUI.CurrentGage <= 0f)
        {
            GameOver();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    public void RecoverAngelGage(float amount)
    {
        angelGageUI.AddAngleGage(amount);
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