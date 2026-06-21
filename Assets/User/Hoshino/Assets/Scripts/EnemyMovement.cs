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

    [Header("移動設定")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private Transform playerTarget;

    [Header("徘徊設定")]
    [SerializeField] private float changeDirectionInterval = 2f;

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

    [Header("消滅タイミング設定")]
    [Tooltip("MiniTomatoやPotatoが潰れてから消滅するまでの時間(秒)")]
    [SerializeField] private float defaultDestroyDelay = 0.75f;
    [Tooltip("Habaneroが撃破・爆発してから消滅するまでの時間(秒)")]
    [SerializeField] private float habaneroDestroyDelay = 2.0f;

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
    private Vector3 movement;
    private Vector3 wanderDirection;
    private int currentHp;
    private float stateTimer = 0f;
    private float currentSpeed;
    private bool isSquashed = false;
    private bool hasExploded = false;
    private bool isKnockedBack = false;
    private bool hasIgniteKnockedBack = false; // 起爆中のノックバックを1回だけに制限する
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
        if (_EnemyType == EnemyType.Habanero && spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // ハバネロのみ、SpriteRendererを自動取得する
        if (findEffect != null) findEffect.SetActive(false);
        if (sweatEffect != null) sweatEffect.SetActive(false);
        StartCoroutine(WanderRoutine());
        ChangeState(EnemyState.wander);
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

                if (spriteRenderer != null) // ハバネロ撃破時に、点滅させるための処理
                {
                    float progress = stateTimer / explosionDelay;
                    float blinkSpeed = Mathf.Lerp(15f, 60f, progress);
                    float lerp = (Mathf.Sin(stateTimer * blinkSpeed) + 1f) / 2f;

                    if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount")) // Shaderで光らせる
                    {
                        spriteRenderer.material.SetFloat("_FlashAmount", lerp);
                        spriteRenderer.material.SetColor("_FlashColor", igniteFlashColor);
                    }
                    
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


    // ============================================================================================================
    // 処理：アニメーション
    // ============================================================================================================
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

    // ============================================================================================================
    // 処理：歩行時
    // ============================================================================================================
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

    // ============================================================================================================
    // 処理：ノックバック
    // ============================================================================================================
    private IEnumerator DeathKnockbackRoutine()
    {
        isKnockedBack = true;

        Vector3 knockbackDir = transform.position - playerTarget.position;
        knockbackDir.y = 0f; // 横方向だけ取得
        knockbackDir.Normalize();

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + knockbackDir * deathKnockbackDistance; // 飛ぶ先

        float elapsed = 0f;

        while (elapsed < deathKnockbackDuration)
        {
            float t = elapsed / deathKnockbackDuration;
            Vector3 pos = Vector3.Lerp(startPos, endPos, t); // 横移動
            float height = Mathf.Sin(t * Mathf.PI) * 2.0f; // 放物線

            pos.y += height;

            transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // 最終位置
        isKnockedBack = false;
        animator.SetTrigger("Squash"); // 着地後に潰れるアニメーション

        float delay = (_EnemyType == EnemyType.Habanero)
            ? habaneroDestroyDelay
            : defaultDestroyDelay;

        StartCoroutine(DestroyAndPlayEffect(delay));
    }

    // ============================================================================================================
    // 処理：敵がプレイヤーを発見したときに出るエフェクト
    // ============================================================================================================
    private IEnumerator ShowDiscoveryEffect()
    {
        if (findEffect != null)
        {
            findEffect.SetActive(true);
            yield return new WaitForSeconds(exclamationDuration);
            findEffect.SetActive(false);
        }
    }

    // ============================================================================================================
    // 処理：指定秒数（2秒）待ってからエフェクトを出して消滅させる
    // ============================================================================================================
    private IEnumerator DestroyAndPlayEffect(float delay)
    {
        // アニメーションが終わるまで待つ
        yield return new WaitForSeconds(delay);

        // 消滅する瞬間にエフェクトを出す
        if (defeatEffectPrefab != null)
        {
            Instantiate(defeatEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }
        Destroy(gameObject); // オブジェクトを破壊
    }

    // ============================================================================================================
    // 処理：状態遷移
    // ============================================================================================================
    private void ChangeState(EnemyState newState)
    {
        // 爆発したら点滅の非表示状態を強制的に元に戻す
        if (newState == EnemyState.find) if (IsFleeingType) StartCoroutine(ShowDiscoveryEffect());
        
        if (sweatEffect != null)
        {
            bool isRunningAway = (newState == EnemyState.Chase && IsFleeingType);
            sweatEffect.SetActive(isRunningAway);
        }

        if (newState == EnemyState.Ignite)
        {
            if (animator != null) animator.SetTrigger("Squash");
        }

        currentState = newState;
        stateTimer = 0f;
    }

    // ============================================================================================================
    // 処理：爆発する
    // ============================================================================================================
    private void Explode()
    {
        if (hasExploded || isSquashed) return;
        hasExploded = true;
        movement = Vector3.zero;
        currentSpeed = 0f;

        if (_EnemyType == EnemyType.Habanero && spriteRenderer != null) // 爆発したら点滅の非表示状態を強制的に元に戻す
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
            {
                spriteRenderer.material.SetFloat("_FlashAmount", 0f);
            }
        }
        Debug.Log("ハバネロが爆発");

        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= explosionRadius)
            {
                if (AlphaGameManager.instance != null) AlphaGameManager.instance.DamageAngelGage(explosionDamage); // 天使ゲージ減少
            }
        }

        if (defeatEffectPrefab != null) // ハバネロが爆発した直後にエフェクトを出す
        {
            Instantiate(defeatEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity); // Y軸を少し浮かせて表示
        }
        animator.SetTrigger("Squash");
        Destroy(gameObject, habaneroDestroyDelay); // ハバネロを短い時間（0.5秒）で消滅させる
    }

    // ============================================================================================================
    // 処理：潰れる
    // ============================================================================================================
    public void Squash()
    {
        if (isSquashed || hasExploded) return;

        isSquashed = true;
        movement = Vector3.zero;

        bool isFleeing = (currentState == EnemyState.Chase && IsFleeingType);
        animator.SetBool("IsFleeing", isFleeing);

        if (sweatEffect != null) sweatEffect.SetActive(false);

        if (_EnemyType == EnemyType.Habanero && spriteRenderer != null) // 潰されたら点滅の非表示状態を強制的に元に戻す
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
            if (spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
            {
                spriteRenderer.material.SetFloat("_FlashAmount", 0f);
            }
        }
        StartCoroutine(DeathKnockbackRoutine());
    }

    // ============================================================================================================
    // 処理：ダメージを受ける
    // ============================================================================================================
    public void TakeDamage(int damageAmount)
    {
        if (isSquashed || hasExploded) return;

        currentHp -= damageAmount;

        if (_EnemyType == EnemyType.Habanero)
        {
            if (currentState != EnemyState.Ignite) // まだ起爆していなければ、起爆する
            {
                ChangeState(EnemyState.Ignite);
            }
            else if (!hasIgniteKnockedBack) // すでに起爆していてまだノックバックしていなければ、1回だけノックバックする
            {
                hasIgniteKnockedBack = true;
            }
        }

        if (currentHp <= 0)
        {
            if (scorePopupPrefab != null) // スコア加算のポップアップ表示
            {
                Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
                Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity);
            }
            Squash();
            AlphaGameManager.instance.AddScore(scoreValue);
            AlphaGameManager.instance.RecoverAngelGage(gageRecoverAmount);
        }
    }

    // ============================================================================================================
    // 処理：プレイヤーにぶつかった
    // ============================================================================================================
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isKnockedBack && !isSquashed && !hasExploded)
        {
            if (_EnemyType == EnemyType.Habanero)
            {
                if (currentState != EnemyState.Ignite) ChangeState(EnemyState.Ignite); // 着火状態に
            }
            else
            {
                PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
                if (player != null) player.TakeDamage(1);
            }
        }
    }

    // ============================================================================================================
    // 処理：敵の範囲の補助線を描画する
    // ============================================================================================================
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
