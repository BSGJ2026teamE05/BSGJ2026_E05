// ---------------------------------------------------------
// EnemyHit.cs
// 作成日:  2026/3/8
// 作成者:  佐々木
// 概要:赤ちゃんと当たると死ぬ
// ---------------------------------------------------------

using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    public float knockbackForce = 10f;
    Rigidbody rb;

    void Start()
    {
        Enemycount.AddEnemy();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 吹っ飛ぶ方向
            Vector3 dir = (transform.position - collision.transform.position).normalized;

            rb.AddForce(dir * knockbackForce + Vector3.up * 5f, ForceMode.Impulse);

            // 少し後に消える
            Enemycount.EnemyDead();
            Destroy(gameObject, 1.5f);
        }
    }
}
