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

    private InputSystemActions _input;

    // 連続発火防止フラグ
    private bool _isTransitioning = false;

    private void Start()
    {
        _input = new InputSystemActions();
    }

    private void OnEnable()
    {
        if (_input == null) _input = new InputSystemActions();

        _input.Crawl.LeftHand.performed += OnLeftHandPerformed;
        _input.Crawl.RightHand.performed += OnRightHandPerformed;

        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Crawl.LeftHand.performed -= OnLeftHandPerformed;
        _input.Crawl.RightHand.performed -= OnRightHandPerformed;

        _input.Disable();
    }

    private void OnLeftHandPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        LoadGameScene();
    }

    /// <summary>右手 performed → タイトルへ</summary>
    private void OnRightHandPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        LoadTitleScene();
    }

    private void LoadGameScene()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        CloudTransition transition = FindAnyObjectByType<CloudTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    private void LoadTitleScene()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        CloudTransition transition = FindAnyObjectByType<CloudTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(titleSceneName);
        });
    }

    private void Update()
    {
        if (runner == null) return;

        // 俯瞰撮影：wristY ではなく Depth(Z) を渡す

        // 左手 → もう一度プレイ
        if (TryClap(runner.isLeftHandDetected, runner.leftDepth, _leftHand, "Left"))
        {

            CloudTransition transition = FindAnyObjectByType<CloudTransition>();

            transition.PlayTransitionIn(() =>
            {
                SceneManager.LoadScene(gameSceneName);
            });
            //SceneManager.LoadScene(gameSceneName);
            return;
        }

        // 右手 → タイトルへ
        if (TryClap(runner.isRightHandDetected, runner.rightDepth, _rightHand, "Right"))
        {
            CloudTransition transition = FindAnyObjectByType<CloudTransition>();

            transition.PlayTransitionIn(() =>
            {
                SceneManager.LoadScene(titleSceneName);
            });
            //SceneManager.LoadScene(titleSceneName);
        }
    }
}