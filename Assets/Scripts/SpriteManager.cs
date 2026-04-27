// ---------------------------------------------------------
// SpriteManager.cs
// 作成日:  2026/04/25
// 作成者:  星野愛由
// 概要:　Player視点で、Enemyが常に正面を向く
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpriteManager : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // シーン内の「MainCamera」タグがついているカメラを自動で見つけて取得
        mainCamera = Camera.main;
    }

    // UpdateではなくLateUpdateを使うのがポイント！
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // オブジェクトの向き（前方）を、カメラの向き（前方）と全く同じにする
        transform.forward = mainCamera.transform.forward;
    }
}