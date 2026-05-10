// ---------------------------------------------------------
// FpsDisplay.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 追加

public class FpsDisplay : MonoBehaviour
{
    // 変数
    int frameCount;
    float prevTime;
    float fps;

    [SerializeField] private TextMeshProUGUI fpsText; // 追加

    // 初期化処理
    void Start()
    {
        // 変数の初期化
        frameCount = 0;
        prevTime = 0.0f;
    }

    // 更新処理
    void Update()
    {
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;
        if (time >= 0.5f)
        {
            fps = frameCount / time;
            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;

            fpsText.text = "FPS: " + Mathf.Round(fps).ToString(); // 追加
        }
    }

    // OnGUIは不要なので削除
}