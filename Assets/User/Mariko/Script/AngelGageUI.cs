// ---------------------------------------------------------
// AngelGageUI.cs
// 作成日:  2026/4/9
// 作成者:  Mariko Haruki
// 概要:天使ゲージUI
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;


public class AngelGageUI : MonoBehaviour
{
    // UI
    [SerializeField] private Image _frame;
	public Image Frame => _frame;

    [SerializeField] private Image _gazeUI;
    public Image GageUI => _gazeUI;

    [SerializeField] private Image _overgazeUI;
    public Image OverGageUI => _overgazeUI;

    [SerializeField] private Image _wingUI;
    public Image WingUI => _wingUI;

    // パラメータ
    [Header("ゲージの下限"), Range(0.0f, 1.0f)]
    [SerializeField] private float _gageUIMin = 0.0f;
    [Header("ゲージの振れ幅"), Range(0.0f, 1.0f)]
    [SerializeField] private float _gageUIRatio = 1.0f;

    [Header("テスト用 AngleGage パラメータ本体")]
    [SerializeField] private float _gage = 100.0f;

    [Header("テスト用 AngleGage ハイ天使状態しきい値")]
    [SerializeField] private float _overgageRatio = 100.0f;

    [Header("テスト用 AngleGage 最大値")]
    [SerializeField, Range(0.0f, 150.0f)] private float _overgageMax = 130.0f;

    [Header("ゲージの加算値")]
    [SerializeField, Range(0.0f, 2.0f)] private float _addCount = 0.5f;
     
    private void Awake()
	{

	}

	private void Start() 
	{
        InitializeGageUI();
	}

    // =========================================================================
    // 初期化関数
    // =========================================================================
    private void InitializeGageUI()
	{
    }
	
	private void Update() 
	{
        if (Keyboard.current.tKey.isPressed) AddAngleGage(_addCount);
        else if (Keyboard.current.yKey.isPressed) SubAngleGage(_addCount);
    }

    // ゲージ減少
    public void SubAngleGage(float count)
    {
        _gage -= count;
        _gage = Mathf.Clamp(_gage, 0.0f, _overgageMax);

        Debug.Log("減算中 gage: " + _gage);
        UpdateAngleGaze();
    }

    // ゲージ増加
    public void AddAngleGage(float count)
    {
        _gage += count;
        _gage = Mathf.Clamp(_gage, 0.0f, _overgageMax);

        Debug.Log("加算中 gage: " + _gage);
        UpdateAngleGaze();
    }

    // ゲージUI表示の更新を行う
    public void UpdateAngleGaze()
    {
        // _gage の範囲を 0 ～ _overgageMax に制限
        float value = Mathf.Clamp(_gage, 0.0f, _overgageMax);

        // 0 ～ 1 に正規化
        float normalizedValue = value / _overgageMax;

        // fillAmount を 0.3 ～ 0.8 のような任意範囲に変換
        float fill = _gageUIMin + normalizedValue * _gageUIRatio;

        // 念のため fillAmount の有効範囲に制限
        fill = Mathf.Clamp01(fill);

        GageUI.fillAmount = fill;
        OverGageUI.fillAmount = fill;

        UpdateViewAngelWing(value > _overgageRatio);
    }

    public void UpdateViewAngelWing(bool key)
    {
        WingUI.gameObject.SetActive(key);
    }

    async UniTask OnClick()
    {
        // await
    }
}
