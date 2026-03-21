// ---------------------------------------------------------
// BaseEnemyScript.cs
// 作成日:  2026/3/19
// 作成者:  鞠子春樹
// 概要:エネミーの基底クラスのスクリプト
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseEnemyScript : ObjectBaseScript
{
	// 追従するターゲット
	[SerializeField] protected GameObject _target;

	// 物理
	[SerializeField] protected Rigidbody _rigidbody;

    // ステータスパラメータ
    [SerializeField] protected float _moveSpeed = 3.0f;
    [SerializeField] protected float _stopDistance = 1.0f;

    private void Awake()
	{
		
	}

	private void Start() 
	{
		if (_target == null) _target = GameObject.FindGameObjectWithTag("Player");
		if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_hp > 0)
        {
            Move();
        }
    }

    virtual protected void Move()
	{
        if (_target == null) return;
        if (_rigidbody == null) return;

        Vector3 direction = _target.transform.position - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;
        if (distance <= _stopDistance) return;

        direction = direction.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        _rigidbody.MoveRotation(lookRotation);

        Vector3 nextPos = _rigidbody.position + direction * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(nextPos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hp <= 0) return;


    }
}
