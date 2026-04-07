using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveImproved : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector 設定
    // -------------------------------------------------------
    [SerializeField] private HandLandmarkerRunner runner;

    [Header("── 移動パラメータ ──")]
    [Tooltip("1ステップで前進する距離(m)")]
    [SerializeField] private float stepDistance = 3f;
    [Tooltip("目標位置へ向かう追従速度(m/s)（大きいほど遅延が減る）")]
    [SerializeField] private float moveSpeed = 20f;

    [Header("── ハイハイ判定 ──")]
    [Tooltip("「手を下ろした」と判定するY座標の閾値（0〜1正規化）")]
    [SerializeField, Range(0f, 1f)] private float stepDownThresholdY = 0.55f;

    [Tooltip("「手を引いた（持ち上げた）」と判定するYの閾値")]
    [SerializeField, Range(0f, 1f)] private float liftUpThresholdY = 0.45f;

    [Tooltip("ステップ後の最低リセット待機時間(s)（チャタリング防止）")]
    [SerializeField] private float stepCooldown = 0.08f;

    [Tooltip("手がロストした後に状態をリセットするまでの時間(s)")]
    [SerializeField] private float lostResetDelay = 0.3f;

    [Header("── 旋回パラメータ ──")]
    [Tooltip("旋回の最高速度(度/s)")]
    [SerializeField] private float maxTurnSpeed = 90f;
    [Tooltip("手を止めたあとの旋回停止係数（大きいほど素早く止まる）")]
    [SerializeField] private float turnStopDamping = 5f;
    [Tooltip("「片手だけ動いている」と判定するY速度差の閾値")]
    [SerializeField] private float turnDiffThreshold = 0.015f;
    [Tooltip("旋回の感度倍率")]
    [SerializeField] private float turnSensitivity = 120f;

    [Header("── 加速ブースト ──")]
    [SerializeField] private int killCountToBoost = 5;
    [SerializeField] private float boostDuration = 5f;
    [SerializeField] private float boostMultiplier = 2f;

    // -------------------------------------------------------
    // 内部状態
    // -------------------------------------------------------

    /// <summary>片手分の追跡状態を管理するクラス</summary>
    private class HandState
    {
        public string Name;

        // --- ステップ判定 ---
        public bool IsLifted = false;
        public float StepCooldownTimer = 0f;
        public float LostTimer = 0f;
        public bool WasDetected = false;

        // --- 速度推定（前フレームとの差のみ・キュー不要） ---
        public float PrevY = 0f;
        public float YVelocity = 0f;

        public HandState(string name) { Name = name; }

        public void Reset()
        {
            IsLifted = false;
            StepCooldownTimer = 0f;
            YVelocity = 0f;
        }
    }

    private HandState _leftState;
    private HandState _rightState;

    private Rigidbody _rb;
    private Vector3 _targetPosition;
    private float _currentTurnSpeed = 0f;
    private string _lastStepHand = "None";

    // ブースト
    private int _killCount = 0;
    private float _boostTimer = 0f;
    private float _boostMult = 1f;

    // -------------------------------------------------------
    // Unity ライフサイクル
    // -------------------------------------------------------

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

        LogDebug();
    }

    private void FixedUpdate()
    {
        // 目標座標へ MoveTowards（Y軸は現在位置を維持）
        Vector3 target = new Vector3(_targetPosition.x, _rb.position.y, _targetPosition.z);
        Vector3 next = Vector3.MoveTowards(_rb.position, target,
                             moveSpeed * _boostMult * Time.fixedDeltaTime);
        _rb.MovePosition(next);

        // 旋回
        float nextY = _rb.rotation.eulerAngles.y + _currentTurnSpeed * Time.fixedDeltaTime;
        _rb.MoveRotation(Quaternion.Euler(0f, nextY, 0f));

        // 手が止まったら旋回を減衰
        _currentTurnSpeed = Mathf.Lerp(_currentTurnSpeed, 0f,
                                Time.fixedDeltaTime * turnStopDamping);
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
            // Y速度を前フレームとの差だけで推定（キュー不要・遅延ゼロ）
            float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.016f;
            state.YVelocity = (rawY - state.PrevY) / dt;
            state.PrevY = rawY;

            state.LostTimer = 0f;
            state.WasDetected = true;
        }
        else
        {
            if (state.WasDetected)
            {
                state.LostTimer += Time.deltaTime;
                if (state.LostTimer >= lostResetDelay)
                {
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
        if (state.StepCooldownTimer > 0f) return;

        float currentY = state.PrevY;

        // フェーズ1：持ち上げ検出
        if (!state.IsLifted && currentY < liftUpThresholdY)
            state.IsLifted = true;

        // フェーズ2：踏み出し検出
        if (state.IsLifted && currentY > stepDownThresholdY)
        {
            state.IsLifted = false;
            state.StepCooldownTimer = stepCooldown;

            // 交互ステップのみ前進
            //if (_lastStepHand != state.Name)
            //{
                //旋回時も前進
                _lastStepHand = state.Name;
                Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f)
                                  * Vector3.forward;
                _targetPosition += forward * (stepDistance * _boostMult);
                Debug.Log($"[Step] {state.Name} +{stepDistance:F1}m Target:{_targetPosition}");
            //}
        }
    }

    // -------------------------------------------------------
    // 旋回判定
    // -------------------------------------------------------
    // 現実のハイハイ：
    //   左手を多く動かす → 右折
    //   右手を多く動かす → 左折

    private void CheckTurn()
    {
        float leftVel = runner.isLeftHandDetected ? Mathf.Abs(_leftState.YVelocity) : 0f;
        float rightVel = runner.isRightHandDetected ? Mathf.Abs(_rightState.YVelocity) : 0f;

        // 左手優勢(正) → 右折、右手優勢(負) → 左折
        float diff = leftVel - rightVel;

        if (Mathf.Abs(diff) < turnDiffThreshold)
        {
            // 差が閾値未満 → 旋回しない（FixedUpdate の StopDamping に任せる）
            return;
        }

        // Lerp を使わず直接セット → 追従遅延ゼロ
        _currentTurnSpeed = Mathf.Clamp(diff * turnSensitivity, -maxTurnSpeed, maxTurnSpeed);

        Debug.Log($"[Turn] diff:{diff:F3} speed:{_currentTurnSpeed:F1}");
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
        Debug.Log($"{l} | {r} | Turn:{_currentTurnSpeed:F1}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _targetPosition);
    }

    // -------------------------------------------------------
    // 外部 API
    // -------------------------------------------------------

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

    public void SetSpeedBoost(float multiplier)
    {
        _boostMult = Mathf.Max(0f, multiplier);
    }
}