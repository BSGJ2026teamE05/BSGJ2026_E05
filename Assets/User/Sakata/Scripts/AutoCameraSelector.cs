//// ---------------------------------------------------------
//// AutoCameraSelector.cs
//// 作成日:  2026/3/
//// 作成者:  
//// 概要:
//// ---------------------------------------------------------

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class AutoCameraSelector : MonoBehaviour
//{
//	// [SerializeField] private int _numId;


//	private void Awake()
//	{

//	}

//	private void Start() 
//	{

//	}

//	private void Update() 
//	{

//	}
//}
using System.Collections;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;

public class AutoCameraSelector : MonoBehaviour
{
    [Tooltip("使用するカメラのインデックス（0始まり）。2番目は 1。")]
    [SerializeField] private int targetCameraIndex = 1;

    [Tooltip("カメラ名の一部で指定する場合に入力（空欄ならindexで選択）")]
    [SerializeField] private string targetCameraNameContains = "";

    private IEnumerator Start()
    {
        // ImageSourceProvider に WebCamSource がセットされるまで待つ
        yield return new WaitUntil(() =>
            ImageSourceProvider.ImageSource != null);

        var imageSource = ImageSourceProvider.ImageSource;

        // デバイス一覧をログ出力
        var candidates = imageSource.sourceCandidateNames;
        if (candidates == null || candidates.Length == 0)
        {
            Debug.LogWarning("[AutoCameraSelector] カメラが見つかりません。");
            yield break;
        }

        for (int i = 0; i < candidates.Length; i++)
            Debug.Log($"[AutoCameraSelector] Camera [{i}]: {candidates[i]}");

        // 名前で検索
        int selectedIndex = -1;
        if (!string.IsNullOrEmpty(targetCameraNameContains))
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i].Contains(targetCameraNameContains))
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        // indexで選択（名前指定なし or 見つからなかった場合）
        if (selectedIndex == -1)
        {
            selectedIndex = (targetCameraIndex < candidates.Length)
                ? targetCameraIndex
                : 0;
        }

        Debug.Log($"[AutoCameraSelector] 選択: [{selectedIndex}] {candidates[selectedIndex]}");
        imageSource.SelectSource(selectedIndex);
    }
}