// ---------------------------------------------------------
// AngelGageUI.cs
// 作成日:  2026/4/9
// 作成者:  Mariko Haruki
// 概要:天使ゲージUI
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


public class AngelGageUI : MonoBehaviour
{
    [SerializeField] private Image _frame;
	public Image Frame => _frame;

    [SerializeField] private Image _gazeUI;
    public Image GageUI => _gazeUI;

    [SerializeField] private Image _overgazeUI;
    public Image OverGageUI => _overgazeUI;

    [SerializeField] private Image _wingUI;
    public Image WingUI => _wingUI;

    private void Awake()
	{

	}

	private void Start() 
	{
        InitializeGageUI();
	}

    // =========================================================================
    // 初期化関数
    // =========================================================================
    private void InitializeGageUI()
	{

	}
	
	private void Update() 
	{

	}


}
