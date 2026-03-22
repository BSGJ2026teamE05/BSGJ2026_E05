// ---------------------------------------------------------
// AutoDestroy.cs
// 作成日:  2026/3/22
// 作成者:  佐々木
// 概要:設定した秒数後に消えるスクリプト
// ---------------------------------------------------------

using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float deleteTime = 3f;

    void Start()
    {
        Destroy(gameObject, deleteTime);
    }
}