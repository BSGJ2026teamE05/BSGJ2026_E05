// ---------------------------------------------------------
// CheckPoint.cs
// 作成日:  2026/3/19
// 作成者:  坂田
// 概要:各チェックポイントの順番をつける
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPoint : MonoBehaviour
{
	// [SerializeField] private int _numId;
	[SerializeField] private int index;
	private CheckPointManager manager;


	private void Awake()
	{

	}

	private void Start() 
	{
        manager = FindFirstObjectByType<CheckPointManager>();
    }

    private void Update() 
	{

	}

    private void OnTriggerEnter(Collider other)
    {
        manager.OnCheckpointReached(index);
    }
}
