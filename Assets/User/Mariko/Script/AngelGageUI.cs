// ---------------------------------------------------------
// AngelGageUI.cs
// 作成日:  2026/4/9
// 作成者:  Mariko Haruki
// 概要:天使ゲージUI
// ---------------------------------------------------------

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

    [Header("ハイ天使状態しきい値（通常ゲージ最大値）")]
    [SerializeField] private float _overgageRatio = 100.0f;

    [Header("ゲージの最大値")]
    [SerializeField, Range(0.0f, 150.0f)] private float _overgageMax = 130.0f;

    [Header("ゲージの加算値")]
    [SerializeField, Range(0.0f, 2.0f)] private float _addCount = 0.5f;

    public float CurrentGage => _gage;

    private void Start()
    {
        InitializeGageUI();
    }

    // =========================================================================
    // 初期化関数
    // =========================================================================
    private void InitializeGageUI()
    {
        UpdateAngleGaze();
    }

    private void Update()
    {
        //if (Keyboard.current.tKey.isPressed)
        //{
        //    AddAngleGage(_addCount);
        //}
        //else if (Keyboard.current.yKey.isPressed)
        //{
        //    SubAngleGage(_addCount);
        //}
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
        // 現在値を 0 ～ 130 に制限
        float value = Mathf.Clamp(_gage, 0.0f, _overgageMax);

        // GageUI は 100 を超えたら更新を止める
        float gageStopValue = Mathf.Clamp(value, 0.0f, _overgageRatio);

        // ただし fillAmount への変換は 0 ～ 130 基準で行う
        float gageNormalized = (_overgageMax <= 0.0f) ? 0.0f : gageStopValue / _overgageMax;
        float overNormalized = (_overgageMax <= 0.0f) ? 0.0f : value / _overgageMax;

        float gageFill = _gageUIMin + gageNormalized * _gageUIRatio;
        float overFill = _gageUIMin + overNormalized * _gageUIRatio;

        gageFill = Mathf.Clamp01(gageFill);
        overFill = Mathf.Clamp01(overFill);

        GageUI.fillAmount = gageFill;
        OverGageUI.fillAmount = overFill;

        UpdateViewAngelWing(value > _overgageRatio);
    }

    public void UpdateViewAngelWing(bool key)
    {
        WingUI.gameObject.SetActive(key);
    }

    async UniTask OnClick()
    {
        await UniTask.Yield();
    }
}