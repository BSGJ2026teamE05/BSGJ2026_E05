using UnityEngine;
using UnityEngine.InputSystem;

public class BabyCrawl : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 80f;

    void Update()
    {
        if (Keyboard.current.aKey.isPressed)
        {
            // 左に回転しながら前進
            transform.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        if (Keyboard.current.dKey.isPressed)
        {
            // 右に回転しながら前進
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
}