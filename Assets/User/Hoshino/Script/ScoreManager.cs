// ---------------------------------------------------------
// ScoreManager.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    // スコアを保持する変数（外部から読み取り専用）
    public int Score { get; private set; }

    // スコアを加算するメソッド
    public void AddScore(int amount)
    {
        Score += amount;
    }


    private void Awake()
	{

	}

	private void Start() 
	{
        
    }
	
	private void Update() 
	{

	}
}
