// ---------------------------------------------------------
// ResultSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 概要:右手で叩くともう一度プレイ、左手で叩くとタイトルへ
// ---------------------------------------------------------
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private HandLandmarkerRunner runner;

    [Header("シーン")]
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string titleSceneName = "";

    [Header("判定パラメータ")]
    [SerializeField, Range(0f, 1f)] private float centerLineY = 0.5f;
    [SerializeField] private float velocityThreshold = 4.5f;
    [SerializeField] private float minMoveDistance = 0.03f;
    [SerializeField] private float clapCooldown = 0.35f;

    // 内部状態（左右で共通構造）
    private HandState leftHand = new HandState();
    private HandState rightHand = new HandState();

    private void Update()
    {
        // 左手 → ゲームへ
        if (TryClap(runner.isLeftHandDetected, runner.leftWristY, ref leftHand))
        {
            SceneManager.LoadScene(gameSceneName);
        }

        // 右手 → タイトルへ
        else if (TryClap(runner.isRightHandDetected, runner.rightWristY, ref rightHand))
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    /// <summary>
    /// 叩き判定
    /// </summary>
    private bool TryClap(bool isDetected, float wristY, ref HandState state)
    {
        if (!isDetected)
        {
            state.Reset(wristY);
            return false;
        }

        float move = wristY - state.prevY;
        float velocity = Mathf.Clamp(move / Time.deltaTime, -5f, 5f);

        // 上にいた履歴を記録
        if (wristY < centerLineY)
        {
            state.wasAbove = true;
        }

        // クールダウン中
        if (Time.time - state.lastClapTime < clapCooldown)
        {
            state.prevY = wristY;
            return false;
        }

        // 判定条件
        bool isClap =
            state.canClap &&
            state.wasAbove &&
            wristY >= centerLineY &&
            velocity > velocityThreshold &&
            move > minMoveDistance;

        if (isClap)
        {
            state.OnClap(wristY);
            return true;
        }

        // 上に戻ったら再び有効
        if (!state.canClap && wristY < centerLineY)
        {
            state.canClap = true;
        }

        state.prevY = wristY;
        return false;
    }
}

#region 内部クラス

[System.Serializable]
public class HandState
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

    public void OnClap(float y)
    {
        canClap = false;
        wasAbove = false;
        lastClapTime = Time.time;
        prevY = y;
    }
}

#endregion