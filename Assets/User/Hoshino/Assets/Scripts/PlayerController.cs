// ---------------------------------------------------------
// PlayerController.cs
// 作成日: 2026/4/7
// 作成者：星野愛由
// 概要: ZL/ZRの交互入力で前進、連続入力で旋回
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [Header("ステータス")]
    public int maxHp = 100;
    private int currentHp;

    [Header("手のオブジェクト")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("手の設定")]
    public float floorY = 1f;          // 床につく高さ
    public float raisedY = 1.5f;       // 上に上がる高さ
    public float lerpSpeed = 10f;      // 手が動く速さ
    public float idleResetTime = 1.0f; // 何秒離したら戻るか

    [Header("赤ちゃんの移動・旋回の設定")]
    public float moveStep = 2.5f;        // 1回の手の動きで進む距離
    public float curveMoveStep = 1.5f;   // カーブ（旋回）時に進む距離
    public float tapRotateAngle = 15f;   // 連続入力1回あたりの回転角度
    public float rotateLerpSpeed = 5f;   // 回転が滑らかになる速さ
    public float stateDuration = 0.5f;   // 移動状態を維持する時間（秒）

    [Header("滑り防止（スピード制限）")]
    public float maxSpeed = 3.0f; // これ以上は速くならない（連打対策）

    private float lastActiveTime;   // 最後に操作した時間
    private bool isNeutral;         // ニュートラル状態か

    private Rigidbody _rigidbody;
    private InputSystemActions input;
    private Vector3 leftTargetPos;
    private Vector3 rightTargetPos;
    private Quaternion targetRotation;

    private enum LastHand { None, Left, Right }
    private enum MoveState { Idle, Forward, Rotating }
    private LastHand lastHand = LastHand.None;
    private MoveState currentMoveState = MoveState.Idle;
    private Coroutine stateResetCoroutine;

    [Header("アニメ")]
    [SerializeField] public HaiHaiAnime _haiHaiAnime;

    private void Awake()
    {
        input = new InputSystemActions();
        _rigidbody = GetComponent<Rigidbody>();

        leftTargetPos = new Vector3(leftHand.localPosition.x, floorY, leftHand.localPosition.z);
        rightTargetPos = new Vector3(rightHand.localPosition.x, floorY, rightHand.localPosition.z);

        targetRotation = transform.rotation;
        UpdateActivityTime();

        currentHp = maxHp;
    }

    void Update()
    {
        // 放置によるリセット判定
        if (!isNeutral && (Time.time - lastActiveTime > idleResetTime)) ResetToNeutral();

        /* 回転と移動の補間処理（常に目標値へ滑らかに動かす） */
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateLerpSpeed);
        leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftTargetPos, Time.deltaTime * lerpSpeed);
        rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightTargetPos, Time.deltaTime * lerpSpeed);
    }
    private void FixedUpdate()
    {
        // 現在の水平方向の速度を取得（落下スピードであるY軸は無視する）
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);

        // もし現在のスピードが上限（maxSpeed）を超えていたら
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            // スピードを上限値に抑え込む
            Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;

            // Y軸の落下速度はそのままに、XとZの速度だけ上書きする
            _rigidbody.linearVelocity = new Vector3(cappedVelocity.x, _rigidbody.linearVelocity.y, cappedVelocity.z);
        }
    }

    private void OnEnable()
    {
        /* == ZL・ZRの入力取得 =================================================================== */
        // 押された瞬間に実行
        input.Crawl.LeftHand.performed += ctx => OnPressLeft();
        input.Crawl.RightHand.performed += ctx => OnPressRight();

        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    /* =====================================================================
       処理：手の状態
       ===================================================================== */
    private void OnPressLeft()
    {
        _haiHaiAnime?.OnInput(); // ← 追加
        isNeutral = false;
        UpdateActivityTime();

        // 連続入力判定：前回も左手だったら旋回
        if (lastHand == LastHand.Left)
        {
            PlayerRotate(tapRotateAngle); // 左回転
        }
        else
        {
            // 交互入力：前進
            leftTargetPos.y = floorY;
            rightTargetPos.y = raisedY;
            PlayerMoveForward();
            lastHand = LastHand.Left;
        }
    }
    private void OnPressRight()
    {
        _haiHaiAnime?.OnInput(); // ← 追加
        isNeutral = false;
        UpdateActivityTime();

        // 連続入力判定：前回も右手だったら旋回
        if (lastHand == LastHand.Right)
        {
            PlayerRotate(-tapRotateAngle); // 右回転
        }
        else
        {
            // 交互入力：前進
            rightTargetPos.y = floorY;
            leftTargetPos.y = raisedY;
            PlayerMoveForward();
            lastHand = LastHand.Right;
        }
    }
    /* ---------------------------------------------------------------------
       処理：両手を床につける
       --------------------------------------------------------------------- */
    private void ResetToNeutral()
    {
        leftTargetPos.y = floorY;
        rightTargetPos.y = floorY;

        isNeutral = true;
        lastHand = LastHand.None;
    }

    /* ---------------------------------------------------------------------
       処理：プレイヤーが前進する
       --------------------------------------------------------------------- */
    private void PlayerMoveForward()
    {
        _rigidbody.AddForce(transform.forward * moveStep, ForceMode.Impulse);
        SetMoveState(MoveState.Forward); // 前進ステートをセット
    }

    /* ---------------------------------------------------------------------
       処理：プレイヤーが回転する
       --------------------------------------------------------------------- */
    private void PlayerRotate(float angle)
    {
        // 1. 回転の目標値を更新
        Vector3 currentEuler = targetRotation.eulerAngles;
        currentEuler.y += angle;
        targetRotation = Quaternion.Euler(currentEuler);

        // 2. ★修正：その場で回るのではなく、カーブを描くように前進力も加える
        // 現在の正面ではなく、曲がる方向の少し先（角度の半分）に向かって押し出すと、
        // 車のハンドルのような自然な放物線移動になります。
        Vector3 curveDirection = Quaternion.Euler(0, angle / 2f, 0) * transform.forward;
        _rigidbody.AddForce(curveDirection * curveMoveStep, ForceMode.Impulse);

        SetMoveState(MoveState.Rotating); // 旋回ステートをセット
    }

    /* =====================================================================
       処理：現在時刻を保存
       ===================================================================== */
    private void UpdateActivityTime()
    {
        lastActiveTime = Time.time;
    }

    /* =====================================================================
       処理：物理的な衝突が起きた瞬間に呼ばれる関数
       ===================================================================== */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // ぶつかった相手から「EnemyController」スクリプトを取得する
            EnemyMovement enemy = collision.gameObject.GetComponent<EnemyMovement>();

            // 相手にスクリプトがちゃんと付いていたら
            if (enemy != null)
            {
                switch (currentMoveState)
                {
                    case MoveState.Forward:
                        Debug.Log("Damage:20（前進）");
                        enemy.TakeDamage(20);
                        break;

                    case MoveState.Rotating:
                        Debug.Log("Damage:10（旋回）");
                        enemy.TakeDamage(10);
                        break;
                }
            }
        }
    }

    /* =====================================================================
       処理：ダメージを計算する
       ===================================================================== */
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"いてっ！ {damage} ダメージを受けた！ 残りHP: {currentHp}");

        if (currentHp <= 0){
            Debug.Log("ゲームオーバー...");
            AlphaGameManager.instance.GameOver();
        }
    }

    /* =====================================================================
      処理：ステートを一定時間維持してからIdleに戻す
      ===================================================================== */
    private void SetMoveState(MoveState newState)
    {
        currentMoveState = newState;
        if (stateResetCoroutine != null) StopCoroutine(stateResetCoroutine);
        stateResetCoroutine = StartCoroutine(ResetStateAfterDelay());
    }

    private IEnumerator ResetStateAfterDelay()
    {
        yield return new WaitForSeconds(stateDuration);
        currentMoveState = MoveState.Idle;
    }
}