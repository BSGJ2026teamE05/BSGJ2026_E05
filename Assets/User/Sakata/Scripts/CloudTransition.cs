using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudTransition : MonoBehaviour
{
    [Header("雲オブジェクトのリスト（左から順に）")]
    public List<RectTransform> clouds;

    [Header("設定")]
    public float slideDuration = 0.6f;
    public float delayBetween = 0.1f;
    public float slideDistance = 800f;

    // シーン遷移後に自動でフェードアウト再生するか
    [Header("シーン開始時に自動で雲を消すか")]
    public bool autoPlayOnStart = true;

    void Start()
    {
        if (autoPlayOnStart)
        {
            // 全雲を画面内（初期位置）に戻してから消す
            ResetCloudsToVisible();
            StartCoroutine(CloudsSlideUpOut());
        }
    }

    // ===== 雲で画面を埋める（遷移前） =====
    public void PlayTransitionIn(Action onComplete = null)
    {
        StartCoroutine(CloudsSlideUp(onComplete));
    }

    // ===== 雲を上へ消す（遷移後） =====
    public void PlayTransitionOut(Action onComplete = null)
    {
        ResetCloudsToVisible();
        StartCoroutine(CloudsSlideUpOut(onComplete));
    }

    // 雲を初期位置（画面を埋めた状態）にリセット
    private void ResetCloudsToVisible()
    {
        foreach (var cloud in clouds)
        {
            cloud.gameObject.SetActive(true);
            // anchoredPositionのYを0（画面内）に戻す
            cloud.anchoredPosition = new Vector2(cloud.anchoredPosition.x, 0);
        }
    }

    // 雲が下から上へ埋まる
    IEnumerator CloudsSlideUp(Action onComplete = null)
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            StartCoroutine(SlideUp(clouds[i]));
            yield return new WaitForSeconds(delayBetween);
        }
        yield return new WaitForSeconds(slideDuration);
        onComplete?.Invoke();
    }

    // 雲が上へ消えていく
    IEnumerator CloudsSlideUpOut(Action onComplete = null)
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            StartCoroutine(SlideUpOut(clouds[i]));
            yield return new WaitForSeconds(delayBetween);
        }
        yield return new WaitForSeconds(slideDuration);
        onComplete?.Invoke();
    }

    // 上へ移動して消える（遷移前）
    IEnumerator SlideUp(RectTransform cloud)
    {
        cloud.gameObject.SetActive(true);

        Vector2 startPos = cloud.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, slideDistance);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            cloud.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        cloud.anchoredPosition = endPos;
        //cloud.gameObject.SetActive(false);
    }

    // 上へ移動して消える（遷移後）
    IEnumerator SlideUpOut(RectTransform cloud)
    {
        Vector2 startPos = cloud.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, slideDistance);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // ゆっくり始まって速くなるイージング
            t = Mathf.Pow(t, 2f);
            cloud.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        cloud.anchoredPosition = endPos;
        cloud.gameObject.SetActive(false);
    }
}