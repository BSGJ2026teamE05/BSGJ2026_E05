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
using UnityEngine.UIElements;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

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
    [SerializeField] private Image fade;

    public TextMeshProUGUI scoreText;

    [SerializeField] private TextMeshProUGUI countdownText;
    private int countdownFrom = 3;
    [SerializeField] private float fadeTime;

    [Header("天使ゲージ")]
    [SerializeField] private AngelGageUI angelGageUI;

    [Header("ゲージ減少速度（/秒）"), Range(0f, 20f)]
    [SerializeField] private float drainSpeed = 5f;

    private bool isGameActive = false;
    public bool IsGameActive => isGameActive;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        scoreText.text = "Score: 0";

        CloudTransition cloudTransition = FindAnyObjectByType<CloudTransition>();

        if (cloudTransition != null)
        {
            cloudTransition.autoPlayOnStart = false;

            cloudTransition.PlayTransitionOut(() =>
            {
                StartCoroutine(CountdownCoroutine());
            });
        }
        else
        {
            StartCoroutine(CountdownCoroutine());
        }
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

    public void FallStage(System.Action onFadeComplete = null)
    {
        if (!isGameActive) return;
        StartCoroutine(FallStageCoroutine(onFadeComplete));

     }

    private IEnumerator CountdownCoroutine()
    {
        countdownText.gameObject.SetActive(true);

        for (int i = countdownFrom; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.text = "GO";

        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);

        isGameActive = true;

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

    private IEnumerator FallStageCoroutine(System.Action onFadeComplete = null)
    {
        isGameActive = false;

        // フェードイン（画面を隠す）
        yield return StartCoroutine(FadeCoroutine(0f, 1f));

        // 画面が隠れたタイミングでコールバック実行
        onFadeComplete?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // フェードアウト（画面を見せる）
        yield return StartCoroutine(FadeCoroutine(1f, 0f));

        isGameActive = true;
    }

    private IEnumerator FadeCoroutine(float from, float to)
    {
        float timer = 0f;
        Color color = fade.color;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, timer / fadeTime);
            fade.color = color;
            yield return null;
        }

        color.a = to;
        fade.color = color;
    }
}