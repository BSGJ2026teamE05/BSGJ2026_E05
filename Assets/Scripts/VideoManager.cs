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
	[Tooltip("暗転がかかる時間")]
	[SerializeField] private float fadeDuration = 0.5f;

    [Header("連動させるオブジェクト")]
	[Tooltip("動画を表示するRawImage")]
	[SerializeField] private GameObject UI_Panel;
	[Tooltip("Video Player Component")]
	[SerializeField] private VideoPlayer videoPlayer;
	[Tooltip("画面を暗転させるための画像")]
	[SerializeField] private CanvasGroup fadeCanvasGroup;

	private float Title_Timer;
	private bool VideoPlaying;
	private bool Transitioning;

	private void Start()
	{
		ResetTimer();

		if (fadeCanvasGroup != null) // 暗転用パネルを透明
		{
			fadeCanvasGroup.alpha = 0f;
			fadeCanvasGroup.gameObject.SetActive(false);
		}
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

        StartCoroutine(FadeAndPlayVideoRoutine());
    }

	private void StopVideo()
	{
		VideoPlaying = false;

        StartCoroutine(FadeAndStopVideoRoutine());
    }

	private bool AnyInputDetected()
	{
		// キーボードの入力チェック
		if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
		// マウスの入力チェック
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

		// コントローラーの入力チェック
		if (Gamepad.current != null)
		{
			// いずれかのボタンが「今フレーム押されたか」をループでチェック
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

	private IEnumerator Fade(float targetAlpha)
	{
		if (fadeCanvasGroup == null) yield break;

		fadeCanvasGroup.gameObject.SetActive(true);
		float startAlpha = fadeCanvasGroup.alpha;
		float time = 0f;

		while (time < fadeDuration)
		{
			time += Time.deltaTime; // 現在時刻を取得
			fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
			yield return null;
		}

		fadeCanvasGroup.alpha = targetAlpha;

		if (targetAlpha == 0f) fadeCanvasGroup.gameObject.SetActive(false);
	}

	private IEnumerator FadeAndPlayVideoRoutine()
	{
		Transitioning = true;

		yield return StartCoroutine(Fade(1f));		 // だんだん画面を暗くする（フェードアウト）

		UI_Panel.SetActive(true);                    // 画面が暗転したら、裏で動画を再生し始める
        if (videoPlayer != null) videoPlayer.Play();

		yield return StartCoroutine(Fade(0f));		 // だんだん画面を明るくする（フェードイン）

		Transitioning = false;
	}

    private IEnumerator FadeAndStopVideoRoutine()
    {
        Transitioning = true;

        yield return StartCoroutine(Fade(1f));		 // だんだん画面を暗くする（フェードアウト）

        UI_Panel.SetActive(false);
        if (videoPlayer != null) videoPlayer.Stop(); // 画面が暗転したら、動画を止めて非表示に
        ResetTimer();

        yield return StartCoroutine(Fade(0f));		 // // だんだん画面を明るくして、タイトル画面を表示

        Transitioning = false;
    }
}
