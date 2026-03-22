//// ---------------------------------------------------------
//// ResultManager.cs
//// 作成日:  2026/3/
//// 作成者:  
//// 概要:
//// ---------------------------------------------------------

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;


//public class ResultManager : MonoBehaviour
//{
//    public TextMeshProUGUI scoreText;


//    private void Awake()
//	{

//	}

//	private void Start() 
//	{
//        // UnityEngine.Object と明記して曖昧さを回避
//        ScoreManager manager = UnityEngine.Object.FindFirstObjectByType<ScoreManager>();

//        if (manager != null)
//        {
//            scoreText.text = "SCORE: " + manager.Score.ToString("N0");
//        }
//        else
//        {
//            Debug.LogWarning("ScoreManagerが見つかりませんでした！");
//        }
//    }
	
//	private void Update() 
//	{

//	}


//    public void OnRetryButton()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene("GameScene");
//    }
//}
