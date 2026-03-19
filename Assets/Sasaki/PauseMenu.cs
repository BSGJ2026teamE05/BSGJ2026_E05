// ---------------------------------------------------------
// PauseMenu.cs
// 作成日:  2026/3/20
// 作成者:  佐々木
// 概要:ポーズ画面
// ---------------------------------------------------------




using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;      // ポーズ画面のパネル
    public Slider volumeSlider;        // 音量スライダー

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;   // ゲームを止める
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;   // ゲーム再開
        isPaused = false;
    }

    public void GoHome()
    {
        Time.timeScale = 1f;   // シーン移動前に必ず戻す！
        SceneManager.LoadScene("Home"); 
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }
}
