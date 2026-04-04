// ---------------------------------------------------------
// ClapDetectorBase.cs
// 作成日:  2026/4/5
// 概要:「手を叩く」検出ロジックの共通基底クラス
// ---------------------------------------------------------

using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public abstract class ClapDetectorBase : MonoBehaviour
{
    // -------------------------------------------------------
    // 共通 Inspector パラメータ
    // -------------------------------------------------------
    [Header("── MediaPipe ──")]
    [SerializeField] protected HandLandmarkerRunner runner;

    [Header("── 叩き判定パラメータ（俯瞰撮影用） ──")]

    [Tooltip("「手を持ち上げた」と判定する Depth の上限\n" +
             "手をカメラ側へ上げると Depth が小さくなる\n" +
             "Console の [ClapDebug] で手を上げたときの値を確認して設定")]
    [SerializeField] protected float liftedDepthMax = 0.35f;

    [Tooltip("「叩いた（机に近い）」と判定する Depth の下限\n" +
             "机に手を叩きつけたときの値を [ClapDebug] で確認して設定")]
    [SerializeField] protected float clapDepthMin = 0.55f;

    [Tooltip("叩きと判定する最低 Depth 増加速度 (正規化/s)\n" +
             "大きいほど素早い動作のみ反応する")]
    [SerializeField] protected float velocityThreshold = 2.0f;

    [Tooltip("叩きと判定する最低 Depth 移動量 (正規化座標)\n" +
             "微小ノイズを除外するための最低値")]
    [SerializeField] protected float minMoveDistance = 0.03f;

    [Tooltip("1 回叩いた後の再判定クールダウン (s)")]
    [SerializeField] protected float clapCooldown = 0.4f;

    [Tooltip("手がロストしてから状態をリセットするまでの時間 (s)")]
    [SerializeField] protected float lostResetDelay = 0.3f;

    // -------------------------------------------------------
    // 内部クラス
    // -------------------------------------------------------
    protected class ClapHandState
    {
        // 「持ち上げ」フェーズを完了したか
        // true = 一度 liftedDepthMax より浅い位置に来た
        public bool IsLifted = false;
        public bool CanClap = true;
        public float PrevDepth = 1f;     // 初期値を大きめに（机の上想定）
        public float DepthVelocity = 0f;
        public float LastClapTime = -999f;
        public float LostTimer = 0f;
        public bool WasDetected = false;

        public void Reset(float depth)
        {
            IsLifted = false;
            CanClap = true;
            PrevDepth = depth;
            DepthVelocity = 0f;
            LostTimer = 0f;
        }
    }

    // -------------------------------------------------------
    // 共通メソッド
    // -------------------------------------------------------

    /// <summary>
    /// 俯瞰撮影用の叩き判定。
    /// 「持ち上げ → 叩きつけ」の 2 フェーズで検出するため
    /// 手を上げただけでは発火しない。
    /// </summary>
    protected bool TryClap(bool isDetected, float depth, ClapHandState state, string handName = "")
    {
        // ── ロスト処理 ──
        if (!isDetected)
        {
            if (state.WasDetected)
            {
                state.LostTimer += Time.deltaTime;
                if (state.LostTimer >= lostResetDelay)
                {
                    state.Reset(depth);
                    state.WasDetected = false;
                }
            }
            return false;
        }

        // ── 検出中 ──
        float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.016f;

        // Depth の速度（正 = 机に近づく、負 = カメラに近づく）
        state.DepthVelocity = (depth - state.PrevDepth) / dt;
        state.DepthVelocity = Mathf.Clamp(state.DepthVelocity, -30f, 30f);

        float move = depth - state.PrevDepth;

        state.LostTimer = 0f;
        state.WasDetected = true;

        // フェーズ1：持ち上げ検出
        // Depth が liftedDepthMax より小さい = カメラ側へ持ち上げた
        if (!state.IsLifted && depth < liftedDepthMax)
        {
            state.IsLifted = true;
            Debug.Log($"[ClapDebug] {handName} Lifted! Depth:{depth:F3}");
        }

        // クールダウン中は判定スキップ
        if (Time.time - state.LastClapTime < clapCooldown)
        {
            state.PrevDepth = depth;
            return false;
        }

        // フェーズ2：叩き検出
        //   ・持ち上げフェーズを経由した
        //   ・Depth が clapDepthMin を超えた（机に近い位置まで戻った）
        //   ・Depth の増加速度が正（机に向かっている）かつ閾値以上
        //   ・今フレームの移動量が最低量以上
        bool isClap =
            state.CanClap &&
            state.IsLifted &&
            depth >= clapDepthMin &&
            state.DepthVelocity > velocityThreshold &&
            move > minMoveDistance;

        if (isClap)
        {
            state.CanClap = false;
            state.IsLifted = false;
            state.LastClapTime = Time.time;
            state.PrevDepth = depth;
            Debug.Log($"[Clap!] {handName} Depth:{depth:F3} Vel:{state.DepthVelocity:F2}");
            return true;
        }

        // 持ち上げ位置に戻ったら再び有効
        if (!state.CanClap && depth < liftedDepthMax)
        {
            state.CanClap = true;
            state.IsLifted = true; // 持ち上げ状態も同時に回復
        }

        state.PrevDepth = depth;

        // 調整用ログ（値が安定したら削除可）
        Debug.Log($"[ClapDebug] {handName} " +
                  $"Depth:{depth:F3} Vel:{state.DepthVelocity:F+0.00;-0.00} " +
                  $"Lifted:{state.IsLifted} CanClap:{state.CanClap}");
        return false;
    }
}