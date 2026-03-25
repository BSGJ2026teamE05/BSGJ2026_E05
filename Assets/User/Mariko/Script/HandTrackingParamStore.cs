// ---------------------------------------------------------
// HandTrackingParamStore.cs
// 作成日:  2026/3/22
// 作成者:  鞠子
// 概要:プレイヤーの移動
// ---------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

[RequireComponent(typeof(Rigidbody))]
public class HandTrackingParamStore : MonoBehaviour
{
    // MediaPipe の HandLandmarkerRunner
    [SerializeField] private HandLandmarkerRunner runner;

    [System.Serializable]
    public class HandPosition
    {
        // 手の座標履歴
        public List<Vector3> _positionList = new List<Vector3>();

        // 何フレーム分の座標を保持するか
        public int _collectionIndex = 10;

        // 先頭と末尾の差分ベクトル
        public Vector3 _moveVector = Vector3.zero;

        // 前進入力を再受付できるか
        public bool _canStep = true;

        // 回転入力を再受付できるか
        public bool _canTurn = true;
    }

    [Header("手の記録")]
    [SerializeField] private HandPosition _leftHand = new HandPosition();
    [SerializeField] private HandPosition _rightHand = new HandPosition();

    [Header("前進判定")]
    [SerializeField, Range(0f, 1f)] private float centerLineY = 0.5f;

    [Header("前進速度設定")]
    [SerializeField] private float forwardDamping = 5.0f;
    [SerializeField] private float minMoveThreshold = 0.02f;
    [SerializeField] private float maxMoveClamp = 0.3f;
    [SerializeField] private float speedMultiplier = 10f;

    [Header("回転判定")]
    [SerializeField] private float turnThreshold = 0.03f;

    [Header("回転量設定")]
    [SerializeField] private float turnPowerMultiplier = 120f;
    [SerializeField] private float maxTurnSpeed = 120f;
    [SerializeField] private float turnDamping = 6f;

    // Rigidbody
    private Rigidbody _rb;

    // 現在の前進速度
    private float _currentForwardSpeed = 0f;

    // 現在の回転速度
    // プラス = 右折、マイナス = 左折
    private float _currentTurnSpeed = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // 倒れないようにX/Z回転を固定
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 当たり判定を安定させる
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        // runner 未設定なら処理しない
        if (runner == null)
        {
            Debug.LogWarning("HandLandmarkerRunner が設定されていません");
            return;
        }

        // 手の座標履歴を更新
        SaveMoveList();

        // 前進判定
        CheckForwardStep(_leftHand, "Left");
        CheckForwardStep(_rightHand, "Right");

        // 回転判定
        CheckTurnInput();
    }

    private void FixedUpdate()
    {
        // 前方向に移動
        Vector3 moveDelta = transform.forward * _currentForwardSpeed * _speedBoostMultiplier *Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + moveDelta);

        // Y軸回転
        float nextY = _rb.rotation.eulerAngles.y + _currentTurnSpeed * Time.fixedDeltaTime;
        Quaternion nextRot = Quaternion.Euler(0f, nextY, 0f);
        _rb.MoveRotation(nextRot);

        // 徐々に減速
        _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, 0f, Time.fixedDeltaTime * forwardDamping);

        // 徐々に回転を止める
        _currentTurnSpeed = Mathf.Lerp(_currentTurnSpeed, 0f, Time.fixedDeltaTime * turnDamping);
    }

    /// <summary>
    /// 左右の手の現在座標を履歴に保存する
    /// x は未使用なので 0、
    /// y に手首Y、
    /// z に奥行き（Depth）を入れる
    /// </summary>
    private void SaveMoveList()
    {
        // 左手
        if (runner.isLeftHandDetected)
        {
            Vector3 leftPos = new Vector3(0f, runner.leftWristY, runner.leftDepth);
            AddPosition(_leftHand, leftPos);
            UpdateMoveVector(_leftHand);

            Debug.Log($"[Left] Y:{runner.leftWristY:F2} Z:{runner.leftDepth:F2} Move:{_leftHand._moveVector} Mag:{_leftHand._moveVector.magnitude:F3}");
        }
        else
        {
            Debug.Log("[Left] Lost");
        }

        // 右手
        if (runner.isRightHandDetected)
        {
            Vector3 rightPos = new Vector3(0f, runner.rightWristY, runner.rightDepth);
            AddPosition(_rightHand, rightPos);
            UpdateMoveVector(_rightHand);

            Debug.Log($"[Right] Y:{runner.rightWristY:F2} Z:{runner.rightDepth:F2} Move:{_rightHand._moveVector} Mag:{_rightHand._moveVector.magnitude:F3}");
        }
        else
        {
            Debug.Log("[Right] Lost");
        }
    }

    /// <summary>
    /// 新しい座標を末尾に追加し、
    /// _collectionIndex を超えたら古いデータを先頭から削除する
    /// </summary>
    private void AddPosition(HandPosition hand, Vector3 pos)
    {
        hand._positionList.Add(pos);

        while (hand._positionList.Count > hand._collectionIndex)
        {
            hand._positionList.RemoveAt(0);
        }
    }

    /// <summary>
    /// リストの先頭と末尾から移動ベクトルを計算する
    /// </summary>
    private void UpdateMoveVector(HandPosition hand)
    {
        if (hand._positionList.Count < 2)
        {
            hand._moveVector = Vector3.zero;
            return;
        }

        Vector3 first = hand._positionList[0];
        Vector3 last = hand._positionList[hand._positionList.Count - 1];
        hand._moveVector = last - first;
    }

    /// <summary>
    /// 手が中央線を上から下にまたいだとき、
    /// ベクトルの大きさに応じて前進速度を加算する
    /// </summary>
    private void CheckForwardStep(HandPosition hand, string handName)
    {
        if (hand._positionList.Count < 2) return;

        Vector3 prev = hand._positionList[hand._positionList.Count - 2];
        Vector3 current = hand._positionList[hand._positionList.Count - 1];

        // 上から下に線をまたいだか
        bool crossedDown = prev.y < centerLineY && current.y >= centerLineY;

        if (hand._canStep && crossedDown)
        {
            hand._canStep = false;

            float moveAmount = hand._moveVector.magnitude;

            // ノイズレベル以上の動きだけ採用
            if (moveAmount >= minMoveThreshold)
            {
                moveAmount = Mathf.Clamp(moveAmount, 0f, maxMoveClamp);

                // ベクトル量を前進速度に変換
                float addSpeed = moveAmount * speedMultiplier;
                _currentForwardSpeed += addSpeed;

                Debug.Log($"{handName} Forward AddSpeed : {addSpeed:F2}");
            }
        }

        // 線より上に戻ったら次回の入力を許可
        if (!hand._canStep && current.y < centerLineY)
        {
            hand._canStep = true;
        }
    }

    /// <summary>
    /// 右手だけ動かすと右折、
    /// 左手だけ動かすと左折する。
    /// 回転の強さは移動ベクトルの大きさで決める
    /// </summary>
    private void CheckTurnInput()
    {
        bool leftDetected = runner.isLeftHandDetected;
        bool rightDetected = runner.isRightHandDetected;

        float leftAmount = _leftHand._moveVector.magnitude;
        float rightAmount = _rightHand._moveVector.magnitude;

        bool leftActive = leftDetected && leftAmount >= turnThreshold;
        bool rightActive = rightDetected && rightAmount >= turnThreshold;

        // 右手だけ強く動いた -> 右折
        if (rightActive && !leftActive && _rightHand._canTurn)
        {
            _rightHand._canTurn = false;

            float turnStrength = Mathf.Clamp(rightAmount * turnPowerMultiplier, 0f, maxTurnSpeed);

            // 右折なのでプラス
            _currentTurnSpeed += turnStrength;

            Debug.Log($"Right hand -> Turn Right : {turnStrength:F2}");
        }

        // 左手だけ強く動いた -> 左折
        if (leftActive && !rightActive && _leftHand._canTurn)
        {
            _leftHand._canTurn = false;

            float turnStrength = Mathf.Clamp(leftAmount * turnPowerMultiplier, 0f, maxTurnSpeed);

            // 左折なのでマイナス
            _currentTurnSpeed -= turnStrength;

            Debug.Log($"Left hand -> Turn Left : {turnStrength:F2}");
        }

        // 入力が弱くなったら再受付
        if (!rightActive)
        {
            _rightHand._canTurn = true;
        }

        if (!leftActive)
        {
            _leftHand._canTurn = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Sceneビューで前方向を見やすくする
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}