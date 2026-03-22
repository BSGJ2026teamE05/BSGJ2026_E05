// ---------------------------------------------------------
// BossEnemy.cs
// 作成日:  2026/3/22
// 作成者:  佐々木
// 概要: ボス敵の情報スクリプト（HP制）
// ---------------------------------------------------------
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("この敵を倒した時のスコア")]
    [SerializeField] private int scoreValue = 1000;

    [Header("ボスHP")]
    [SerializeField] private int maxHP = 5;
    private int currentHP;

    [Header("死亡時に閉じる扉")]
    [SerializeField] private GameObject _door; // 死んだ時にfalseにする扉

    [Header("死亡演出")]
    [SerializeField] private GameObject bloodEffectPrefab;
    [SerializeField] private GameObject UIPrefab;

    [Header("被弾演出")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("演出位置調整")]
    [SerializeField] private Vector3 bloodOffset = Vector3.zero;
    [SerializeField] private Vector3 goodUIOffset = new Vector3(0, 2f, 0);

    private bool isDead = false;

    private void Start()
    {
        currentHP = maxHP;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemy();
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;
        currentHP -= damage;

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"[Boss] HP: {currentHP} / {maxHP}");

        if (currentHP <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        if (isDead) return;
        isDead = true;

        // 扉を非表示にする
        if (_door != null)
        {
            _door.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDead(scoreValue);
        }

        SpawnDeathEffects();
        Destroy(gameObject, 1.5f);
    }

    private void SpawnDeathEffects()
    {
        if (bloodEffectPrefab != null)
        {
            Vector3 bloodPos = transform.position + bloodOffset;
            Instantiate(bloodEffectPrefab, bloodPos, Quaternion.identity);
        }

        if (UIPrefab != null)
        {
            Vector3 goodPos = transform.position + goodUIOffset;
            Instantiate(UIPrefab, goodPos, Quaternion.identity);
        }
    }

    public float GetHPRatio() => (float)currentHP / maxHP;
}