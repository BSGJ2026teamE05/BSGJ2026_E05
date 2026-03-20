// ---------------------------------------------------------
// GameManager.cs
// 作成日:  2026/3/17
// 作成者:  佐々木
// 概要: ゲームmanager
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("敵カウント")]
    public int totalEnemy = 0;
    public int deadEnemy = 0;

    [Header("スコア")]
    public int score = 0;

    [Header("UI")]
    [SerializeField] private Text enemyCountText;
    [SerializeField] private Text scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateEnemyCountUI();
        UpdateScoreUI();
    }

    public void AddEnemy()
    {
        totalEnemy++;
        UpdateEnemyCountUI();
    }

    public void EnemyDead(int addScore)
    {
        deadEnemy++;
        score += addScore;

        UpdateEnemyCountUI();
        UpdateScoreUI();
    }

    public void AddScore(int addScore)
    {
        score += addScore;
        UpdateScoreUI();
    }

    private void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = deadEnemy + " / " + totalEnemy;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
    }
}