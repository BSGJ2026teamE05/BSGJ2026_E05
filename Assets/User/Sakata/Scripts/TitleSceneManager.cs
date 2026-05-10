// ---------------------------------------------------------
// TitleSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 概要:両手同時に叩く動作でゲームシーンへ移動
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class TitleSceneManager : ClapDetectorBase
{

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

    private InputSystemActions input;

    // ── Input Action のタイムスタンプ ──
    private float _leftActionTime = -999f;
    private float _rightActionTime = -999f;


    private void Start()
    {
        input = new InputSystemActions();
    }

    private void OnEnable()
    {
        if (input == null) input = new InputSystemActions();

        input.Crawl.LeftHand.performed += OnLeftHandPerformed;
        input.Crawl.RightHand.performed += OnRightHandPerformed;
        input.Enable();
    }

    private void OnDisable()
    {
        input.Crawl.LeftHand.performed -= OnLeftHandPerformed;
        input.Crawl.RightHand.performed -= OnRightHandPerformed;

        input.Disable();

    }

    private void OnLeftHandPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _leftActionTime = Time.time;
        CheckInputSimultaneous();
    }

    private void OnRightHandPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _rightActionTime = Time.time;
        CheckInputSimultaneous();
    }

    private void CheckInputSimultaneous()
    {
        if (_leftActionTime < 0f || _rightActionTime < 0f) return;

        if (Mathf.Abs(_leftActionTime - _rightActionTime) <= simultaneousWindow)
        {
            // リセット（連続発火防止）
            _leftActionTime = -999f;
            _rightActionTime = -999f;

            OnBothHandsClapped();
        }
    }

    /// <summary>両手同時入力が確定したときの処理</summary>
    private void OnBothHandsClapped()
    {
        doorController.OpenDoor();
        // SceneManager.LoadScene(gameSceneName);
    }

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