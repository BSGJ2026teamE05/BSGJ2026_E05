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

    public Rigidbody theRB;
    public Animator animator;

    Vector3 movement;

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
    }
    void FixedUpdate()
    {
        //theRBの移動後の位置を決定するための式
        theRB.MovePosition(theRB.position + movement * moveSpeed * Time.fixedDeltaTime);

    }
}
