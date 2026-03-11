// ---------------------------------------------------------
// EndResultUI.cs
// 作成日:  2026/3/8
// 作成者:  佐々木
// 概要:　エンドSceneのリザルト
// ---------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class EndResultUI : MonoBehaviour
{
    [SerializeField] private Text resultText;

    void Start()
    {
        int total = Enemycount.totalEnemy;
        int dead = Enemycount.deadEnemy;

        resultText.text = "倒した敵 : " + dead + " / " + total;
    }
}
