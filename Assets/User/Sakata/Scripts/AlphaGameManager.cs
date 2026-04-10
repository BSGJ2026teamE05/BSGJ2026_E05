// ---------------------------------------------------------
// AlphaGameManager.cs
// 作成日:  2026/4/10
// 作成者:  坂田
// 概要: ゲームマネジャー作成
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class AlphaGameManager : MonoBehaviour
{
	// [SerializeField] private int _numId;
	public static AlphaGameManager instance; // どこからでもアクセスできる

	public int score = 0;
	public float timeLimit = 60f;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI timerText;


	void Awake() => instance = this;

	private void Start() 
	{

	}
	void Update() 
	{
		if (timeLimit > 0)
		{
			timeLimit -= Time.deltaTime;
			timerText.text = "Time: " + Mathf.CeilToInt(timeLimit);
		}
		else
		{
			GameOver();
		}
	}

	public void AddScore(int amount)
	{
		score += amount;
		scoreText.text = "Score:" + score;
	}

	public void GameClear() => SceneManager.LoadScene("SakataProtoResultScene");
	public void GameOver() => SceneManager.LoadScene("SakataProtoResultScene");
}
