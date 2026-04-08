// ---------------------------------------------------------
// Enemy.cs
// 作成日:  2026/3/17
// 作成者:  佐々木
// 概要: 敵の情報スクリプト
// ---------------------------------------------------------

using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("この敵を倒した時のスコア")]
    [SerializeField] private int scoreValue = 100;

    [Header("死亡演出")]
    [SerializeField] private GameObject bloodEffectPrefab;   // 地面の血しぶき
    [SerializeField] private GameObject UIPrefab;        // 空中のGood演出

    [Header("演出位置調整")]
    [SerializeField] private Vector3 bloodOffset = Vector3.zero;
    [SerializeField] private Vector3 goodUIOffset = new Vector3(0, 2f, 0);

    private bool isDead = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemy();
        }
    }

    public void Dead()
    {
        if (isDead) return;
        isDead = true;

        // ★ゲージ回復
        if (PeakGaugeManager.Instance != null)
        {
            PeakGaugeManager.Instance.RecoverOnEnemyKill();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDead(scoreValue);
        }

        FindFirstObjectByType<EnemyKillBooster>()?.OnEnemyKilled();


        SpawnDeathEffects();
        Destroy(gameObject, 1.5f);


    }

    private void SpawnDeathEffects()
    {
        // 地面に血しぶき
        if (bloodEffectPrefab != null)
        {
            Vector3 bloodPos = transform.position + bloodOffset;
            Instantiate(bloodEffectPrefab, bloodPos, Quaternion.identity);
        }

        // 空中に演出
        if (UIPrefab != null)
        {
            Vector3 goodPos = transform.position + goodUIOffset;
            Instantiate(UIPrefab, goodPos, Quaternion.identity);
        }
    }
}