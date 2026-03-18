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
    public float stepDistance = 2.0f;
    public float moveSpeed = 3f;
    public float rotateSpeed = 2f;

    [Header("ハイハイ判定")]
    public float pullY = 0.3f;
    public float resetY = 0.4f;
    public float pullZ = 0.2f;
    public float resetZ = 0.1f;

    [Header("回転パラメータ")]
    public float rotateAngle = 15;
    public int haihaiCount = 3;

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
}
