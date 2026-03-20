// ---------------------------------------------------------
// NameSceneLoader.cs
// 作成日:  2026/3/8
// 作成者:  佐々木
// 概要:しーんとぶやつ
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public class NameSceneLoader : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "GameScene";

    public static string savedName;

    public void LoadScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}