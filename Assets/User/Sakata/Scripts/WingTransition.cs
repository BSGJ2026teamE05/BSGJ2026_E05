// ---------------------------------------------------------
// WingTransition.cs
// 作成日:  2026/4/19
// 作成者:  坂田
// 概要:羽のトランジション
// ---------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WingTransition : MonoBehaviour
{
    [Header("羽のImage")]
    [SerializeField] private RectTransform wingImage;

    [Header("設定")]
    [SerializeField] private float expandDuration = 0.6f;  // 広がる時間
    [SerializeField] private float retractDuration = 0.5f; // 戻る時間

    [Header("シーン開始時に自動で羽を戻すか")]
    [SerializeField] private bool autoPlayOnStart = true;

    // 画面外左端のX座標（羽の待機位置）
    private float offScreenX;
    // 画面を覆った状態のX座標
    private float coveredX = 0f;

    private void Awake()
    {
        // 画面外左端を計算（羽の幅の分だけ左へ）
        offScreenX = -(Screen.width + wingImage.rect.width) / 2f;
    }

    private void Start()
    {
        if (autoPlayOnStart)
        {
            wingImage.gameObject.SetActive(true);
            SetWingPosition(coveredX);
            StartCoroutine(RetractWing());
        }
        else
        {
            // 待機位置へ
            wingImage.gameObject.SetActive(false);
            SetWingPosition(offScreenX);
        }
    }

    // ===== 羽を広げて画面を覆う（遷移前） =====
    public void PlayTransitionIn(Action onComplete = null)
    {
        SetWingPosition(offScreenX);
        StartCoroutine(ExpandWing(onComplete));
    }

    // ===== 羽を左へ戻す（遷移後） =====
    public void PlayTransitionOut(Action onComplete = null)
    {
        SetWingPosition(coveredX);
        StartCoroutine(RetractWing(onComplete));
    }

    // 羽が左から右へ広がる
    IEnumerator ExpandWing(Action onComplete = null)
    {
        wingImage.gameObject.SetActive(true);
        float elapsed = 0f;
        Vector2 startPos = new Vector2(offScreenX, wingImage.anchoredPosition.y);
        Vector2 endPos = new Vector2(coveredX, wingImage.anchoredPosition.y);

        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / expandDuration);
            // 勢いよく広がるイージング（最初速く）
            t = 1f - Mathf.Pow(1f - t, 3f);
            wingImage.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        wingImage.anchoredPosition = endPos;
        onComplete?.Invoke();
    }

    // 羽が左へ戻る（逆再生）
    IEnumerator RetractWing(Action onComplete = null)
    {

        float elapsed = 0f;
        Vector2 startPos = new Vector2(coveredX, wingImage.anchoredPosition.y);
        Vector2 endPos = new Vector2(offScreenX, wingImage.anchoredPosition.y);

        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            // 素早く戻るイージング（最初速く）
            t = 1f - Mathf.Pow(1f - t, 2f);
            wingImage.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        wingImage.anchoredPosition = endPos;
        wingImage.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private void SetWingPosition(float x)
    {
        wingImage.anchoredPosition = new Vector2(x, wingImage.anchoredPosition.y);
    }

}
