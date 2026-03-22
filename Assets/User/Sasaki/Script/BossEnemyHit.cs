// ---------------------------------------------------------
// BossEnemyHit.cs
// 作成日:  2026/3/22
// 作成者:  佐々木
// 概要: ボスと当たるとダメージ（HP制）
// ---------------------------------------------------------
using UnityEngine;

public class BossEnemyHit : MonoBehaviour
{
    public float knockbackForce = 10f;

    Rigidbody rb;
    BossEnemy bossEnemy;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bossEnemy = GetComponent<BossEnemy>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 吹っ飛ぶ方向
            Vector3 dir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(dir * knockbackForce + Vector3.up * 5f, ForceMode.Impulse);

            // ダメージ処理（Dead()ではなくTakeDamageを呼ぶ）
            if (bossEnemy != null)
            {
                bossEnemy.TakeDamage(1);
            }
        }
    }
}