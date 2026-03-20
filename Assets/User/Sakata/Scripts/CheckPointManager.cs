// ---------------------------------------------------------
// CheckPointManager.cs
// 作成日:  2026/3/19
// 作成者:  坂田
// 概要:チェックポイントを通ったか管理
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPointManager : MonoBehaviour
{
	// [SerializeField] private int _numId;
	[SerializeField] private GameObject[] checkPoints;

	[Header("色の設定")]
	public Color activeColor = Color.yellow;
	public Color inactiveColor = Color.gray;
	public Color clearedColor = Color.green;

	private int currentIndex = 0;
	private void Awake()
	{

	}

	private void Start() 
	{
		UpdateColors();
	}
	
	private void Update() 
	{

	}

    public void OnCheckpointReached(int index)
    {
        // 順番通りでなければ無視
        if (index != currentIndex) return;

        Debug.Log($"Checkpoint {index} 通過！");
        currentIndex++;
        UpdateColors();

        if (currentIndex >= checkPoints.Length)
        {
            Debug.Log("全チェックポイント通過");
        }
    }

    private void UpdateColors()
    {
        for (int i = 0; i < checkPoints.Length; i++)
        {
            var renderer = checkPoints[i].GetComponent<Renderer>();
            if (i < currentIndex)
                renderer.material.color = clearedColor;
            else if (i == currentIndex)
                renderer.material.color = activeColor;
            else
                renderer.material.color = inactiveColor;
        }
    }
}
