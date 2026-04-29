// ---------------------------------------------------------
// DoorController.cs
// 作成日:  2026/3/26
// 作成者:  坂田
// 概要:　ドアが開く機能
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class DoorController : MonoBehaviour
{
    // [SerializeField] private int _numId;
    [Header("ドア")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("プレイヤー")]
    [SerializeField] private Transform player;

    [Header("設定")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float fallThresholdY = -5f;

    [Header("シーン")]
    [SerializeField] private string nextSceneName;

    private bool isOpening = false;
    private bool isOpened = false;
	
	private void Update() 
	{
        if (isOpening)
        {
            OpenAnimation();
        }

        // プレイヤーが落ちたら遷移
        if (isOpened && player.position.y < fallThresholdY)
        {
            isOpened = false; // 二重起動防止

            CloudTransition transition = FindAnyObjectByType<CloudTransition>();

            // 雲が全部埋まってからシーン遷移
            transition.PlayTransitionIn(() =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }
    }

    public void OpenDoor()
    {
        isOpening = true;
    }

    private void OpenAnimation()
    {
        Quaternion leftTarget = Quaternion.Euler(-90, 0, openAngle);
        Quaternion rightTarget = Quaternion.Euler(-90, 0, -openAngle);

        // 左ドア（左に回転）
        leftDoor.localRotation = Quaternion.Lerp(
            leftDoor.localRotation,
            leftTarget,
            Time.deltaTime * openSpeed
        );
        // 右ドア（右に回転）
        rightDoor.localRotation = Quaternion.Lerp(
            rightDoor.localRotation,
            rightTarget,
            Time.deltaTime * openSpeed
        );

        if (Quaternion.Angle(leftDoor.localRotation, leftTarget) < 1f)
        {
            leftDoor.localRotation = leftTarget;  // スナップして確定
            rightDoor.localRotation = rightTarget;
            isOpening = false;
            isOpened = true;
        }
    }
}
