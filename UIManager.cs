// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("戰鬥狀態顯示")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("玩家狀態顯示")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Slider playerShieldSlider;
    [SerializeField] private TextMeshProUGUI playerShieldText;
    [SerializeField] private GameObject playerBuffPanel;
    [SerializeField] private GameObject playerBuffIconPrefab;

    [Header("敵人狀態顯示")]
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private Slider enemyShieldSlider;
    [SerializeField] private TextMeshProUGUI enemyShieldText;
    [SerializeField] private GameObject enemyBuffPanel;

    [Header("手牌界面")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardCountText;

    [Header("天賦界面")]
    [SerializeField] private Transform talentPanel;
    [SerializeField] private GameObject talentIconPrefab;
    [SerializeField] private TextMeshProUGUI talentCountText;

    [Header("戰鬥結果界面")]
    [SerializeField] private GameObject battleResultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button continueButton;

    [Header("管理器參考")]
    [SerializeField] private BattleFlowManager battleFlowManager;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private TalentManager talentManager;
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private EntityStats playerStats;
    [SerializeField] private EntityStats enemyStats;

    private List<GameObject> currentCardUIs = new List<GameObject>();
    private Dictionary<int, GameObject> talentIcons = new Dictionary<int, GameObject>();

    // ---------- 初始化 ----------
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    private void InitializeUI()
    {
        // 隱藏戰鬥結果界面
        battleResultPanel.SetActive(false);
        
        // 初始化按鈕監聽
        continueButton.onClick.AddListener(OnContinueButtonClick);
    }

    private void SubscribeToEvents()
    {
        // 訂閱戰鬥事件
        BattleFlowManager.OnBattleStateChanged += OnBattleStateChanged;
        BattleFlowManager.OnTurnStart += OnTurnStart;
        BattleFlowManager.OnTurnEnd += OnTurnEnd;
        
        // 訂閱連擊事件
        ComboManager.OnComboUpdated += OnComboUpdated;
        ComboManager.OnTotalComboUpdated += OnTotalComboUpdated;
        
        // 訂閱天賦事件
        // TalentManager.OnTalentActivated += OnTalentActivated;
    }

    private void OnDisable()
    {
        // 取消訂閱
        BattleFlowManager.OnBattleStateChanged -= OnBattleStateChanged;
        BattleFlowManager.OnTurnStart -= OnTurnStart;
        BattleFlowManager.OnTurnEnd -= OnTurnEnd;
        ComboManager.OnComboUpdated -= OnComboUpdated;
        ComboManager.OnTotalComboUpdated -= OnTotalComboUpdated;
        // TalentManager.OnTalentActivated -= OnTalentActivated;
    }

    // ---------- 戰鬥狀態更新 ----------
    private void OnBattleStateChanged(BattleFlowManager.BattleState newState)
    {
        string stateName = newState switch
        {
            BattleFlowManager.BattleState.Preparation => "準備中",
            BattleFlowManager.BattleState.PlayerTurn => "玩家回合",
            BattleFlowManager.BattleState.EnemyTurn => "敵人回合",
            BattleFlowManager.BattleState.Resolving => "結算中",
            BattleFlowManager.BattleState.Victory => "勝利",
            BattleFlowManager.BattleState.Defeat => "失敗",
            _ => "未知"
        };

        stateText.text = $"狀態: {stateName}";
    }

    public void UpdateTurnDisplay(string message)
    {
        turnText.text = message;
    }

    // ---------- 實體狀態更新 ----------
    public void UpdateEntityHealthDisplay(EntityStats stats, bool isPlayer)
    {
        if (isPlayer)
        {
            playerHealthSlider.maxValue = stats.MaxHealth;
            playerHealthSlider.value = stats.CurrentHealth;
            playerHealthText.text = $"{stats.CurrentHealth}/{stats.MaxHealth}";
            
            playerShieldSlider.value = stats.Shield;
            playerShieldText.text = stats.Shield.ToString();
        }
        else
        {
            enemyHealthSlider.maxValue = stats.MaxHealth;
            enemyHealthSlider.value = stats.CurrentHealth;
            enemyHealthText.text = $"{stats.CurrentHealth}/{stats.MaxHealth}";
            
            enemyShieldSlider.value = stats.Shield;
            enemyShieldText.text = stats.Shield.ToString();
        }
    }

    // ---------- 手牌管理 ----------
    public void UpdateHandDisplay(List<CardData> handCards)
    {
        // 清除當前手牌顯示
        foreach (var cardUI in currentCardUIs)
        {
            Destroy(cardUI);
        }
        currentCardUIs.Clear();

        // 創建新的手牌顯示
        foreach (var card in handCards)
        {
            GameObject cardUI = Instantiate(cardPrefab, handPanel);
            CardUI cardUIComponent = cardUI.GetComponent<CardUI>();
            
            if (cardUIComponent != null)
            {
                cardUIComponent.Initialize(card, OnCardClicked);
            }
            
            currentCardUIs.Add(cardUI);
        }
    }

    private void OnCardClicked(CardData card)
    {
        // 通知BattleFlowManager玩家選擇了卡牌
        battleFlowManager.OnPlayerPlayCard(card);
    }

    public void UpdateDeckCountDisplay(int drawPileCount, int discardPileCount)
    {
        deckCountText.text = $"牌庫: {drawPileCount}";
        discardCountText.text = $"棄牌: {discardPileCount}";
    }

    // ---------- 連擊顯示 ----------
    private void OnComboUpdated(AttackType attackType, int comboCount)
    {
        UpdateComboDisplay();
    }

    private void OnTotalComboUpdated(int totalCombo)
    {
        comboText.text = $"連擊: {totalCombo}";
    }

    private void UpdateComboDisplay()
    {
        string comboInfo = $"輕: {comboManager.GetComboCount(AttackType.Light)} | " +
                          $"中: {comboManager.GetComboCount(AttackType.Medium)} | " +
                          $"重: {comboManager.GetComboCount(AttackType.Heavy)}";
        
        comboText.text = comboInfo;
    }

    // ---------- 天賦顯示 ----------
    public void UpdateTalentDisplay(List<int> activeTalentIDs)
    {
        talentCountText.text = $"天賦: {activeTalentIDs.Count}";

        // 更新天賦圖標顯示（簡化實現）
        // 實際應該創建/更新天賦圖標
    }

    // ---------- 戰鬥結果 ----------
    public void ShowBattleResult(bool isVictory)
    {
        battleResultPanel.SetActive(true);
        resultText.text = isVictory ? "戰鬥勝利！" : "戰鬥失敗...";
        resultText.color = isVictory ? Color.green : Color.red;
    }

    private void OnContinueButtonClick()
    {
        battleResultPanel.SetActive(false);
        // 這裡可以返回大地圖或進行其他處理
    }

    // ---------- 回合事件 ----------
    private void OnTurnStart()
    {
        // 更新所有UI狀態
        UpdateEntityHealthDisplay(playerStats, true);
        UpdateEntityHealthDisplay(enemyStats, false);
        UpdateComboDisplay();
    }

    private void OnTurnEnd()
    {
        // 清理臨時UI狀態
    }

    // ---------- 公共方法（供其他管理器調用）---------
    public void ShowDamageText(int damage, Vector3 position, bool isCrit = false)
    {
        // 實現傷害數字顯示
        Debug.Log($"顯示傷害數字: {damage} {(isCrit ? "暴擊！" : "")}");
    }

    public void ShowStatusEffect(string effectName, EntityStats target)
    {
        // 實現狀態效果顯示
        Debug.Log($"顯示狀態效果: {effectName}");
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        // 實現臨時消息顯示
        Debug.Log($"UI消息: {message}");
    }
}

// ---------- 卡牌UI組件 ----------
public class CardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;

    private CardData cardData;
    private System.Action<CardData> onClickCallback;

    public void Initialize(CardData data, System.Action<CardData> onClick)
    {
        cardData = data;
        onClickCallback = onClick;

        cardNameText.text = data.CardName;
        damageText.text = $"傷害: {data.BaseDamage}";
        accuracyText.text = $"命中: {data.BaseAccuracy}%";
        critText.text = $"暴擊: {data.BaseCritChance}%";

        cardButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        onClickCallback?.Invoke(cardData);
    }

    public void SetInteractable(bool interactable)
    {
        cardButton.interactable = interactable;
    }
}