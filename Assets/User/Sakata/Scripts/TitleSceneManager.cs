//// ---------------------------------------------------------
//// TitleSceneManager.cs
//// 作成日:  2026/3/26
//// 作成者:  坂田
//// 概要: 両手同時に叩く動作でゲームシーンへ移動
//// ---------------------------------------------------------
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private HandLandmarkerRunner runner;
    [SerializeField] private string gameSceneName = "PrototypeScenetest";

    [Header("判定")]
    [SerializeField, Range(0f, 1f)] private float centerLineY = 0.5f;
    [SerializeField] private float velocityThreshold = 4.5f;
    [SerializeField] private float minMoveDistance = 0.03f;
    [SerializeField] private float clapCooldown = 0.35f;
    [SerializeField] private float simultaneousWindow = 0.3f;

    [SerializeField] private DoorController doorController;

    private HandState leftHand = new HandState();
    private HandState rightHand = new HandState();

    private float leftClapTime = -999f;
    private float rightClapTime = -999f;

    private bool isStarted = false;

    void Update()
    {
        if (TryClap(runner.isLeftHandDetected, runner.leftWristY, ref leftHand))
        {
            leftClapTime = Time.time;
        }

        if (TryClap(runner.isRightHandDetected, runner.rightWristY, ref rightHand))
        {
            rightClapTime = Time.time;
        }

        if (leftClapTime > 0f && rightClapTime > 0f)
        {
            if (Mathf.Abs(leftClapTime - rightClapTime) <= simultaneousWindow)
            {
                leftClapTime = -999f;
                rightClapTime = -999f;
                //SceneManager.LoadScene(gameSceneName);
                doorController.OpenDoor();
            }
        }
    }

    private bool TryClap(bool isDetected, float wristY, ref HandState state)
    {
        if (!isDetected)
        {
            state.Reset(wristY);
            return false;
        }

        float move = wristY - state.prevY;
        float velocity = move / Time.deltaTime;
        velocity = Mathf.Clamp(velocity, -5f, 5f);

        if (wristY < centerLineY)
        {
            state.wasAbove = true;
        }

        if (Time.time - state.lastClapTime < clapCooldown)
        {
            state.prevY = wristY;
            return false;
        }

        if (state.canClap &&
            state.wasAbove &&
            wristY >= centerLineY &&
            velocity > velocityThreshold &&
            move > minMoveDistance)
        {
            state.canClap = false;
            state.wasAbove = false;
            state.lastClapTime = Time.time;
            state.prevY = wristY;
            return true;
        }

        if (!state.canClap && wristY < centerLineY)
        {
            state.canClap = true;
        }

        state.prevY = wristY;
        return false;
    }

    private class HandState
    {
        public bool canClap = true;
        public bool wasAbove = false;
        public float prevY = 0f;
        public float lastClapTime = 0f;

        public void Reset(float y)
        {
            canClap = true;
            wasAbove = false;
            prevY = y;
        }
    }
}