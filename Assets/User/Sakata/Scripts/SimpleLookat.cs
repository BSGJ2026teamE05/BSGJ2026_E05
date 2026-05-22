using UnityEngine;

public class SimpleLookat : MonoBehaviour
{
    [SerializeField] Transform target;   // ゴール
    [SerializeField] Transform player;   // プレイヤー
    [SerializeField] Transform cursor;   // 矢印

    [Header("Settings")]
    [SerializeField] float radius = 2.0f;      // プレイヤーからの距離
    [SerializeField] float heightOffset = 1.0f; // 高さ


    void Update()
    {
        Vector3 direction = target.position - player.position;
        direction.y = 0;
        direction.Normalize();

        cursor.position = player.position
            + direction * radius
            + Vector3.up * heightOffset;

        cursor.rotation = Quaternion.LookRotation(direction);

        cursor.Rotate(90, 0, 0);
    }
}