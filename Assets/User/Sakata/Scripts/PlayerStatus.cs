// ---------------------------------------------------------
// PlayerStatus.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStatus : MonoBehaviour
{
    [Header("ステータス")]
    public int maxHp = 100;
    private int currentHp;

    private PlayerMoveImproved _mover; // MoveStateの参照用

    private void Awake()
    {
        currentHp = maxHp;
        _mover = GetComponent<PlayerMoveImproved>();
    }

    // 敵スクリプトから呼ばれるダメージ処理
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"いてっ！ {damage} ダメージを受けた！ 残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            Debug.Log("ゲームオーバー...");
            AlphaGameManager.instance.GameOver();
        }
    }

    // 敵への攻撃
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyMovement enemy = collision.gameObject.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.TakeDamage(20);
                Debug.Log("Damage:20（体当たり）");
            }
        }
    }

}
