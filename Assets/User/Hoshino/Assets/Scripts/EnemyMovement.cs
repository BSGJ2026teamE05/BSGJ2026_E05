// ---------------------------------------------------------
// EnemyMovement.cs
// 作成日:  2026/3/19
// 作成者:  星野愛由
// 概要:　Enemy
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EnemyMovement : MonoBehaviour
{
    [Header("ステータス")]
    public int maxHp = 50;
    private int currentHp;

    [Header("移動設定")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private Transform playerTarget;

    [Header("ノックバック設定")]
    [SerializeField] private float knockbackDistance = 2f;    // 後ろに跳ね返る距離
    [SerializeField] private float knockbackDuration = 0.5f;  // 跳ね返るアクションにかかる時間

    public Rigidbody _rigidbody;
    public Animator animator;

    Vector3 movement;

    private bool isSquashed = false; // 潰れたかどうかを管理
    private bool isKnockedBack = false; // 追加：跳ね返り中かどうかを管理

    private void Awake()
    {

    }

    private void Start()
    {
        // ★追加：ゲーム開始時にHPを最大値にする
        currentHp = maxHp;

        // InspectorでplayerTargetをセットし忘れた場合の対策
        // "Player"タグがついているオブジェクトを自動的に探して設定します
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        // 潰されている、またはプレイヤーが見つからない場合は処理しない
        if (isSquashed || playerTarget == null) return;

        // プレイヤーに向かうベクトル（方向）を計算
        Vector3 direction = playerTarget.position - transform.position;

        // 高低差（Y軸）で敵が浮いたり沈んだりするのを防ぐため、Y軸の向きは0にする（平面移動の場合）
        direction.y = 0;

        // 向きを正規化（長さを1にする）してmovementに代入
        movement = direction.normalized;

        /* アニメーターへの反映 */
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.z);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
    void FixedUpdate()
    {
        // 潰されている、跳ね返り中、プレイヤーがいない場合は動かさない
        if (isSquashed || isKnockedBack || playerTarget == null) return;

        _rigidbody.MovePosition(_rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // 追加：プレイヤーにぶつかった時の処理
    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手がPlayerタグを持っていて、かつ現在跳ね返り中でない場合
        if (collision.gameObject.CompareTag("Player") && !isKnockedBack && !isSquashed)
        {
            // プレイヤーにダメージを与える処理があればここに書く
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(1);

            // 後ろに跳ね返る処理（コルーチン）を開始
            StartCoroutine(KnockbackRoutine());
        }
    }

    // 追加：後ろに跳ね返る動きをコントロールするコルーチン
    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true; // 追従を一時ストップ

        // アニメーションを待機状態にする（必要に応じて被ダメージアニメーション等に変更してください）
        animator.SetFloat("Speed", 0);

        // プレイヤーから敵へ向かうベクトル（＝後ろ方向）を計算
        Vector3 knockbackDir = (transform.position - playerTarget.position).normalized;
        knockbackDir.y = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + knockbackDir * knockbackDistance;

        float elapsedTime = 0f;

        // 指定した時間（knockbackDuration）をかけて後ろに下がる
        while (elapsedTime < knockbackDuration)
        {
            _rigidbody.MovePosition(Vector3.Lerp(startPos, targetPos, elapsedTime / knockbackDuration));
            elapsedTime += Time.fixedDeltaTime;

            // 次の物理演算フレームまで待つ
            yield return new WaitForFixedUpdate();
        }

        isKnockedBack = false; // 跳ね返り終了、再びUpdateで追従を再開する
    }

    public void Squash()
    {
        if (isSquashed) return;

        isSquashed = true;
        movement = Vector3.zero;

        animator.SetTrigger("Squash");

        Destroy(gameObject, 2.0f);
    }

    // === ★追加：プレイヤーから呼ばれるダメージ処理 ===
    public void TakeDamage(int damageAmount)
    {
        if (isSquashed) return; // すでに潰れていたらダメージを受けない

        currentHp -= damageAmount;
        Debug.Log($"敵に {damageAmount} のダメージ！ 敵の残りHP: {currentHp}");

        // HPが0以下になったら Squash を呼んで倒す
        if (currentHp <= 0)
        {
            Squash();
        }
    }
}