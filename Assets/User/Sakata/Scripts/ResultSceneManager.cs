// ---------------------------------------------------------
// ResultSceneManager.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 修正日:  2026/4/5
// 概要:右手で叩くともう一度プレイ、左手で叩くとタイトルへ
// ---------------------------------------------------------
using UnityEngine;
using UnityEngine.Rendering.LookDev;
using UnityEngine.SceneManagement;

public class ResultSceneManager : ClapDetectorBase
{
    [Header("── シーン ──")]
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string titleSceneName = "";

    [Header("── BGM/SE ──")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip gameOverBGM;
    [SerializeField] private AudioClip gameClearBGM;
    [SerializeField] private AudioClip SE;

    [SerializeField] private Material gameClearSky;
    [SerializeField] private Material gameOverSky;

    private ClapHandState _leftHand = new ClapHandState();
    private ClapHandState _rightHand = new ClapHandState();
    private InputSystemActions _input;
    private bool _isTransitioning = false;
    private bool _isInputting = false; // 名前入力中フラグ
    private AudioSource audioSource = null;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        string lastResult = PlayerPrefs.GetString("LastResult", "");
        if (bgmSource != null)
        {
            if (lastResult == "GameClear" && gameClearBGM != null)
            {
                bgmSource.clip = gameClearBGM;
                bgmSource.Play();
                RenderSettings.skybox = gameClearSky;
            }
            else if (lastResult == "GameOver" && gameOverBGM != null)
            {
                bgmSource.clip = gameOverBGM;
                bgmSource.Play();
                RenderSettings.skybox = gameOverSky;
            }
        }

        _input = new InputSystemActions();

        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        ResultManager resultManager = FindAnyObjectByType<ResultManager>();
        if (resultManager != null)
        {
            bool isRankIn = resultManager.CheckAndShowEntryWindow(lastScore);
            if (isRankIn)
            {
                _isInputting = true;
                _input.Disable();
            }
        }
        else
        {
            Debug.LogWarning("ResultManagerが見つかりません");
        }
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

    private void OnRightHandPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        LoadTitleScene();
    }

    public void OnNameInputComplete()
    {
        _isInputting = false;
        _input.Enable();
    }

    private void LoadGameScene()
    {
        if (_isTransitioning) return;
        if (_isInputting) return; // 名前入力中はシーン遷移しない
        _isTransitioning = true;
        PlaySE(SE);
        CloudTransition transition = FindAnyObjectByType<CloudTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    private void LoadTitleScene()
    {
        if (_isTransitioning) return;
        if (_isInputting) return; // 名前入力中はシーン遷移しない
        _isTransitioning = true;
        PlaySE(SE);
        CloudTransition transition = FindAnyObjectByType<CloudTransition>();
        transition.PlayTransitionIn(() =>
        {
            SceneManager.LoadScene(titleSceneName);
        });
    }

    private void Update()
    {
        if (_isInputting) return; // 名前入力中はハンドトラッキングを無視
        if (runner == null) return;

        if (TryClap(runner.isLeftHandDetected, runner.leftDepth, _leftHand, "Left"))
        {
            CloudTransition transition = FindAnyObjectByType<CloudTransition>();
            transition.PlayTransitionIn(() =>
            {
                SceneManager.LoadScene(gameSceneName);
            });
            return;
        }

        if (TryClap(runner.isRightHandDetected, runner.rightDepth, _rightHand, "Right"))
        {
            CloudTransition transition = FindAnyObjectByType<CloudTransition>();
            transition.PlayTransitionIn(() =>
            {
                SceneManager.LoadScene(titleSceneName);
            });
        }
    }

    private void PlaySE(AudioClip clip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.Log("audiosource=null");
        }

    }

}