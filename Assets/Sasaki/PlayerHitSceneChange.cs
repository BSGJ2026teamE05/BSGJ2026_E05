// ---------------------------------------------------------
// PlayerHitSceneChange.cs
// 作成日:  2026/3/8
// 作成者:  佐々木
// 概要: プレイヤーが当たったらシーン遷移
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHitSceneChange : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string nextSceneName = "GameClear";

    private bool isTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.CompareTag(playerTag))
        {
            isTriggered = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}