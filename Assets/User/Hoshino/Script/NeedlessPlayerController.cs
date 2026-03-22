// ---------------------------------------------------------
// (needless)PlayerController.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理に必要
using UnityEngine.InputSystem;


public class NeedlessPlayerController : MonoBehaviour
{
    public ScoreManager scoreManager;

    // 外部から読み取れるようにpublic（またはプロパティ）にする
    public int Score { get; private set; }

    public void AddScore(int amount)
    {
        Score += amount;
        Debug.Log("Current Score: " + Score);
    }


    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {
        // Enterキーを押したらリザルトシーンを「追加」で読み込む
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            // LoadSceneMode.Additive がポイント！
            SceneManager.LoadScene("ResultScene", LoadSceneMode.Additive);

            // ゲーム側の動きを止めたい場合は時間を止める
            // Time.timeScale = 0f;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        // 敵（Enemyタグがついたオブジェクト）に触れたらスコア加算
        if (collision.gameObject.CompareTag("Enemy"))
        {
            scoreManager.AddScore(100);
            Destroy(collision.gameObject); // 敵を消す
        }
    }
}
