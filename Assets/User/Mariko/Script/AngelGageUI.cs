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
    [Header("ゲージの下限"), Range(0.0f, 180.0f)]
    [SerializeField] private float _gageMin = 100.0f;
    [Header("ゲージの上限"), Range(0.0f, 180.0f)]
    [SerializeField] private float _gageMax = 100.0f;

    [Header("テスト用 AngleGage パラメータ")]
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
        _gage = 100;
    }
	
	private void Update() 
	{
        if (Input.GetKey(KeyCode.T)) AddAngleGage(_addCount);
        if (Input.GetKey(KeyCode.Y)) SubAngleGage(_addCount);

    }

    // ゲージ減少
    public void SubAngleGage(float count)
    {
        _gage -= count;
        _gage = Mathf.Clamp(_gage, 0.0f, _gageMax);
        UpdateAngleGaze();
    }

    // ゲージ増加
    public void AddAngleGage(float count)
    {
        _gage += count;
        _gage = Mathf.Clamp(_gage, 0.0f, _gageMax);
        UpdateAngleGaze();
    }

    // ゲージUI表示の更新を行う
    public void UpdateAngleGaze()
    {
        // 値を限定
        var value = Mathf.Clamp(_gage, 0.0f, _gageMax);

        // fillamountの計算
        GageUI.fillAmount = (value + _gageMin) / (_overgageMax + _gageMax);
        OverGageUI.fillAmount = (value + _gageMin) / (_overgageMax + _gageMax);

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
