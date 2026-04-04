// ---------------------------------------------------------
// ResultSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 修正日:  2026/4/5
// 概要:右手で叩くともう一度プレイ、左手で叩くとタイトルへ
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultSceneManager : ClapDetectorBase
{
    [Header("── シーン ──")]
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string titleSceneName = "";

    // 内部状態
    private ClapHandState _leftHand = new ClapHandState();
    private ClapHandState _rightHand = new ClapHandState();

    private void Update()
    {
        if (runner == null) return;

        // 俯瞰撮影：wristY ではなく Depth(Z) を渡す

        // 左手 → もう一度プレイ
        if (TryClap(runner.isLeftHandDetected, runner.leftDepth, _leftHand, "Left"))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        // 右手 → タイトルへ
        if (TryClap(runner.isRightHandDetected, runner.rightDepth, _rightHand, "Right"))
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}