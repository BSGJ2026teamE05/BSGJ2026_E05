// ---------------------------------------------------------
// ScoreFloat.cs
// 作成日:  2026/04/27
// 作成者:  星野愛由
// 概要:　Enemy撃破時に、Enemyの上部に緑色で＋１５と出す
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScoreFloat : MonoBehaviour
{
    [Tooltip("上にフワッと上がるスピード")]
    public float floatSpeed = 2f;
    [Tooltip("何秒後に消えるか")]
    public float destroyTime = 1.0f;

    private void Start()
    {
        // 誕生してから destroyTime 秒後に自分自身を削除する
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        // 毎フレーム、少しずつ上に移動する
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}
