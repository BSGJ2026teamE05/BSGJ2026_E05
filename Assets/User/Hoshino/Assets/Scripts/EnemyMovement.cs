// ---------------------------------------------------------
// EnemyMovement.cs
// 作成日:  2026/3/19
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
    // === ★追加：徘徊用のスピードが抜け落ちていたので復活させました ===
    [SerializeField] public float wanderSpeed = 2f;
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

    [Header("ノックバック設定")]
    [SerializeField] private float knockbackDistance = 2f;
    [SerializeField] private float knockbackDuration = 0.5f;

    [Header("スコア")]
    [SerializeField] private int scoreValue = 10;
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

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
                    break; // ★追加：状態が変わったらここで処理をストップして次のフレームに任せる
                }

                if (IsStationaryType)
                {
                    movement = Vector3.zero;
                    currentSpeed = 0f;
                }
                else
                {
                    movement = wanderDirection;
                    currentSpeed = wanderSpeed; // ★修正：moveSpeed ではなく wanderSpeed に変更
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
                if (IsStationaryType)
                {
                    movement = Vector3.zero;
                    currentSpeed = 0f;
                }
                else
                {
                    movement = wanderDirection;
                    currentSpeed = wanderSpeed; // ★修正：moveSpeed ではなく wanderSpeed に変更
                }

                stateTimer += Time.deltaTime;
                if (stateTimer >= cooldownDuration) ChangeState(EnemyState.wander);
                break;

            case EnemyState.Ignite:
                movement = Vector3.zero;
                currentSpeed = 0f;
                stateTimer += Time.deltaTime;

                // ★追加：HasProperty で「このマテリアルはFlash機能を持っているか？」をチェック（エラー防止）
                if (spriteRenderer != null && spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
                {
                    float progress = stateTimer / explosionDelay;
                    float blinkSpeed = Mathf.Lerp(15f, 60f, progress);
                    float lerp = (Mathf.Sin(stateTimer * blinkSpeed) + 1f) / 2f;
                    spriteRenderer.material.SetFloat("_FlashAmount", lerp);
                    spriteRenderer.material.SetColor("_FlashColor", igniteFlashColor);
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

    private IEnumerator ShowDiscoveryEffect()
    {
        if (findEffect != null)
        {
            findEffect.SetActive(true);
            yield return new WaitForSeconds(exclamationDuration);
            findEffect.SetActive(false);
        }
    }

    private void Explode()
    {
        if (hasExploded || isSquashed) return;

        hasExploded = true;
        movement = Vector3.zero;
        currentSpeed = 0f;

        // ★追加：エラー防止のチェック
        if (spriteRenderer != null && spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
        {
            spriteRenderer.material.SetFloat("_FlashAmount", 0f);
        }

        Debug.Log("ドカーン！ハバネロが爆発した！");

        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= explosionRadius)
            {
                PlayerController pc = playerTarget.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(explosionDamage);
            }
        }

        animator.SetTrigger("Squash");
        Destroy(gameObject, 0.5f);
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
                collision.gameObject.GetComponent<PlayerController>().TakeDamage(1);
                StartCoroutine(KnockbackRoutine());
            }
        }
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;
        animator.SetFloat("Speed", 0);

        Vector3 knockbackDir = (transform.position - playerTarget.position).normalized;
        knockbackDir.y = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + knockbackDir * knockbackDistance;

        float elapsedTime = 0f;

        while (elapsedTime < knockbackDuration)
        {
            _rigidbody.MovePosition(Vector3.Lerp(startPos, targetPos, elapsedTime / knockbackDuration));
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isKnockedBack = false;
    }

    public void Squash()
    {
        if (isSquashed || hasExploded) return;

        isSquashed = true;
        movement = Vector3.zero;

        bool isFleeing = (currentState == EnemyState.Chase && IsFleeingType);
        animator.SetBool("IsFleeing", isFleeing);

        if (sweatEffect != null) sweatEffect.SetActive(false);

        // ★追加：エラー防止のチェック
        if (spriteRenderer != null && spriteRenderer.material != null && spriteRenderer.material.HasProperty("_FlashAmount"))
        {
            spriteRenderer.material.SetFloat("_FlashAmount", 0f);
        }

        animator.SetTrigger("Squash");

        Destroy(gameObject, 2.0f);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isSquashed || hasExploded) return;

        currentHp -= damageAmount;

        if (currentHp <= 0)
        {
            if (scorePopupPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
                Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity);
            }

            Squash();
            AlphaGameManager.instance.AddScore(scoreValue);
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