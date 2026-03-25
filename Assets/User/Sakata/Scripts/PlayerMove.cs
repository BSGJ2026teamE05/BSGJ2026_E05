// ---------------------------------------------------------
// PlayerMove.cs
// 作成日:  2026/3/19
// 作成者:  坂田
// 概要:webカメラのハンドトラッキングによるプレイヤーの前進および回転
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;


public class PlayerMove : MonoBehaviour
{
	// [SerializeField] private int _numId;
	[SerializeField] private HandLandmarkerRunner runner;

    [Header("移動パラメータ")]
    [SerializeField ] private float stepDistance = 2.0f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 2f;

    [Header("ハイハイ判定")]
    [SerializeField]private float pullY = 0.3f;
    [SerializeField] private float resetY = 0.4f;
    [SerializeField] private float pullZ = 0.2f;
    [SerializeField] private float resetZ = 0.1f;

    [Header("回転パラメータ")]
    [SerializeField] private float rotateAngle = 15;
    [SerializeField] private int haihaiCount = 3;

    [Header("攻撃パラメータ")]
    [SerializeField] private int attackDamage = 1;

    private int consecutiveCount = 0;
    private string lastStepHnad = "None";

    private Rigidbody rb;

    private Vector3 targetPos;
    private float currentYRotation;
    private float targetYRotation;
    private Quaternion targetRot;
    private string lastHand = "None";
    private bool canLeft = true;
    private bool canRight = true;
    private bool hasRotated = false;


    private void Awake()
	{

	}

	private void Start() 
	{
        targetPos = transform.position;
        currentYRotation = transform.eulerAngles.y;
        targetYRotation = currentYRotation;

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    }

    private void Update() 
	{
        if (!runner.isLeftHandDetected) canLeft = true;
        if (!runner.isRightHandDetected) canRight = true;

        if (runner.isLeftHandDetected) CheckHand("Left", runner.leftWristY, runner.leftDepth, ref canLeft);
        if (runner.isRightHandDetected) CheckHand("Right", runner.rightWristY, runner.rightDepth, ref canRight);

        if (runner.isLeftHandDetected || runner.isRightHandDetected)
        {
            string leftLog = runner.isLeftHandDetected ? $"Y:{runner.leftWristY:F2} Z:{runner.leftDepth:F2}" : "L:Lost";
            string rightLog = runner.isRightHandDetected ? $"Y:{runner.rightWristY:F2} Z:{runner.rightDepth:F2}" : "R:Lost";
            Debug.Log($"{leftLog} | {rightLog}");
        }

        Vector3 currentPos = transform.position;
        Vector3 moveTarget = new Vector3(targetPos.x, currentPos.y, targetPos.z);
        transform.position = Vector3.Lerp(currentPos, moveTarget, Time.deltaTime * moveSpeed);

        currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, Time.deltaTime * rotateSpeed);
        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
    }

    void CheckHand(string handName, float y, float z, ref bool canStep)
    {
        if (canStep && y > pullY && z > pullZ)
        {
            canStep = false;

            if (handName == lastStepHnad)
            {
                consecutiveCount++;

                int threshold = hasRotated ? 1 : haihaiCount;

                if (consecutiveCount >= threshold)
                {
                    if (handName == "Right") targetYRotation -= rotateAngle;
                    else if (handName == "Left") targetYRotation += rotateAngle;
                    consecutiveCount = 0;
                    hasRotated = true;
                }
            }
            else
            {
                Vector3 forward = Quaternion.Euler(0, targetYRotation, 0) * Vector3.forward;
                targetPos += forward * stepDistance;
                lastStepHnad = handName;
                consecutiveCount = 1;
                hasRotated = false;
            }
        }
        if (!canStep && y < resetY && z < resetZ)
        {
            canStep = true;
        }
    }

    // lastAttackTimeとattackCooldownは削除

    private HashSet<int> _hitEnemies = new HashSet<int>();

    private void OnCollisionEnter(Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<EnemyManager>();
        if (enemy != null)
        {
            int enemyId = collision.gameObject.GetInstanceID();

            // 同じエネミーにはまだヒットしていない場合のみダメージ
            if (!_hitEnemies.Contains(enemyId))
            {
                _hitEnemies.Add(enemyId);
                enemy.TakeDamage(attackDamage);
                Debug.Log($"{collision.gameObject.name} に攻撃！");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // エネミーから離れたらリセット、再接触でまたダメージ
        int enemyId = collision.gameObject.GetInstanceID();
        _hitEnemies.Remove(enemyId);
    }
}
