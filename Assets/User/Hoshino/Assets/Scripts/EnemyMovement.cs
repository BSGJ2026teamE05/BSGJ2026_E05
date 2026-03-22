// ---------------------------------------------------------
// PlayerMovement.cs
// 作成日:  2026/3/19
// 作成者:  星野愛由
// 概要:　Enemy
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EnemyMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;

    public Rigidbody _rigidbody;
    public Animator animator;

    Vector3 movement;

    private bool isSquashed = false; // 潰れたかどうかを管理

    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {
        movement.x = Keyboard.current.dKey.isPressed ? 1f : (Keyboard.current.aKey.isPressed ? -1f : 0f);   // 左右（A/D）
        movement.z = Keyboard.current.wKey.isPressed ? 1f : (Keyboard.current.sKey.isPressed ? -1f : 0f);   // 前後（W/S）

        /* アニメーターへの反映 */
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.z);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Squash();
        }
    }
    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + movement * moveSpeed * Time.fixedDeltaTime);

    }


    public void Squash()
    {
        if (isSquashed) return;

        isSquashed = true;
        movement = Vector3.zero;

        animator.SetTrigger("Squash");

        Destroy(gameObject, 1.5f);
    }
}