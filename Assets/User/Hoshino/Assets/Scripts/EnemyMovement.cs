// ---------------------------------------------------------
// EnemyMovement.cs
// 作成日:  2026/03/19
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
    [Header("Enemyの種類")]
    [SerializeField] private EnemyType _EnemyType = EnemyType.MiniTomato;

    [Header("演出設定")]
    [Tooltip("発見状態の演出（！）")]
    [SerializeField] private GameObject findEffect;
    [Tooltip("逃げている状態の演出（汗）")]
    [SerializeField] private GameObject sweatEffect;
    [Tooltip("エフェクトを表示しておく（発見移行）時間")]
    [SerializeField] private float exclamationDuration = 1.0f;
    [Tooltip("撃破時に再生する3Dエフェクト")]
    [SerializeField] private GameObject defeatEffectPrefab;

    [Header("起爆エフェクト設定 (Habanero専用)")]
    [Tooltip("点滅する色 (HDR対応：強く光らせることができます)")]
    [ColorUsage(true, true)]
    [SerializeField] private Color igniteFlashColor = Color.yellow;
    [Tooltip("Habaneroの画像（SpriteRenderer）を割り当ててください")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("ステータス")]
    [SerializeField] public int maxHp = 50;
    private int currentHp;

    [Header("移動設定")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private Transform playerTarget;

    [Header("徘徊設定")]
    [SerializeField] private float changeDirectionInterval = 2f;
    private Vector3 wanderDirection;

    [Header("索敵設定")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseDuration = 5f;
    [SerializeField] private float cooldownDuration = 1.5f;

    [Header("爆発設定 (Habanero専用)")]
    [Tooltip("起爆状態に入るプレイヤーとの距離")]
    [SerializeField] private float igniteRadius = 0.5f;
    [Tooltip("爆発のダメージが届く範囲")]
    [SerializeField] private float explosionRadius = 3f;
    [Tooltip("爆発でプレイヤーに与えるダメージ")]
    [SerializeField] private int explosionDamage = 1;
    [Tooltip("起爆状態になってから爆発するまでの時間(秒)")]
    [SerializeField] private float explosionDelay = 5f;
    private bool hasExploded = false;

    [Header("消滅タイミング設定")]
    [Tooltip("MiniTomatoやPotatoが潰れてから消滅するまでの時間(秒)")]
    [SerializeField] private float defaultDestroyDelay = 0.75f;
    [Tooltip("Habaneroが撃破・爆発してから消滅するまでの時間(秒)")]
    [SerializeField] private float habaneroDestroyDelay = 2.0f;

    // === ★追加：起爆中のノックバックを1回だけに制限するフラグ ===
    private bool hasIgniteKnockedBack = false;

    //[Header("ノックバック設定")]
    //[SerializeField] private float knockbackDistance = 2f;
    //[SerializeField] private float knockbackDuration = 0.5f;

    [Header("死亡時ノックバック")]
    [SerializeField] private float deathKnockbackDistance = 1.5f;
    [SerializeField] private float deathKnockbackDuration = 0.2f;

    [Header("スコア")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private int gageRecoverAmount = 10;
    [Tooltip("撃破時に出すスコアのプレハブ（+15など）")]
    [SerializeField] private GameObject scorePopupPrefab;

    private EnemyState currentState;
    private enum EnemyState { wander, find, Chase, Cooldown, Ignite }
    public enum EnemyType { MiniTomato, Potato, Habanero }

    public Rigidbody _rigidbody;
    public Animator animator;
    Vector3 movement;

    private float stateTimer = 0f;
    private float currentSpeed;
    private bool isSquashed = false;
    private bool isKnockedBack = false;
    private bool IsFleeingType => _EnemyType is EnemyType.MiniTomato or EnemyType.Potato;
    private bool IsStationaryType => _EnemyType is EnemyType.Potato;

    private void Start()
    {
        currentHp = maxHp;

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        // ★修正：ハバネロの場合のみ、SpriteRendererを自動取得する（不要な処理を削除）
        if (_EnemyType == EnemyType.Habanero && spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (findEffect != null) findEffect.SetActive(false);
        if (sweatEffect != null) sweatEffect.SetActive(false);
        StartCoroutine(WanderRoutine());
        ChangeState(EnemyState.wander);
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            int randomDir = UnityEngine.Random.Range(0, 5);
            Vector3 newDirection = Vector3.zero;

            switch (randomDir)
            {
                case 0: newDirection = Vector3.forward; break;
                case 1: newDirection = Vector3.back; break;
                case 2: newDirection = Vector3.left; break;
                case 3: newDirection = Vector3.right; break;
                case 4: newDirection = Vector3.zero; break;
            }

            wanderDirection = newDirection;
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }

    private void Update()
    {
        if (isSquashed || hasExploded || playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        switch (currentState)
        {
            case EnemyState.wander:
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.find);
                    break;
                }

                if (IsStationaryType)
                {
                    movement = Vector3.zero;
                    currentSpeed = 0f;
                }
                else
                {
                    movement = wanderDirection;
                    currentSpeed = moveSpeed;
                }
                break;

            case EnemyState.find:
                Vector3 lookDir = playerTarget.position - transform.position;
                lookDir.y = 0;
                movement = Vector3.zero;
                currentSpeed = 0;

                stateTimer += Time.deltaTime;
                if (stateTimer >= exclamationDuration) ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                Vector3 direction = Vector3.zero;

                if (IsFleeingType)
                {
                    direction = transform.position - playerTarget.position;
                    direction.y = 0;
                    movement = direction.normalized;
                    currentSpeed = moveSpeed;

                    stateTimer += Time.deltaTime;
                    if (stateTimer >= chaseDuration) ChangeState(EnemyState.Cooldown);
                }
                else
                {
                    if (_EnemyType == EnemyType.Habanero && distanceToPlayer <= igniteRadius)
                    {
                        ChangeState(EnemyState.Ignite);
                        break;
                    }

                    direction = playerTarget.position - transform.position;
                    direction.y = 0;
                    movement = direction.normalized;
                    currentSpeed = moveSpeed;

                    if (distanceToPlayer > detectionRange * 1.5f) ChangeState(EnemyState.wander);
                }
                break;

            case EnemyState.Cooldown:
                if (_EnemyType == EnemyType.Potato)
                {
                    movement = Vector3.zero;
                    currentSpeed = 0f;
                }
                else
                {
                    movement = wanderDirection;
                    currentSpeed = moveSpeed;
                }

                stateTimer += Time.deltaTime;
                if (stateTimer >= cooldownDuration) ChangeState(EnemyState.wander);
                break;

            case EnemyState.Ignite:
                movement = Vector3.zero;
                currentSpeed = 0f;
                stateTimer += Time.deltaTime;

                // === ★復活：確実に点滅させる処理 ===
                if (spriteRenderer != null)
                {
                    float progress = stateTimer / explosionDelay;
                    float blinkSpeed = Mathf.Lerp(15f, 60f, progress);
                    float lerp = (Mathf.Sin(stateTimer * blinkSpeed) + 1f) / 2f;

                    // 1. 専用ShaderがあればShaderで光らせる
                    if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
                    {
                        spriteRenderer.material.SetFloat("_FlashAmount", lerp);
                        spriteRenderer.material.SetColor("_FlashColor", igniteFlashColor);
                    }
                    // 2. ★超重要修正：elseを削除。Shaderの有無に関わらず「絶対に」C#でも点滅させる！
                    if (lerp > 0.5f)
                    {
                        // 色を少し透明な黄色にしつつ、ピーク時(0.85以上)は一瞬だけ完全に非表示にして激しい点滅を作る
                        spriteRenderer.color = new Color(igniteFlashColor.r, igniteFlashColor.g, igniteFlashColor.b, 0.4f);
                        spriteRenderer.enabled = (lerp < 0.85f);
                    }
                    else
                    {
                        spriteRenderer.color = Color.white;
                        spriteRenderer.enabled = true;
                    }
                }

                if (stateTimer >= explosionDelay)
                {
                    Explode();
                }
                break;
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isSquashed || isKnockedBack || hasExploded || playerTarget == null) return;
        _rigidbody.MovePosition(_rigidbody.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        float animX = movement.x;
        float animZ = movement.z;

        if (movement.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.z))
            {
                animX = movement.x > 0 ? 1f : -1f;
                animZ = 0f;
            }
            else
            {
                animX = 0f;
                animZ = movement.z > 0 ? 1f : -1f;
            }
        }

        animator.SetFloat("Horizontal", animX);
        animator.SetFloat("Vertical", animZ);
        animator.SetFloat("Speed", movement.sqrMagnitude > 0.01f ? currentSpeed : 0f);

        bool isFleeing = (currentState == EnemyState.Chase && IsFleeingType);
        animator.SetBool("IsFleeing", isFleeing);
    }

    //private IEnumerator KnockbackRoutine()
    //{
    //    isKnockedBack = true;
    //    animator.SetFloat("Speed", 0);

    //    Vector3 knockbackDir = (transform.position - playerTarget.position).normalized;
    //    knockbackDir.y = 0;

    //    Vector3 startPos = transform.position;
    //    //Vector3 targetPos = startPos + knockbackDir * knockbackDistance;

    //    float elapsedTime = 0f;

    //    while (elapsedTime < knockbackDuration)
    //    {
    //        // === ★追加：吹き飛んでいる最中に爆発したり倒されたりしたら強制終了する ===
    //        if (isSquashed || hasExploded) break;

    //        _rigidbody.MovePosition(Vector3.Lerp(startPos, targetPos, elapsedTime / knockbackDuration));
    //        elapsedTime += Time.fixedDeltaTime;
    //        yield return new WaitForFixedUpdate();
    //    }

    //    isKnockedBack = false;
    //}

    private IEnumerator DeathKnockbackRoutine()
    {
        isKnockedBack = true;

        Vector3 knockbackDir = transform.position - playerTarget.position;

        // 横方向だけ取得
        knockbackDir.y = 0f;
        knockbackDir.Normalize();

        Vector3 startPos = transform.position;

        // 飛ぶ先
        Vector3 endPos = startPos + knockbackDir * deathKnockbackDistance;

        float elapsed = 0f;

        while (elapsed < deathKnockbackDuration)
        {
            float t = elapsed / deathKnockbackDuration;

            // 横移動
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            // 放物線
            float height = Mathf.Sin(t * Mathf.PI) * 2.0f;

            pos.y += height;

            transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最終位置
        transform.position = endPos;

        isKnockedBack = false;

        // ===== 着地後につぶれアニメ =====
        animator.SetTrigger("Squash");

        float delay = (_EnemyType == EnemyType.Habanero)
            ? habaneroDestroyDelay
            : defaultDestroyDelay;

        StartCoroutine(DestroyAndPlayEffect(delay));
    }

    private IEnumerator ShowDiscoveryEffect()
    {
        if (findEffect != null)
        {
            findEffect.SetActive(true);
            yield return new WaitForSeconds(exclamationDuration);
            findEffect.SetActive(false);
        }
    }

    // === ★追加：指定秒数（2秒）待ってからエフェクトを出して消滅させるコルーチン ===
    private IEnumerator DestroyAndPlayEffect(float delay)
    {
        // アニメーションが終わるまで待つ
        yield return new WaitForSeconds(delay);

        // 消滅する瞬間にエフェクトを出す
        if (defeatEffectPrefab != null)
        {
            Instantiate(defeatEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        // オブジェクトを破壊
        Destroy(gameObject);
    }

    private void ChangeState(EnemyState newState)
    {
        if (newState == EnemyState.find)
        {
            if (IsFleeingType) StartCoroutine(ShowDiscoveryEffect());
        }

        if (sweatEffect != null)
        {
            bool isRunningAway = (newState == EnemyState.Chase && IsFleeingType);
            sweatEffect.SetActive(isRunningAway);
        }

        if (newState == EnemyState.Ignite)
        {
            Debug.Log($"ハバネロ起爆！{explosionDelay}秒後に爆発します。");
            if (animator != null) animator.SetTrigger("Squash");
        }

        currentState = newState;
        stateTimer = 0f;
    }

    private void Explode()
    {
        if (hasExploded || isSquashed) return;

        hasExploded = true;
        movement = Vector3.zero;
        currentSpeed = 0f;

        // ★追加：爆発したら点滅の非表示状態を強制的に元に戻す
        if (_EnemyType == EnemyType.Habanero && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
            {
                spriteRenderer.material.SetFloat("_FlashAmount", 0f);
            }
        }

        Debug.Log("ドカーン！ハバネロが爆発した！");

        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= explosionRadius)
            {
                // 天使ゲージ減少
                if (AlphaGameManager.instance != null)
                {
                    AlphaGameManager.instance.DamageAngelGage(explosionDamage);
                }
            }
        }

        // === ★追加：ハバネロは「爆発した直後（今この瞬間）」にエフェクトを出す ===
        if (defeatEffectPrefab != null)
        {
            // Y軸を少し浮かせて表示
            Instantiate(defeatEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        animator.SetTrigger("Squash");
        // === ★変更：Habanero専用の短い時間（0.5秒）で消滅させる ===
        Destroy(gameObject, habaneroDestroyDelay);
    }

    public void Squash()
    {
        if (isSquashed || hasExploded) return;

        isSquashed = true;
        movement = Vector3.zero;

        bool isFleeing = (currentState == EnemyState.Chase && IsFleeingType);
        animator.SetBool("IsFleeing", isFleeing);

        if (sweatEffect != null) sweatEffect.SetActive(false);

        // ★追加：潰されたら点滅の非表示状態を強制的に元に戻す
        if (_EnemyType == EnemyType.Habanero && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
            {
                spriteRenderer.material.SetFloat("_FlashAmount", 0f);
            }
        }

        //animator.SetTrigger("Squash");

        //// === ★変更：エネミーの種類を見て、待機する時間（Delay）を振り分ける ===
        //float delay = (_EnemyType == EnemyType.Habanero) ? habaneroDestroyDelay : defaultDestroyDelay;
        //StartCoroutine(DestroyAndPlayEffect(delay));
        StartCoroutine(DeathKnockbackRoutine());
    }

    public void TakeDamage(int damageAmount)
    {
        if (isSquashed || hasExploded) return;

        currentHp -= damageAmount;

        if (_EnemyType == EnemyType.Habanero)
        {
            // まだ起爆していなければ、攻撃をきっかけに起爆する
            if (currentState != EnemyState.Ignite)
            {
                ChangeState(EnemyState.Ignite);
            }
            // すでに起爆していて、まだノックバックしていなければ、1回だけノックバックする
            else if (!hasIgniteKnockedBack)
            {
                hasIgniteKnockedBack = true;
                //StartCoroutine(KnockbackRoutine());
            }
        }

        if (currentHp <= 0)
        {
            // 1. スコア加算のポップアップ表示
            if (scorePopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
                Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity);
            }

            Squash();
            AlphaGameManager.instance.AddScore(scoreValue);
            AlphaGameManager.instance.RecoverAngelGage(gageRecoverAmount);

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isKnockedBack && !isSquashed && !hasExploded)
        {
            if (_EnemyType == EnemyType.Habanero)
            {
                if (currentState != EnemyState.Ignite)
                {
                    ChangeState(EnemyState.Ignite);
                }
            }
            else
            {
                PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();

                if (player != null)
                {
                    player.TakeDamage(1);
                }
                else
                {
                    PlayerMoveImproved playerMove = collision.gameObject.GetComponentInParent<PlayerMoveImproved>();
                    if (playerMove != null)
                    {
                        Debug.LogWarning("PlayerController はありませんが、PlayerMoveImproved を検出しました。");
                    }
                    else
                    {
                        Debug.LogWarning($"プレイヤーにぶつかりましたが、親オブジェクトにダメージを処理するスクリプトが見つかりません: {collision.gameObject.name}");
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (_EnemyType == EnemyType.Habanero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, igniteRadius);

            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}