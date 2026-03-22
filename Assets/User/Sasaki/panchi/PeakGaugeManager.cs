using UnityEngine;
using UnityEngine.UI;

public class PeakGaugeManager : MonoBehaviour
{
    public static PeakGaugeManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private Image staminaBarImage;
    [SerializeField] private Image penaltyBarImage;

    [Header("ゲージゼロ時に表示するオブジェクト")]
    [SerializeField] private GameObject gaugeZeroObject; // ★ゼロになったらtrueにするオブジェクト

    [Header("Settings")]
    [SerializeField] private float maxCapacity = 100f;
    [SerializeField] private float staminaRecoveryRate = 10f;
    [SerializeField] private float autoHungerIncreaseRate = 5f;
    [SerializeField] private float enemyKillRecoverAmount = 5f;

    [Header("Current State (Read Only)")]
    [SerializeField] private float currentLimit;
    [SerializeField] private float currentStamina;
    [SerializeField] private float penaltyHungerTotal;

    private bool _isGaugeZero = false; // 二度発火防止フラグ

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        penaltyHungerTotal = 0f;
        currentLimit = maxCapacity;
        currentStamina = maxCapacity;

        // 初期状態は非表示にしておく
        if (gaugeZeroObject != null) gaugeZeroObject.SetActive(false);

        UpdateVisuals();
    }

    private void Update()
    {
        penaltyHungerTotal += autoHungerIncreaseRate * Time.deltaTime;
        penaltyHungerTotal = Mathf.Min(penaltyHungerTotal, maxCapacity);

        currentLimit = Mathf.Max(0f, maxCapacity - penaltyHungerTotal);
        currentStamina = Mathf.Min(currentStamina, currentLimit);

        if (currentStamina < currentLimit)
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, currentLimit);
        }

        CheckGaugeZero(); // ★ゼロチェック
        UpdateVisuals();
    }

    /// <summary>ゲージがゼロになったか監視する</summary>
    private void CheckGaugeZero()
    {
        if (!_isGaugeZero && currentLimit <= 0f)
        {
            _isGaugeZero = true;
            if (gaugeZeroObject != null)
            {
                gaugeZeroObject.SetActive(true);
                Debug.Log("[PeakGauge] ゲージゼロ！オブジェクトを表示しました");
            }
        }

        // ゲージが回復したら再び非表示に戻す場合はこちらを有効化
        // if (_isGaugeZero && currentLimit > 0f)
        // {
        //     _isGaugeZero = false;
        //     if (gaugeZeroObject != null) gaugeZeroObject.SetActive(false);
        // }
    }

    /// <summary>敵を倒した時に呼ぶ</summary>
    public void RecoverOnEnemyKill()
    {
        penaltyHungerTotal -= enemyKillRecoverAmount;
        penaltyHungerTotal = Mathf.Max(0f, penaltyHungerTotal);
    }

    private void UpdateVisuals()
    {
        staminaBarImage.fillAmount = currentStamina / maxCapacity;
        penaltyBarImage.fillAmount = penaltyHungerTotal / maxCapacity;
    }
}