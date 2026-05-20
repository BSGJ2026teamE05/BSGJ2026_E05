// ---------------------------------------------------------
// VideoManager.cs
// 作成日:  2026/05/20
// 作成者:  星野愛由
// 概要:　タイトル画面で一定時間経過したら、映像が流れる処理
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
	[Header("設定項目")]
	[Tooltip("デモ映像が流れるまでの時間")]
	[SerializeField] private float Title_TimeOut = 10f;
    [Tooltip("ゲームパッドのスティック")]
	[SerializeField] private float stickDeadzone = 0.2f;

    [Header("連動させるオブジェクト")]
	[Tooltip("動画を表示するRawImage")]
	[SerializeField] private GameObject UI_Panel;
	[Tooltip("Video Player Component")]
	[SerializeField] private VideoPlayer videoPlayer;

	private float Title_Timer;
	private bool VideoPlaying;

	private void Start() 
	{
		ResetTimer();
	}
	
	private void Update() 
	{
		if (VideoPlaying)
		{
			if(AnyInputDetected())
			{
				StopVideo();
			}
			return;
		}

		if (AnyInputDetected())
		{
			Title_Timer = 0f;
		}
		else
		{
			Title_Timer += Time.deltaTime;					// "現在時刻を記録する"のを代入
			if (Title_Timer >= Title_TimeOut) StartVideo(); // Title_TimeOut分、経過したら映像スタート
		}
	}

	private void ResetTimer()
	{
		Title_Timer = 0f;
	}

	private void StartVideo()
	{
		VideoPlaying = true;
		UI_Panel.SetActive(true); // 映像の表示

		if (videoPlayer != null)
		{
			videoPlayer.Play();
		}
	}

	private void StopVideo()
	{
		VideoPlaying = false;
		UI_Panel.SetActive(false); // 映像の"非"表示

		if(videoPlayer != null)
		{
			videoPlayer.Stop();
		}

		ResetTimer();
	}

	private bool AnyInputDetected()
	{
        // 1. キーボードの入力チェック（何かキーが押されたか）
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        // 2. マウスの入力チェック
        if (Mouse.current != null)
        {
            // クリックされたか
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
            {
                return true;
            }

            // マウスが動いたか
            if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
            {
                return true;
            }
        }

        // 3. ゲームパッド（コントローラー）の入力チェック
        if (Gamepad.current != null)
        {
            // いずれかのボタンが「今フレーム押されたか」をループでチェック
            // allControlsの中から「ButtonControl（ボタン型の入力）」だけを安全に判別します
            var controls = Gamepad.current.allControls;
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i] is UnityEngine.InputSystem.Controls.ButtonControl button)
                {
                    if (button.wasPressedThisFrame)
                    {
                        return true;
                    }
                }
            }

            // スティックの傾きチェック（デッドゾーンを考慮して誤検知を防ぐ）
            if (Gamepad.current.leftStick.ReadValue().magnitude > stickDeadzone ||
                Gamepad.current.rightStick.ReadValue().magnitude > stickDeadzone)
            {
                return true;
            }
        }

        return false;
    }
}
