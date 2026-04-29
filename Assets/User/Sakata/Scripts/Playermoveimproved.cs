using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("── 旋回パラメータ ──")]
    [Tooltip("目標角度へ向かう回転速度(度/秒)")]
    [SerializeField] private float rotateSpeed = 180f;
    [Tooltip("連続入力1回あたりの回転角度（コントローラー）")]
    public float tapRotateAngle = 15f;

    [Header("── ハイハイ判定 ──")]
    [Tooltip("「手を下ろした」と判定するY座標の閾値（0〜1正規化）")]
    [SerializeField, Range(0f, 1f)] private float stepDownThresholdY = 0.55f;
    [Tooltip("「手を引いた（持ち上げた）」と判定するYの閾値")]
    [SerializeField, Range(0f, 1f)] private float liftUpThresholdY = 0.45f;
    [Tooltip("ステップ後の最低リセット待機時間(s)（チャタリング防止）")]
    [SerializeField] private float stepCooldown = 0.08f;
    [Tooltip("手がロストした後に状態をリセットするまでの時間(s)")]
    [SerializeField] private float lostResetDelay = 0.3f;

    [Header("── コントローラー入力 ──")]
    [SerializeField] private HaiHaiAnime _haiHaiAnime;
    public Transform leftHand;
    public Transform rightHand;
    [Tooltip("床につく高さ")]
    public float floorY = 1f;
    [Tooltip("上に上がる高さ")]
    public float raisedY = 1.5f;
    [Tooltip("手が動く速さ")]
    public float lerpSpeed = 10f;
    [Tooltip("何秒離したら戻るか")]
    public float idleResetTime = 1.0f;
    [Tooltip("カーブ時の前進距離倍率（0〜1）")]
    [Range(0f, 1f)]
    public float curveStepRatio = 0.6f;

    [Header("── 加速ブースト ──")]
    [SerializeField] private int killCountToBoost = 5;
    [SerializeField] private float boostDuration = 5f;
    [SerializeField] private float boostMultiplier = 2f;

    // -------------------------------------------------------
    // 内部状態（MediaPipe）
    // -------------------------------------------------------

    private class HandState
    {
        public string Name;
        public bool IsLifted = false;
        public float StepCooldownTimer = 0f;
        public float LostTimer = 0f;
        public bool WasDetected = false;
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
    private string _lastStepHand = "None";

    // -------------------------------------------------------
    // 内部状態（コントローラー）
    // -------------------------------------------------------

    private InputSystemActions _input;
    private Vector3 _leftTargetPos;
    private Vector3 _rightTargetPos;
    private float _lastActiveTime;
    private bool _isNeutral;

    private enum LastHand { None, Left, Right }
    private LastHand _lastHand = LastHand.None;

    // -------------------------------------------------------
    // 共通内部状態
    // -------------------------------------------------------

    private Rigidbody _rb;
    private Vector3 _targetPosition;
    private float _targetRotationY = 0f; // 目標角度（速度ではなく角度で管理）

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

        // コントローラー初期化
        _input = new InputSystemActions();
        if (leftHand != null) _leftTargetPos = new Vector3(leftHand.localPosition.x, floorY, leftHand.localPosition.z);
        if (rightHand != null) _rightTargetPos = new Vector3(rightHand.localPosition.x, floorY, rightHand.localPosition.z);
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
        _targetRotationY = transform.eulerAngles.y; // 初期角度を引き継ぐ
    }

    private void OnEnable()
    {
        _input.Crawl.LeftHand.performed += ctx => OnPressLeft();
        _input.Crawl.RightHand.performed += ctx => OnPressRight();
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void Update()
    {
        if (!AlphaGameManager.instance.IsGameActive) return;

        // ── ブーストタイマー ──
        if (_boostTimer > 0f)
        {
            _boostTimer -= Time.deltaTime;
            if (_boostTimer <= 0f) _boostMult = 1f;
        }

        // ── コントローラー：手オブジェクトの補間アニメ ──
        if (leftHand != null) leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, _leftTargetPos, Time.deltaTime * lerpSpeed);
        if (rightHand != null) rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, _rightTargetPos, Time.deltaTime * lerpSpeed);

        // ── コントローラー：放置によるリセット判定 ──
        if (!_isNeutral && (Time.time - _lastActiveTime > idleResetTime))
            ResetToNeutral();

        // ── MediaPipe：ハンドトラッキング ──
        if (runner == null) return;

        UpdateHandState(_leftState, runner.isLeftHandDetected, runner.leftWristY);
        UpdateHandState(_rightState, runner.isRightHandDetected, runner.rightWristY);

        CheckStep(_leftState);
        CheckStep(_rightState);
        //CheckTurn();

        LogDebug();
    }

    private void FixedUpdate()
    {
        if (!AlphaGameManager.instance.IsGameActive) return;


        // 目標座標へ MoveTowards（Y軸は現在位置を維持）
        Vector3 target = new Vector3(_targetPosition.x, _rb.position.y, _targetPosition.z);
        Vector3 next = Vector3.MoveTowards(_rb.position, target, moveSpeed * _boostMult * Time.fixedDeltaTime);
        _rb.MovePosition(next);

        // 旋回：目標角度へ一定速度で近づく（暴走しない）
        float currentY = _rb.rotation.eulerAngles.y;
        float nextY = Mathf.MoveTowardsAngle(currentY, _targetRotationY, rotateSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(Quaternion.Euler(0f, nextY, 0f));
    }

    // -------------------------------------------------------
    // MediaPipe：手の状態更新
    // -------------------------------------------------------

    private void UpdateHandState(HandState state, bool isDetected, float rawY)
    {
        if (state.StepCooldownTimer > 0f)
            state.StepCooldownTimer -= Time.deltaTime;

        if (isDetected)
        {
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
    // MediaPipe：ステップ判定（ハイハイ前進）
    // -------------------------------------------------------

    private void CheckStep(HandState state)
    {
        if (state.StepCooldownTimer > 0f) return;

        float currentY = state.PrevY;

        if (!state.IsLifted && currentY < liftUpThresholdY)
            state.IsLifted = true;

        if (state.IsLifted && currentY > stepDownThresholdY)
        {
            state.IsLifted = false;
            state.StepCooldownTimer = stepCooldown;

            _haiHaiAnime?.OnInput();

            // 常に前進
            Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
            _targetPosition += forward * (stepDistance * _boostMult);

            if (_lastStepHand == state.Name)
            {
                // 同じ手が続く → 旋回（2回目以降はずっと旋回）
                float angle = state.Name == "Left" ? tapRotateAngle : -tapRotateAngle;
                _targetRotationY += angle;
                Debug.Log($"[Turn] {state.Name} +{angle}度 → 目標:{_targetRotationY:F1}");
            }
            else
            {
                // 違う手 → 旋回リセット（前進のみ）
                _lastStepHand = state.Name;
                Debug.Log($"[Step] {state.Name} +{stepDistance:F1}m");
            }
        }
    }


    // -------------------------------------------------------
    // コントローラー：ZL入力
    // -------------------------------------------------------

    private void OnPressLeft()
    {
        _haiHaiAnime?.OnInput();
        _isNeutral = false;
        _lastActiveTime = Time.time;

        if (_lastHand == LastHand.Left)
        {
            // 連続入力 → 左旋回＋カーブ前進
            _targetRotationY += tapRotateAngle;
            Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
            _targetPosition += forward * (stepDistance * curveStepRatio * _boostMult);
        }
        else
        {
            // 交互入力 → 前進
            if (leftHand != null) _leftTargetPos.y = floorY;
            if (rightHand != null) _rightTargetPos.y = raisedY;
            Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
            _targetPosition += forward * (stepDistance * _boostMult);
            _lastHand = LastHand.Left;
        }
    }

    // -------------------------------------------------------
    // コントローラー：ZR入力
    // -------------------------------------------------------

    private void OnPressRight()
    {
        _haiHaiAnime?.OnInput();
        _isNeutral = false;
        _lastActiveTime = Time.time;

        if (_lastHand == LastHand.Right)
        {
            // 連続入力 → 右旋回＋カーブ前進
            _targetRotationY -= tapRotateAngle;
            Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
            _targetPosition += forward * (stepDistance * curveStepRatio * _boostMult);
        }
        else
        {
            // 交互入力 → 前進
            if (rightHand != null) _rightTargetPos.y = floorY;
            if (leftHand != null) _leftTargetPos.y = raisedY;
            Vector3 forward = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y, 0f) * Vector3.forward;
            _targetPosition += forward * (stepDistance * _boostMult);
            _lastHand = LastHand.Right;
        }
    }

    // -------------------------------------------------------
    // コントローラー：放置リセット
    // -------------------------------------------------------

    private void ResetToNeutral()
    {
        if (leftHand != null) _leftTargetPos.y = floorY;
        if (rightHand != null) _rightTargetPos.y = floorY;
        _isNeutral = true;
        _lastHand = LastHand.None;
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

    // -------------------------------------------------------
    // 衝突判定
    // -------------------------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            FindFirstObjectByType<AlphaGameManager>().GameClear();
        }
    }

    // -------------------------------------------------------
    // デバッグ
    // -------------------------------------------------------

    private void LogDebug()
    {
        string l = runner.isLeftHandDetected
            ? $"L Y:{runner.leftWristY:F2} Vel:{_leftState.YVelocity:F3} Lifted:{_leftState.IsLifted}"
            : "L:Lost";
        string r = runner.isRightHandDetected
            ? $"R Y:{runner.rightWristY:F2} Vel:{_rightState.YVelocity:F3} Lifted:{_rightState.IsLifted}"
            : "R:Lost";
        Debug.Log($"{l} | {r} | TargetRot:{_targetRotationY:F1}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _targetPosition);
    }
}
