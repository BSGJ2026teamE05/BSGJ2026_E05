// ---------------------------------------------------------
// EnemyManager.cs
// 作成日:  2026/3/
// 作成者:  坂田
// 概要:エネミーとプレイヤーの衝突判定
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyManager : MonoBehaviour
{
	// [SerializeField] private int _numId;
	[Header("HPの設定")]
	[SerializeField]
	private int maxHP = 1;
	private int currentHp;


	private void Awake()
	{
		currentHp = maxHP;
	}

	private void Start() 
	{

	}
	
	private void Update() 
	{

	}

	public void TakeDamage (int damage)
	{
		currentHp -= damage;
		Debug.Log($"{gameObject.name} HP: {currentHp}/{maxHP}");

		if (currentHp <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		Debug.Log($"{gameObject.name} 倒した");
		Destroy(gameObject);
	}
}
