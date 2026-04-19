//// ---------------------------------------------------------
//// FluffyMove.cs
//// 作成日:  2026/4/19
//// 作成者:  坂田
//// 概要: オブジェクトがふわふわ揺れる
//// ---------------------------------------------------------

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public class FluffyMove : MonoBehaviour
//{
//    [SerializeField] private float _amplitude = 0.3f;  // 振れ幅
//    [SerializeField] private float _speed = 1.0f;  // 速さ

//    private float _originY;

//    private void Awake()
//    {
//        _originY = transform.position.y;
//    }

//    private void Update()
//    {
//        float y = _originY + Mathf.Sin(Time.time * _speed) * _amplitude;
//        transform.position = new Vector3(transform.position.x, y, transform.position.z);
//    }
//}
// ---------------------------------------------------------
// FluffyMove.cs
// 作成日:  2026/4/19
// 作成者:  坂田
// 概要: オブジェクトがふわふわ揺れる（通常オブジェクト・UIテキスト対応）
// ---------------------------------------------------------
using UnityEngine;

public class FluffyMove : MonoBehaviour
{
    [SerializeField] private float _amplitude = 0.3f;  // 振れ幅
    [SerializeField] private float _speed = 1.0f;      // 速さ

    private float _originY;
    private RectTransform _rectTransform;
    private bool _isUI;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _isUI = _rectTransform != null;
    }

    // AwakeからStartに変更（UIレイアウト確定後に初期値を取得するため）
    private void Start()
    {
        _originY = _isUI
            ? _rectTransform.anchoredPosition.y
            : transform.position.y;
    }

    private void Update()
    {
        float offsetY = Mathf.Sin(Time.time * _speed) * _amplitude;

        if (_isUI)
        {
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.y = _originY + offsetY;
            _rectTransform.anchoredPosition = pos;
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = _originY + offsetY;
            transform.position = pos;
        }
    }
}