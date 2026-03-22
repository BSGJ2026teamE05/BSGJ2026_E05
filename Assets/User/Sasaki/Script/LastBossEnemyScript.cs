// ---------------------------------------------------------
// LastBossEnemyScript.cs
// 作成日:  2026/3/22
// 作成者:  佐々木
// 概要: ラスボスの行動スクリプト（デデデ大王風）
//       BossEnemyScriptの全動作 ＋ ラスボス専用動作を追加
//       プレイヤーが近づくまでIdle待機
// ---------------------------------------------------------
using System.Collections;
using UnityEngine;

public class LastBossEnemyScript : BaseEnemyScript
{
    // -------------------------------------------------------
    // ステート定義
    // -------------------------------------------------------
    private enum BossState
    {
        Idle,       // プレイヤーが近づくまで待機
        Watch,      // 様子見（横移動あり）
        Walk,       // もっさり歩き
        Charge,     // 突進
        Recover,    // 突進後よろけ
        Strafe,     // 横移動
        BackAway,   // 距離を取る
        JumpMove,   // ジャンプして移動
        Float,      // 少し浮いて様子見
        Stamp,      // ジャンプスタンプ攻撃
        Landing,    // 着地硬直
    }

    // -------------------------------------------------------
    // パラメータ
    // -------------------------------------------------------
    [Header("起動距離")]
    [SerializeField] private float _activateDistance = 10f;    // この距離に入ったら動き出す

    [Header("様子見")]
    [SerializeField] private float _watchTimeMin = 1.5f;
    [SerializeField] private float _watchTimeMax = 3.0f;

    [Header("横移動（Strafe）")]
    [SerializeField] private float _strafeSpeed = 1.2f;
    [SerializeField] private float _strafeTimeMin = 1.0f;
    [SerializeField] private float _strafeTimeMax = 2.5f;

    [Header("もっさり歩き")]
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _walkTimeMin = 1.0f;
    [SerializeField] private float _walkTimeMax = 2.5f;

    [Header("突進")]
    [SerializeField] private float _chargeSpeed = 12f;
    [SerializeField] private float _chargeDuration = 0.6f;
    [SerializeField] private float _chargeInterval = 5f;

    [Header("よろけ")]
    [SerializeField] private float _recoverTime = 1.0f;

    [Header("距離を取る（BackAway）")]
    [SerializeField] private float _backAwayDistance = 4.0f;
    [SerializeField] private float _backAwaySpeed = 2.0f;
    [SerializeField] private float _backAwayTime = 1.5f;
    [SerializeField] private float _backAwayChance = 0.5f;

    [Header("回転補間")]
    [SerializeField] private float _rotateSlerp = 3f;

    [Header("ジャンプ移動")]
    [SerializeField] private float _jumpMoveSpeed = 5f;
    [SerializeField] private float _jumpRiseSpeed = 6f;
    [SerializeField] private float _jumpRiseHeight = 3f;
    [SerializeField] private float _jumpMoveDuration = 0.8f;
    [SerializeField] private float _jumpInterval = 6f;

    [Header("浮遊（Float）")]
    [SerializeField] private float _floatHeight = 3.5f;
    [SerializeField] private float _floatRiseSpeed = 2f;
    [SerializeField] private float _floatWatchTime = 2.5f;
    [SerializeField] private float _floatInterval = 8f;

    [Header("スタンプ攻撃")]
    [SerializeField] private float _stampRiseHeight = 5f;
    [SerializeField] private float _stampRiseSpeed = 6f;
    [SerializeField] private float _stampAimDuration = 0.8f;
    [SerializeField] private float _stampFallSpeed = 22f;
    [SerializeField] private float _stampDamageRadius = 3f;
    [SerializeField] private float _stampInterval = 10f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("着地硬直")]
    [SerializeField] private float _landingTime = 0.8f;

    // -------------------------------------------------------
    // 内部変数
    // -------------------------------------------------------
    private BossState _currentState = BossState.Idle;
    private float _lastChargeTime = -999f;
    private float _lastJumpTime = -999f;
    private float _lastFloatTime = -999f;
    private float _lastStampTime = -999f;
    private Vector3 _chargeDirection;
    private float _strafeSideDir = 1f;
    private bool _checkedBackAway = false;
    private float _groundY;
    private bool _isAerialAction = false;

    // -------------------------------------------------------
    // 初期化
    // -------------------------------------------------------
    private void OnEnable()
    {
        _groundY = transform.position.y;
        _currentState = BossState.Idle;
        StartCoroutine(BossBrain());
    }

    // -------------------------------------------------------
    // メインのステートマシン
    // -------------------------------------------------------
    private IEnumerator BossBrain()
    {
        while (true)
        {
            switch (_currentState)
            {
                case BossState.Idle: yield return StartCoroutine(StateIdle()); break;
                case BossState.Watch: yield return StartCoroutine(StateWatch()); break;
                case BossState.Walk: yield return StartCoroutine(StateWalk()); break;
                case BossState.Charge: yield return StartCoroutine(StateCharge()); break;
                case BossState.Recover: yield return StartCoroutine(StateRecover()); break;
                case BossState.Strafe: yield return StartCoroutine(StateStrafe()); break;
                case BossState.BackAway: yield return StartCoroutine(StateBackAway()); break;
                case BossState.JumpMove: yield return StartCoroutine(StateJumpMove()); break;
                case BossState.Float: yield return StartCoroutine(StateFloat()); break;
                case BossState.Stamp: yield return StartCoroutine(StateStamp()); break;
                case BossState.Landing: yield return StartCoroutine(StateLanding()); break;
            }
        }
    }

    // -------------------------------------------------------
    // Idle：プレイヤーが近づくまで待機
    // -------------------------------------------------------
    private IEnumerator StateIdle()
    {
        while (true)
        {
            if (_target == null)
            {
                _target = GameObject.FindGameObjectWithTag("Player");
                yield return null;
                continue;
            }

            float dist = Vector3.Distance(transform.position, _target.transform.position);
            if (dist <= _activateDistance)
            {
                Debug.Log("[LastBoss] プレイヤーを検知！起動します");
                _currentState = BossState.Watch;
                yield break;
            }

            yield return null;
        }
    }

    // -------------------------------------------------------
    // 様子見（横移動しながらプレイヤーを見る）
    // -------------------------------------------------------
    private IEnumerator StateWatch()
    {
        float waitTime = Random.Range(_watchTimeMin, _watchTimeMax);
        float elapsed = 0f;
        float side = Random.value < 0.5f ? 1f : -1f;

        while (elapsed < waitTime)
        {
            if (IsPlayerTooClose() && !_checkedBackAway)
            {
                _checkedBackAway = true;
                if (Random.value < _backAwayChance) { _currentState = BossState.BackAway; yield break; }
            }
            if (!IsPlayerTooClose()) _checkedBackAway = false;
            _strafeSideDir = side;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _currentState = DecideNextState();
    }

    // -------------------------------------------------------
    // 横移動（Strafe）
    // -------------------------------------------------------
    private IEnumerator StateStrafe()
    {
        float strafeTime = Random.Range(_strafeTimeMin, _strafeTimeMax);
        float elapsed = 0f;
        float side = Random.value < 0.5f ? 1f : -1f;

        while (elapsed < strafeTime)
        {
            if (IsPlayerTooClose() && !_checkedBackAway)
            {
                _checkedBackAway = true;
                if (Random.value < _backAwayChance) { _currentState = BossState.BackAway; yield break; }
            }
            if (!IsPlayerTooClose()) _checkedBackAway = false;
            _strafeSideDir = side;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _currentState = DecideNextState();
    }

    // -------------------------------------------------------
    // 距離を取る
    // -------------------------------------------------------
    private IEnumerator StateBackAway()
    {
        float elapsed = 0f;
        while (elapsed < _backAwayTime) { elapsed += Time.deltaTime; yield return null; }
        _currentState = BossState.Watch;
    }

    // -------------------------------------------------------
    // もっさり歩き
    // -------------------------------------------------------
    private IEnumerator StateWalk()
    {
        float walkTime = Random.Range(_walkTimeMin, _walkTimeMax);
        float elapsed = 0f;
        while (elapsed < walkTime) { elapsed += Time.deltaTime; yield return null; }
        _currentState = DecideNextState();
    }

    // -------------------------------------------------------
    // 突進
    // -------------------------------------------------------
    private IEnumerator StateCharge()
    {
        _lastChargeTime = Time.time;
        if (_target != null)
        {
            _chargeDirection = (_target.transform.position - transform.position);
            _chargeDirection.y = 0;
            _chargeDirection.Normalize();
        }
        float elapsed = 0f;
        while (elapsed < _chargeDuration) { elapsed += Time.deltaTime; yield return null; }
        _currentState = BossState.Recover;
    }

    // -------------------------------------------------------
    // よろけ
    // -------------------------------------------------------
    private IEnumerator StateRecover()
    {
        yield return new WaitForSeconds(_recoverTime);
        _currentState = BossState.Watch;
    }

    // -------------------------------------------------------
    // ジャンプ移動
    // -------------------------------------------------------
    private IEnumerator StateJumpMove()
    {
        _lastJumpTime = Time.time;
        _isAerialAction = true;
        _rigidbody.useGravity = false;

        // 上昇
        float targetY = _groundY + _jumpRiseHeight;
        while (transform.position.y < targetY)
        {
            _rigidbody.MovePosition(_rigidbody.position + Vector3.up * _jumpRiseSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        // 空中水平移動
        float elapsed = 0f;
        while (elapsed < _jumpMoveDuration)
        {
            if (_target != null)
            {
                Vector3 dir = (_target.transform.position - transform.position);
                dir.y = 0;
                if (dir.magnitude > 0.1f)
                {
                    dir.Normalize();
                    FaceDirection(dir);
                    _rigidbody.MovePosition(_rigidbody.position + dir * _jumpMoveSpeed * Time.fixedDeltaTime);
                }
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 落下
        _rigidbody.useGravity = true;
        while (transform.position.y > _groundY + 0.15f)
        {
            yield return new WaitForFixedUpdate();
        }

        _isAerialAction = false;
        _currentState = BossState.Landing;
    }

    // -------------------------------------------------------
    // 浮遊
    // -------------------------------------------------------
    private IEnumerator StateFloat()
    {
        _lastFloatTime = Time.time;
        _isAerialAction = true;
        _rigidbody.useGravity = false;

        // 上昇
        float targetY = _groundY + _floatHeight;
        while (transform.position.y < targetY)
        {
            _rigidbody.MovePosition(_rigidbody.position + Vector3.up * _floatRiseSpeed * Time.fixedDeltaTime);
            LookAtTarget();
            yield return new WaitForFixedUpdate();
        }

        // ホバリング
        float elapsed = 0f;
        while (elapsed < _floatWatchTime)
        {
            LookAtTarget();
            float hover = Mathf.Sin(Time.time * 2f) * 0.02f;
            _rigidbody.MovePosition(_rigidbody.position + Vector3.up * hover);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 落下
        _rigidbody.useGravity = true;
        while (transform.position.y > _groundY + 0.15f)
        {
            yield return new WaitForFixedUpdate();
        }

        _isAerialAction = false;
        _currentState = BossState.Landing;
    }

    // -------------------------------------------------------
    // スタンプ攻撃
    // -------------------------------------------------------
    private IEnumerator StateStamp()
    {
        _lastStampTime = Time.time;
        _isAerialAction = true;
        _rigidbody.useGravity = false;

        // スタンプ先をプレイヤー位置でロック
        Vector3 stampTarget = _target != null
            ? new Vector3(_target.transform.position.x, _groundY, _target.transform.position.z)
            : transform.position;

        // 上昇しながらスタンプ先の真上へ
        float targetY = _groundY + _stampRiseHeight;
        while (transform.position.y < targetY)
        {
            _rigidbody.MovePosition(_rigidbody.position + Vector3.up * _stampRiseSpeed * Time.fixedDeltaTime);
            Vector3 hDir = new Vector3(stampTarget.x - transform.position.x, 0, stampTarget.z - transform.position.z);
            if (hDir.magnitude > 0.2f)
            {
                hDir.Normalize();
                FaceDirection(hDir);
                _rigidbody.MovePosition(_rigidbody.position + hDir * _walkSpeed * Time.fixedDeltaTime);
            }
            yield return new WaitForFixedUpdate();
        }

        // ため
        yield return new WaitForSeconds(_stampAimDuration);

        // 高速落下
        while (transform.position.y > _groundY + 0.15f)
        {
            _rigidbody.MovePosition(_rigidbody.position + Vector3.down * _stampFallSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        // 着地ダメージ
        OnStampLanded();

        _rigidbody.useGravity = true;
        _isAerialAction = false;
        _currentState = BossState.Landing;
    }

    /// <summary>スタンプ着地ダメージ判定</summary>
    private void OnStampLanded()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _stampDamageRadius, _playerLayer);
        foreach (var hit in hits)
        {
            hit.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            Debug.Log("[LastBoss] スタンプヒット！");
        }
    }

    // -------------------------------------------------------
    // 着地硬直
    // -------------------------------------------------------
    private IEnumerator StateLanding()
    {
        yield return new WaitForSeconds(_landingTime);
        _currentState = BossState.Watch;
    }

    // -------------------------------------------------------
    // 次のステートを決める
    // -------------------------------------------------------
    private BossState DecideNextState()
    {
        bool canCharge = (Time.time - _lastChargeTime) >= _chargeInterval;
        bool canJump = (Time.time - _lastJumpTime) >= _jumpInterval;
        bool canFloat = (Time.time - _lastFloatTime) >= _floatInterval;
        bool canStamp = (Time.time - _lastStampTime) >= _stampInterval;

        if (canStamp && Random.value < 0.30f) return BossState.Stamp;
        if (canCharge && Random.value < 0.35f) return BossState.Charge;
        if (canFloat && Random.value < 0.25f) return BossState.Float;
        if (canJump && Random.value < 0.25f) return BossState.JumpMove;

        float r = Random.value;
        if (r < 0.33f) return BossState.Watch;
        if (r < 0.66f) return BossState.Strafe;
        return BossState.Walk;
    }

    // -------------------------------------------------------
    // Move()（FixedUpdateから呼ばれる）
    // -------------------------------------------------------
    protected override void Move()
    {
        if (_target == null || _rigidbody == null) return;
        if (_currentState == BossState.Idle) return; // Idle中は何もしない

        // 空中アクション中はコルーチンに任せる
        if (_isAerialAction)
        {
            LookAtTarget();
            return;
        }

        switch (_currentState)
        {
            case BossState.Watch:
            case BossState.Strafe:
                StrafeMove();
                break;
            case BossState.Walk:
                MoveTowardTarget(_walkSpeed);
                break;
            case BossState.Charge:
                _rigidbody.MovePosition(_rigidbody.position + _chargeDirection * _chargeSpeed * Time.fixedDeltaTime);
                FaceDirection(_chargeDirection);
                break;
            case BossState.BackAway:
                BackAwayMove();
                break;
            case BossState.Recover:
            case BossState.Landing:
                LookAtTarget();
                break;
        }
    }

    // -------------------------------------------------------
    // ヘルパー
    // -------------------------------------------------------
    private bool IsPlayerTooClose()
    {
        if (_target == null) return false;
        return Vector3.Distance(transform.position, _target.transform.position) < _backAwayDistance;
    }

    private void StrafeMove()
    {
        LookAtTarget();
        Vector3 toTarget = (_target.transform.position - transform.position);
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.01f) return;
        Vector3 right = Vector3.Cross(Vector3.up, toTarget.normalized);
        _rigidbody.MovePosition(_rigidbody.position + right * _strafeSideDir * _strafeSpeed * Time.fixedDeltaTime);
    }

    private void BackAwayMove()
    {
        LookAtTarget();
        Vector3 away = (transform.position - _target.transform.position);
        away.y = 0;
        if (away.sqrMagnitude < 0.01f) return;
        _rigidbody.MovePosition(_rigidbody.position + away.normalized * _backAwaySpeed * Time.fixedDeltaTime);
    }

    private void MoveTowardTarget(float speed)
    {
        Vector3 dir = (_target.transform.position - transform.position);
        dir.y = 0;
        if (dir.magnitude <= _stopDistance) return;
        dir.Normalize();
        FaceDirection(dir);
        _rigidbody.MovePosition(_rigidbody.position + dir * speed * Time.fixedDeltaTime);
    }

    private void LookAtTarget()
    {
        if (_target == null) return;
        Vector3 dir = (_target.transform.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        FaceDirection(dir);
    }

    private void FaceDirection(Vector3 dir)
    {
        Quaternion target = Quaternion.LookRotation(dir);
        Quaternion next = Quaternion.Slerp(_rigidbody.rotation, target, _rotateSlerp * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(next);
    }
}
