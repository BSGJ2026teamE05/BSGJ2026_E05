// ---------------------------------------------------------
// BossEnemyScript.cs
// 作成日:  2026/3/22
// 作成者:  佐々木
// 概要: ボスの行動スクリプト（カービィ中ボス風）
//       プレイヤーが近づくまでIdle待機
// ---------------------------------------------------------
using System.Collections;
using UnityEngine;

public class BossEnemyMoveScript : BaseEnemyScript
{
    // -------------------------------------------------------
    // ステート定義
    // -------------------------------------------------------
    private enum BossState
    {
        Idle,       // プレイヤーが近づくまで待機
        Watch,      // プレイヤーを見ながら様子見（横移動あり）
        Walk,       // もっさり近づく
        Charge,     // プレイヤーに向かって突進
        Recover,    // 突進後のよろけ
        Strafe,     // 横移動しながら様子見
        BackAway,   // プレイヤーから距離を取る
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

    // -------------------------------------------------------
    // 内部変数
    // -------------------------------------------------------
    private BossState _currentState = BossState.Idle;
    private float _lastChargeTime = -999f;
    private Vector3 _chargeDirection;
    private float _strafeSideDir = 1f;
    private bool _checkedBackAway = false;

    // -------------------------------------------------------
    // 初期化
    // -------------------------------------------------------
    private void OnEnable()
    {
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
                Debug.Log("[Boss] プレイヤーを検知！起動します");
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
    // 次のステートをランダムに決める
    // -------------------------------------------------------
    private BossState DecideNextState()
    {
        bool canCharge = (Time.time - _lastChargeTime) >= _chargeInterval;
        if (canCharge && Random.value < 0.4f) return BossState.Charge;

        float r = Random.value;
        if (r < 0.33f) return BossState.Watch;
        if (r < 0.66f) return BossState.Strafe;
        return BossState.Walk;
    }

    // -------------------------------------------------------
    // プレイヤーが近すぎるか判定
    // -------------------------------------------------------
    private bool IsPlayerTooClose()
    {
        if (_target == null) return false;
        return Vector3.Distance(transform.position, _target.transform.position) < _backAwayDistance;
    }

    // -------------------------------------------------------
    // Move()（FixedUpdateから呼ばれる）
    // -------------------------------------------------------
    protected override void Move()
    {
        if (_target == null || _rigidbody == null) return;
        if (_currentState == BossState.Idle) return; // Idle中は動かない

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
                LookAtTarget();
                break;
        }
    }

    // -------------------------------------------------------
    // ヘルパー
    // -------------------------------------------------------

    /// <summary>プレイヤーを向きながら横移動</summary>
    private void StrafeMove()
    {
        LookAtTarget();
        Vector3 toTarget = (_target.transform.position - transform.position);
        toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.01f) return;
        Vector3 right = Vector3.Cross(Vector3.up, toTarget.normalized);
        _rigidbody.MovePosition(_rigidbody.position + right * _strafeSideDir * _strafeSpeed * Time.fixedDeltaTime);
    }

    /// <summary>プレイヤーから後ずさる</summary>
    private void BackAwayMove()
    {
        LookAtTarget();
        Vector3 awayDir = (transform.position - _target.transform.position);
        awayDir.y = 0;
        if (awayDir.sqrMagnitude < 0.01f) return;
        awayDir.Normalize();
        _rigidbody.MovePosition(_rigidbody.position + awayDir * _backAwaySpeed * Time.fixedDeltaTime);
    }

    /// <summary>プレイヤーに向かってもっさり歩く</summary>
    private void MoveTowardTarget(float speed)
    {
        Vector3 dir = (_target.transform.position - transform.position);
        dir.y = 0;
        if (dir.magnitude <= _stopDistance) return;
        dir.Normalize();
        FaceDirection(dir);
        _rigidbody.MovePosition(_rigidbody.position + dir * speed * Time.fixedDeltaTime);
    }

    /// <summary>プレイヤーの方向をゆっくり向く</summary>
    private void LookAtTarget()
    {
        if (_target == null) return;
        Vector3 dir = (_target.transform.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        FaceDirection(dir);
    }

    /// <summary>指定方向にSlerpで滑らかに向く</summary>
    private void FaceDirection(Vector3 dir)
    {
        Quaternion target = Quaternion.LookRotation(dir);
        Quaternion next = Quaternion.Slerp(_rigidbody.rotation, target, _rotateSlerp * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(next);
    }
}