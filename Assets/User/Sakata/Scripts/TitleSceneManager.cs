// ---------------------------------------------------------
// TitleSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 概要:両手同時に叩く動作でゲームシーンへ移動
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : ClapDetectorBase
{
    [Header("── シーン ──")]
    [SerializeField] private string gameSceneName = "PrototypeScenetest";

    [Header("── 両手同時判定 ──")]
    [Tooltip("左右の叩きが何秒以内なら「同時」と判定するか")]
    [SerializeField] private float simultaneousWindow = 0.3f;

    [Header("── 演出 ──")]
    [SerializeField] private DoorController doorController;

    // 内部状態
    private ClapHandState _leftHand = new ClapHandState();
    private ClapHandState _rightHand = new ClapHandState();

    private float _leftClapTime = -999f;
    private float _rightClapTime = -999f;

    private void Update()
    {
        if (runner == null) return;

        // 俯瞰撮影：wristY ではなく Depth(Z) を渡す
        if (TryClap(runner.isLeftHandDetected, runner.leftDepth, _leftHand, "Left"))
            _leftClapTime = Time.time;

        if (TryClap(runner.isRightHandDetected, runner.rightDepth, _rightHand, "Right"))
            _rightClapTime = Time.time;

        // 両手が simultaneousWindow 秒以内に叩かれたら発火
        if (_leftClapTime > 0f && _rightClapTime > 0f)
        {
            if (Mathf.Abs(_leftClapTime - _rightClapTime) <= simultaneousWindow)
            {
                _leftClapTime = -999f;
                _rightClapTime = -999f;

                doorController.OpenDoor();
                // SceneManager.LoadScene(gameSceneName);
            }
        }
    }
}