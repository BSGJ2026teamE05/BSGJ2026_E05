// ---------------------------------------------------------
// TitleSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 概要: 両手同時に叩く動作でゲームシーンへ移動
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine.SceneManagement;


public class TitleSceneManager : MonoBehaviour
{
	// [SerializeField] private int _numId;
	[SerializeField] private HandLandmarkerRunner runner;

	[SerializeField] private string gameSceneName = "PrototypeScenetest";

	[Header("叩く判定")]
	[SerializeField, Range(0f, 1f)] private float centerLineY = 0.5f;
	[SerializeField] private float simultaneousWindow = 0.3f; // 両手の同時判定の時間
    [SerializeField] private float swingDownThreshold = 0.05f; // 振り下ろし速度の閾値


    private bool canLeftClap = true;
	private bool canRightClap = true;

	private float leftClapTime = -999f;
	private float rightClapTime = -999f;

    // 座標履歴
    private float _leftPrevY = 0f;
    private float _rightPrevY = 0f;



    private void Awake()
	{

	}

	private void Start() 
	{

	}
	
	private void Update() 
	{
        CheckClap(runner.isLeftHandDetected, runner.leftWristY, ref canLeftClap, ref leftClapTime);
        CheckClap(runner.isRightHandDetected, runner.rightWristY, ref canRightClap, ref rightClapTime);

        // 両手の叩いた時間差が許容範囲内なら発動
        if (leftClapTime > 0f && rightClapTime > 0f)
        {
            if (Mathf.Abs(leftClapTime - rightClapTime) <= simultaneousWindow)
            {
                leftClapTime = -999f;
                rightClapTime = -999f;
                SceneManager.LoadScene(gameSceneName);
            }
        }

    }

    private bool CheckClap(bool isDetected, float wristY, ref bool canClap,ref float clapTime)
	{
		if (!isDetected)
		{
			canClap = true;
			return false;
		}

		if (canClap && wristY >= centerLineY)
		{
			canClap = false;
			clapTime = Time.time;
		}

        if (!canClap && wristY < centerLineY)
        {
            canClap = true;
        }

		return false;
    }
}
