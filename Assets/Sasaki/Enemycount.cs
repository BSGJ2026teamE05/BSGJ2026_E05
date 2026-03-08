// ---------------------------------------------------------
// Enemycount.cs
// 作成日:  2026/3/8
// 作成者:  佐々木
// 概要: 敵の総数と死亡数を管理
// ---------------------------------------------------------

using UnityEngine;

public class Enemycount : MonoBehaviour
{
    public static int totalEnemy = 0;
    public static int deadEnemy = 0;

    [Header("Debug表示")]
    [SerializeField] private int inspectorTotalEnemy;
    [SerializeField] private int inspectorDeadEnemy;

    void Update()
    {
        // static → Inspector用にコピー
        inspectorTotalEnemy = totalEnemy;
        inspectorDeadEnemy = deadEnemy;
    }

    public static void AddEnemy()
    {
        totalEnemy++;
    }

    public static void EnemyDead()
    {
        deadEnemy++;
    }
}