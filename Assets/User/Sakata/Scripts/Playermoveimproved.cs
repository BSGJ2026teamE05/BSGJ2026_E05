// ---------------------------------------------------------
// PlayerMoveImproved.cs
// 作成日:  2026/4/5
// 作成者:　坂田
// 概要:webカメラのハンドトラッキングによるプレイヤーの前進および回転
// ---------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveImproved : MonoBehaviour
{

    [SerializeField] private HandLandmarkerRunner runner;

    [Header("── 移動パラメータ ──")]
    [Tooltip("1ステップで前進する距離(m)")]
    [SerializeField] private float stepDistance = 3f;
    [Tooltip("目標位置へ向かう補間速度(m/s)（大きいほどキビキビ動く）")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("── ハイハイ判定 ──")]
    [Tooltip("「手を下ろした」と判定するY座標の上昇閾値（0〜1正規化）\n俯瞰カメラでは手を前に出すと画面上のYが上がる方向に設定する")]
    [SerializeField, Range(0f, 1f)] private float stepDownThresholdY = 0.55f;

    [Tooltip("「手を引いた（持ち上げた）」と判定するYの下降閾値")]
    [SerializeField, Range(0f, 1f)] private float liftUpThresholdY = 0.45f;

    [Tooltip("履歴バッファのフレーム数（多いほど滑らかだが遅延増）")]
    [SerializeField, Range(3, 30)] private int historyFrames = 8;

    [Tooltip("ステップ後の最低リセット待機時間(s)（チャタリング防止）")]
    [SerializeField] private float stepCooldown = 0.15f;

    [Tooltip("手がロストした後に状態をリセットするまでの時間(s)")]
    [SerializeField] private float lostResetDelay = 0.3f;

    [Header("── 旋回パラメータ ──")]
    [Tooltip("旋回速度(度/s)")]
    [SerializeField] private float maxTurnSpeed = 90f;
    [Tooltip("旋回速度の追従係数（大きいほど素早く目標速度に達する）")]
    [SerializeField] private float turnDamping = 12f;
    [Tooltip("手を止めたあとの旋回停止係数（大きいほど素早く止まる）")]
    [SerializeField] private float turnStopDamping = 5f;
    [Tooltip("「片手だけ動いている」と判定するY速度差の閾値")]
    [SerializeField] private float turnDiffThreshold = 0.015f;
    [Tooltip("旋回の感度倍率（大きくすると少しの動きで大きく旋回）")]
    [SerializeField] private float turnSensitivity = 120f;

    [Header("── 加速ブースト ──")]
    [SerializeField] private int killCountToBoost = 5;
    [SerializeField] private float boostDuration = 5f;
    [SerializeField] private float boostMultiplier = 2f;

    /// <summary>片手分の追跡状態を管理するクラス</summary>
    private class HandState
    {
        public string Name;

        // Y座標の履歴（正規化座標）
        public readonly Queue<float> YHistory = new Queue<float>();

        // --- ステップ判定 ---
        public bool IsLifted = false;   // 手が「引き上げ」状態か
        public float StepCooldownTimer = 0f;
        public float LostTimer = 0f;    // ロスト継続時間
        public bool WasDetected = false;

        // --- 速度推定 ---
        public float PrevY = 0f;
        public float YVelocity = 0f;   // 正規化Y/s

        public HandState(string name) { Name = name; }

        public void Reset()
        {
            IsLifted = false;
            StepCooldownTimer = 0f;
            YHistory.Clear();
            YVelocity = 0f;
        }
    }

    private HandState _leftState;
    private HandState _rightState;

    private Rigidbody _rb;
    private Vector3 _targetPosition;        // 次に向かう目標座標
    private float _currentTurnSpeed = 0f;

    // 交互ステップ管理
    private string _lastStepHand = "None";

    // ブースト
    private int _killCount = 0;
    private float _boostTimer = 0f;
    private float _boostMult = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _leftState = new HandState("Left");
        _rightState = new HandState("Right");
    }

    private void Start()
    {
        _rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _targetPosition = transform.position;
    }

    private void Update()
    {
        // ブーストタイマー
        if (_boostTimer > 0f)
        {
            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f) _boostMult = 1f;
        }

        if (runner == null) return;

        // 各手の状態を更新
        UpdateHandState(_leftState, runner.isLeftHandDetected, runner.leftWristY);
        UpdateHandState(_rightState, runner.isRightHandDetected, runner.rightWristY);

        // 前進・旋回判定
        CheckStep(_leftState);
        CheckStep(_rightState);
        CheckTurn();

        // デバッグログ
        LogDebug();
    }

    private void FixedUpdate()
    {
        // 目標座標へ向けて移動（Y軸は現在位置を維持）
        Vector3 target = new Vector3(_targetPosition.x, _rb.position.y, _targetPosition.z);
        Vector3 next = Vector3.MoveTowards(_rb.position, target, moveSpeed * _boostMult * Time.fixedDeltaTime);
        _rb.MovePosition(next);

        // 旋回
        float nextY = _rb.rotation.eulerAngles.y + _currentTurnSpeed * Time.fixedDeltaTime;
        _rb.MoveRotation(Quaternion.Euler(0f, nextY, 0f));

        // 回転を止める（CheckTurn が動いていないフレームだけ減衰が効く）
        _currentTurnSpeed = Mathf.Lerp(_currentTurnSpeed, 0f, Time.fixedDeltaTime * turnStopDamping);
    }

    // -------------------------------------------------------
    // 手の状態更新
    // -------------------------------------------------------

    private void UpdateHandState(HandState state, bool isDetected, float rawY)
    {
        // クールダウン更新
        if (state.StepCooldownTimer > 0f)
            state.StepCooldownTimer -= Time.deltaTime;

        if (isDetected)
        {
            state.LostTimer = 0f;

            // Y速度を推定（dt で正規化）
            float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.016f;
            state.YVelocity = (rawY - state.PrevY) / dt;
            state.PrevY = rawY;

            // 履歴に追加
            state.YHistory.Enqueue(rawY);
            while (state.YHistory.Count > historyFrames)
                state.YHistory.Dequeue();

            state.WasDetected = true;
        }
        else
        {
            // ロスト中のタイマー加算
            if (state.WasDetected)
            {
                state.LostTimer += Time.deltaTime;
                if (state.LostTimer >= lostResetDelay)
                {
                    // 一定時間ロストしたら状態リセット
                    state.Reset();
                    state.WasDetected = false;
                }
            }
        }
    }

    // -------------------------------------------------------
    // ステップ判定（ハイハイ前進）
    // -------------------------------------------------------

    private void CheckStep(HandState state)
    {
        if (state.YHistory.Count < 2) return;
        if (state.StepCooldownTimer > 0f) return;

        float currentY = state.PrevY; // 最新値

        // 手を引いた（上げた）→ IsLifted = true
        if (!state.IsLifted && currentY < liftUpThresholdY)
        {
            state.IsLifted = true;
        }

        // 引いた状態から前に出した（下ろした）→ ステップ
        if (state.IsLifted && currentY > stepDownThresholdY)
        {
            state.IsLifted = false;
            state.StepCooldownTimer = stepCooldown;

            // 交互に踏み出したら前進（目標座標をstepDistance分前方へ移動）
            if (_lastStepHand != state.Name)
            {
                _lastStepHand = state.Name;
                // 現在の向きを基準に目標座標を前進方向へ加算
                Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
                _targetPosition += forward * (stepDistance * _boostMult);
                Debug.Log($"[Step] {state.Name} -> Forward +{stepDistance:F1}m  Target:{_targetPosition}");
            }
        }
    }

    // -------------------------------------------------------
    // 旋回判定
    // -------------------------------------------------------
    // 「左手だけ大きく動く → 左折」「右手だけ大きく動く → 右折」
    // Y速度の絶対値の差で「どちらの手が主体的に動いているか」を判定する
    // -------------------------------------------------------

    private void CheckTurn()
    {
        bool leftOk = runner.isLeftHandDetected && _leftState.YHistory.Count >= 2;
        bool rightOk = runner.isRightHandDetected && _rightState.YHistory.Count >= 2;

        float leftVel = leftOk ? Mathf.Abs(_leftState.YVelocity) : 0f;
        float rightVel = rightOk ? Mathf.Abs(_rightState.YVelocity) : 0f;

        float diff = rightVel - leftVel; // 正 → 右手優勢、負 → 左手優勢

        if (Mathf.Abs(diff) < turnDiffThreshold)
        {
            // 差が閾値未満なら目標速度を 0 に向けて減衰（FixedUpdate の damping に任せる）
            return;
        }

        // 差の大きさに応じた目標旋回速度を直接セット（加算ではなく上書き）
        float targetTurnSpeed = Mathf.Clamp(diff * turnSensitivity, -maxTurnSpeed, maxTurnSpeed);

        // 急激な変化を避けるため Lerp で追従（turnDamping の逆数で応答速度を調整）
        _currentTurnSpeed = Mathf.Lerp(_currentTurnSpeed, targetTurnSpeed, Time.deltaTime * turnDamping);

        Debug.Log($"[Turn] diff:{diff:F3} target:{targetTurnSpeed:F1} current:{_currentTurnSpeed:F1}");
    }

    // -------------------------------------------------------
    // デバッグログ
    // -------------------------------------------------------

    private void LogDebug()
    {
        string l = runner.isLeftHandDetected
            ? $"L Y:{runner.leftWristY:F2} Vel:{_leftState.YVelocity:F3} Lifted:{_leftState.IsLifted}"
            : "L:Lost";
        string r = runner.isRightHandDetected
            ? $"R Y:{runner.rightWristY:F2} Vel:{_rightState.YVelocity:F3} Lifted:{_rightState.IsLifted}"
            : "R:Lost";
        Debug.Log($"{l} | {r} | Target:{_targetPosition} Turn:{_currentTurnSpeed:F2}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        // 目標座標までの線
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _targetPosition);
    }

    /// <summary>敵を倒したときに呼び出す</summary>
    public void OnEnemyKilled()
    {
        _killCount++;
        if (_killCount >= killCountToBoost)
        {
            _killCount = 0;
            _boostTimer = boostDuration;
            _boostMult = boostMultiplier;
            Debug.Log("[Boost] Start!");
        }
    }

    /// <summary>外部から加速倍率を直接セットする（1.0f = 等倍）</summary>
    public void SetSpeedBoost(float multiplier)
    {
        _boostMult = Mathf.Max(0f, multiplier);
    }
}