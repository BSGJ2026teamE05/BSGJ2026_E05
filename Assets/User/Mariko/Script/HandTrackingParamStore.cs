// ---------------------------------------------------------
// HandTrackingParamStore.cs
// 作成日:  2026/3/
// 作成者:  
// 概要:
// ---------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class HandTrackingParamStore : MonoBehaviour
{
    // HandLandmarkerRunnerをInspectorでセット
    [SerializeField] private HandLandmarkerRunner runner;

    // 
    [SerializeField] public class HandPosition
    {
        public List<Vector3> _positionList;
        public int _collectionIndex;
    }

    [SerializeField] private HandPosition _leftHand;
    [SerializeField] private HandPosition _rightHand;

    private void Update()
    {
        // 左手
        if (runner.isLeftHandDetected)
        {
            Debug.Log(
                $"[Left]  Y:{runner.leftWristY:F2} Z:{runner.leftDepth:F2}"
            );
        }
        else
        {
            Debug.Log("[Left] Lost");
        }

        // 右手
        if (runner.isRightHandDetected)
        {
            Debug.Log(
                $"[Right] Y:{runner.rightWristY:F2} Z:{runner.rightDepth:F2}"
            );
        }
        else
        {
            Debug.Log("[Right] Lost");
        }

        SaveMoveList();
    }

    void SaveMoveList()
    {
        // 左手
        if (runner.isLeftHandDetected)
        {
            _leftHand._positionList.Add(new Vector3(0.0f, runner.leftWristY, runner.leftDepth));
        }

        // 右手
        if (runner.isRightHandDetected)
        {
            _leftHand._positionList.Add(new Vector3(0.0f, runner.leftWristY, runner.leftDepth));
        }
    }
}