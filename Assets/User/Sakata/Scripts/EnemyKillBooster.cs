// ---------------------------------------------------------
// EnemyKillBooster.cs
// 作成日:  2026/3/22
// 作成者:  坂田
// 概要: 敵をいい定数倒すと一定時間プレイヤーを加速する
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyKillBooster : MonoBehaviour
{
    [Header("発動条件")]
    [Tooltip("何体倒すと加速するか")]
    [SerializeField] private int killsRequired = 5;

    [Header("加速設定")]
    [Tooltip("加速中の速度倍率 (例: 2.0 = 2倍速)")]
    [SerializeField] private float boostMultiplier = 2.0f;

    [Tooltip("加速が続く秒数")]
    [SerializeField] private float boostDuration = 5.0f;

    // 現在の撃破カウント
    private int _killCount = 0;

    // 加速タイマー (0 以下なら非加速)
    private float _boostTimer = 0f;

    // 加速中かどうか
    public bool IsBoosting => _boostTimer > 0f;

    // 参照先
    private HandTrackingParamStore _paramStore;

    private void Awake()
    {
        _paramStore = GetComponent<HandTrackingParamStore>();

    }

    private void Update()
    {
        if (_boostTimer <= 0f) return;

        _boostTimer -= Time.deltaTime;

        // タイマー切れ → 加速終了
        if (_boostTimer <= 0f)
        {
            _boostTimer = 0f;
            ApplyBoost(1.0f);   // 倍率を等倍に戻す
            Debug.Log("[EnemyKillBooster] 加速終了");
        }
    }

    /// <summary>
    /// 敵を倒したときに外部から呼ぶ。
    /// Enemy.cs 等に以下を記述して使う:
    ///   FindObjectOfType<EnemyKillBooster>()?.OnEnemyKilled();
    /// または敵が死亡時に直接参照して呼ぶ。
    /// </summary>
    public void OnEnemyKilled()
    {
        _killCount++;
        Debug.Log($"[EnemyKillBooster] 撃破数 : {_killCount} / {killsRequired}");

        if (_killCount >= killsRequired)
        {
            _killCount = 0;         // カウントをリセット
            ActivateBoost();
        }
    }

    // 加速を発動する
    private void ActivateBoost()
    {
        _boostTimer = boostDuration;
        ApplyBoost(boostMultiplier);
        Debug.Log($"[EnemyKillBooster] 加速発動！ x{boostMultiplier} / {boostDuration}秒");
    }

    // HandTrackingParamStore の SpeedMultiplier を上書きする
    private void ApplyBoost(float multiplier)
    {
        if (_paramStore != null)
        {
            _paramStore.SetSpeedBoost(multiplier);
        }
    }

    // 残り時間を UI 等に渡したいときに使う
    public float GetRemainingBoostTime() => Mathf.Max(0f, _boostTimer);
}
