using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// ポーズメニューの管理クラス（Input System対応版）
/// Escキーでポーズ画面の表示/非表示を切り替えます
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ポーズメニューのルートGameObject（Panelなど）")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Scene Settings")]
    [Tooltip("タイトル画面のシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";

    // ポーズ中かどうかのフラグ
    private bool isPaused = false;

    // Input System の Escape キー参照
    private Key escapeKey = Key.Escape;

    // ポーズ状態の公開プロパティ（外部から読み取り可）
    public bool IsPaused => isPaused;

    private void Start()
    {
        // 起動時はポーズメニューを非表示にする
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] pauseMenuUI が設定されていません。Inspector で割り当ててください。");
        }
    }

    private void Update()
    {
        // Input System の Keyboard クラスで Escape キーを検出
        if (Keyboard.current != null && Keyboard.current[escapeKey].wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// ポーズ状態をトグル（切り替え）する
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// ゲームをポーズする
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        // Time.timeScale を 0 にしてゲームを一時停止
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("[PauseMenuManager] ゲームをポーズしました。");
    }

    /// <summary>
    /// ゲームを再開する
    /// ポーズメニューの「再開」ボタンにアタッチしてください
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Time.timeScale を元に戻してゲームを再開
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("[PauseMenuManager] ゲームを再開しました。");
    }

    /// <summary>
    /// タイトル画面へ戻る
    /// ポーズメニューの「タイトルへ戻る」ボタンにアタッチしてください
    /// </summary>
    public void GoToTitle()
    {
        // シーン遷移前に必ず timeScale をリセットする
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log($"[PauseMenuManager] タイトル画面（{titleSceneName}）へ移動します。");
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// ゲームを終了する
    /// ポーズメニューの「ゲーム終了」ボタンにアタッチしてください
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[PauseMenuManager] ゲームを終了します。");

#if UNITY_EDITOR
        // エディタ上では再生モードを停止する
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドされたアプリではアプリケーションを終了する
        Application.Quit();
#endif
    }

    /// <summary>
    /// シーン破棄時に timeScale を必ずリセットする（安全策）
    /// </summary>
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}