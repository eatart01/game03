using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 戰鬥流程管理器 - 處理戰鬥狀態機和流程控制
/// </summary>
public class BattleFlowManager : MonoBehaviour
{
    // 單例模式
    public static BattleFlowManager Instance { get; private set; }
    
    // 戰鬥狀態
    public BattleState CurrentState { get; private set; }
    public enum BattleState
    {
        Idle,               // 閒置狀態
        PlayerTurn,         // 玩家回合
        EnemyTurn,          // 敵人回合
        Resolving,          // 解析中
        Victory,            // 勝利
        Defeat,             // 失敗
        RewardSelection,    // 獎勵選擇
        TalentSelection     // 天賦選擇
    }
    
    // 當前戰鬥數據
    private EntityStats currentEnemy;
    private int currentEnemyID;
    private bool isTalentEnemy = false;
    
    // 組件引用
    private BattleResolver battleResolver;
    private DeckManager deckManager;
    private PlayerProgress playerProgress;
    private UIManager uiManager;
    
    // 事件
    public System.Action<BattleState> OnBattleStateChanged;
    public System.Action<EntityStats> OnEnemySpawned;
    public System.Action OnBattleStarted;
    public System.Action OnBattleEnded;
    
    // 戰鬥數據追蹤
    private int turnNumber = 0;
    private string enemyNextAttack = "";
    private List<string> combatLog = new List<string>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        battleResolver = GetComponent<BattleResolver>();
        deckManager = GetComponent<DeckManager>();
        playerProgress = PlayerProgress.Instance;
        uiManager = UIManager.Instance;
        
        // 訂閱事件
        battleResolver.OnBattleResolved += HandleBattleResult;
        battleResolver.OnCombatLog += AddCombatLog;
        
        if (uiManager != null)
        {
            uiManager.startBattleButton.onClick.AddListener(StartBattle);
            uiManager.continueButton.onClick.AddListener(ContinueToNextEnemy);
            uiManager.nextEnemyButton.onClick.AddListener(ContinueToNextEnemy);
        }
    }
    
    private void Start()
    {
        SetState(BattleState.Idle);
        InitializeBattleEnvironment();
    }
    
    /// <summary>
    /// 初始化戰鬥環境
    /// </summary>
    private void InitializeBattleEnvironment()
    {
        // 預加載第一個敵人
        PreloadNextEnemy();
        
        // 更新UI顯示
        if (uiManager != null)
        {
            uiManager.UpdatePlayerUI(
                playerProgress.playerStats.currentHealth,
                playerProgress.playerStats.maxHealth,
                playerProgress.playerStats.shield,
                playerProgress.playerStats.ToDictionary()
            );
        }
    }
    
    /// <summary>
    /// 設置戰鬥狀態
    /// </summary>
    private void SetState(BattleState newState)
    {
        BattleState previousState = CurrentState;
        CurrentState = newState;
        
        Debug.Log($"戰鬥狀態變化: {previousState} -> {newState}");
        OnBattleStateChanged?.Invoke(newState);
        
        HandleStateEnter(newState, previousState);
    }
    
    /// <summary>
    /// 處理狀態進入
    /// </summary>
    private void HandleStateEnter(BattleState newState, BattleState previousState)
    {
        switch (newState)
        {
            case BattleState.PlayerTurn:
                OnPlayerTurnStart();
                break;
                
            case BattleState.EnemyTurn:
                OnEnemyTurnStart();
                break;
                
            case BattleState.Victory:
                OnVictory();
                break;
                
            case BattleState.Defeat:
                OnDefeat();
                break;
                
            case BattleState.RewardSelection:
                OnRewardSelection();
                break;
                
            case BattleState.TalentSelection:
                OnTalentSelection();
                break;
        }
    }
    
    /// <summary>
    /// 開始戰鬥
    /// </summary>
    public void StartBattle()
    {
        if (CurrentState != BattleState.Idle) return;
        
        turnNumber = 0;
        combatLog.Clear();
        
        // 創建敵人
        currentEnemy = CreateEnemyStats(currentEnemyID);
        isTalentEnemy = playerProgress.IsTalentEnemy(currentEnemyID);
        
        // 初始化戰鬥系統
        battleResolver.StartBattle(playerProgress.playerStats, currentEnemy);
        deckManager.OnCombatStart();
        
        // 預測敵人第一次攻擊
        UpdateEnemyNextAttack();
        
        // 更新UI
        uiManager.UpdateEnemyUI(
            currentEnemy.entityName,
            currentEnemy.currentHealth,
            currentEnemy.maxHealth,
            currentEnemy.ToDictionary(),
            enemyNextAttack
        );
        
        uiManager.SetCardInteractivity(true);
        
        SetState(BattleState.PlayerTurn);
        OnBattleStarted?.Invoke();
        OnEnemySpawned?.Invoke(currentEnemy);
        
        AddCombatLog($"戰鬥開始！對戰 {currentEnemy.entityName}");
    }
    
    /// <summary>
    /// 創建敵人狀態
    /// </summary>
    private EntityStats CreateEnemyStats(int enemyID)
    {
        int baseHealth = 100 + ((enemyID - 1) * 30); // 基礎生命 + 成長
        
        // 計算屬性加成（攻,防,命,暴,堅,閃 循環）
        int[] baseStats = new int[6];
        int baseStatValue = (enemyID - 1) / 6; // 每6個敵人一輪
        int statIndex = (enemyID - 1) % 6;
        
        for (int i = 0; i < 6; i++)
        {
            baseStats[i] = (i == statIndex) ? baseStatValue + 1 : baseStatValue;
        }
        
        // 特殊處理81-100號敵人
        if (enemyID >= 81 && enemyID <= 100)
        {
            // 這裡實現偷取屬性邏輯
            ApplyEnemyStealingMechanic(enemyID, baseStats);
        }
        
        return EntityStats.CreateEnemyStats(enemyID, baseHealth, baseStats);
    }
    
    /// <summary>
    /// 應用敵人偷取機制（81-100號敵人）
    /// </summary>
    private void ApplyEnemyStealingMechanic(int enemyID, int[] baseStats)
    {
        // 這裡實現敵人偷取玩家屬性的邏輯
        // 根據遊戲規則，每個敵人隨機偷取一個屬性
    }
    
    /// <summary>
    /// 玩家回合開始
    /// </summary>
    private void OnPlayerTurnStart()
    {
        turnNumber++;
        
        // 玩家每回合恢復
        playerProgress.playerStats.OnTurnStart();
        
        // 更新UI
        uiManager.UpdatePlayerUI(
            playerProgress.playerStats.currentHealth,
            playerProgress.playerStats.maxHealth,
            playerProgress.playerStats.shield,
            playerManager.playerStats.ToDictionary()
        );
        
        // 觸發回合開始天賦
        TalentManager.Instance.CheckTalentTriggers("OnTurnStart");
        deckManager.OnTurnStart();
        
        AddCombatLog($"回合 {turnNumber} - 玩家行動");
    }
    
    /// <summary>
    /// 敵人回合開始
    /// </summary>
    private void OnEnemyTurnStart()
    {
        AddCombatLog($"回合 {turnNumber} - 敵人行動");
        
        // 執行敵人AI
        StartCoroutine(ExecuteEnemyTurn());
    }
    
    /// <summary>
    /// 執行敵人回合
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        SetState(BattleState.Resolving);
        
        // 短暫延遲讓玩家看到回合變化
        yield return new WaitForSeconds(0.5f);
        
        // 解析敵人行動
        battleResolver.ResolveEnemyAction();
        
        // 更新敵人下一次攻擊預測
        UpdateEnemyNextAttack();
        
        // 更新UI
        uiManager.UpdateEnemyUI(
            currentEnemy.entityName,
            currentEnemy.currentHealth,
            currentEnemy.maxHealth,
            currentEnemy.ToDictionary(),
            enemyNextAttack
        );
        
        uiManager.UpdatePlayerUI(
            playerProgress.playerStats.currentHealth,
            playerProgress.playerStats.maxHealth,
            playerProgress.playerStats.shield,
            playerProgress.playerStats.ToDictionary()
        );
    }
    
    /// <summary>
    /// 玩家選擇卡牌
    /// </summary>
    public void PlayerSelectCard(int cardIndex)
    {
        if (CurrentState != BattleState.PlayerTurn) return;
        
        SetState(BattleState.Resolving);
        uiManager.SetCardInteractivity(false);
        
        // 獲取卡牌類型
        List<string> currentHand = deckManager.GetCurrentHand();
        if (cardIndex < 0 || cardIndex >= currentHand.Count) return;
        
        string cardType = currentHand[cardIndex];
        
        // 使用卡牌
        deckManager.PlayCard(cardIndex);
        
        // 解析玩家行動
        battleResolver.ResolvePlayerAction(cardType);
        
        // 更新UI
        uiManager.UpdatePlayerUI(
            playerProgress.playerStats.currentHealth,
            playerProgress.playerStats.maxHealth,
            playerProgress.playerStats.shield,
            playerProgress.playerStats.ToDictionary()
        );
        
        uiManager.UpdateEnemyUI(
            currentEnemy.entityName,
            currentEnemy.currentHealth,
            currentEnemy.maxHealth,
            currentEnemy.ToDictionary(),
            enemyNextAttack
        );
    }
    
    /// <summary>
    /// 處理戰鬥結果
    /// </summary>
    private void HandleBattleResult(BattleResult result)
    {
        if (result.isVictory)
        {
            SetState(BattleState.Victory);
        }
        else
        {
            SetState(BattleState.Defeat);
        }
        
        // 清理戰鬥狀態
        deckManager.OnCombatEnd();
        uiManager.SetCardInteractivity(false);
        
        OnBattleEnded?.Invoke();
    }
    
    /// <summary>
    /// 勝利處理
    /// </summary>
    private void OnVictory()
    {
        AddCombatLog("戰鬥勝利！");
        
        // 更新玩家進度
        playerProgress.OnEnemyDefeated(currentEnemyID, isTalentEnemy);
        
        // 顯示勝利UI
        uiManager.ShowBattleResult(true, GetVictoryRewardText());
        
        // 決定下一步
        if (isTalentEnemy)
        {
            StartCoroutine(ProceedToTalentSelection());
        }
        else
        {
            StartCoroutine(ProceedToRewardSelection());
        }
    }
    
    /// <summary>
    /// 失敗處理
    /// </summary>
    private void OnDefeat()
    {
        AddCombatLog("戰鬥失敗...");
        
        // 處理玩家死亡
        playerProgress.OnPlayerDeath();
        
        // 顯示失敗UI
        uiManager.ShowBattleResult(false, "你被擊敗了");
    }
    
    /// <summary>
    /// 獎勵選擇處理
    /// </summary>
    private void OnRewardSelection()
    {
        List<string> rewards = GenerateRewardOptions();
        uiManager.ShowRewardPanel(rewards);
    }
    
    /// <summary>
    /// 天賦選擇處理
    /// </summary>
    private void OnTalentSelection()
    {
        var newTalents = TalentManager.Instance.UnlockNewTalents();
        if (newTalents.Count > 0)
        {
            uiManager.ShowTalentPanel(newTalents.ConvertAll(t => t.description), playerProgress.availableTalentPoints);
        }
        else
        {
            // 所有天賦已解鎖，直接進入激活選擇
            var availableTalents = TalentManager.Instance.GetAvailableTalents();
            uiManager.ShowTalentPanel(availableTalents.ConvertAll(t => t.description), playerProgress.availableTalentPoints);
        }
    }
    
    /// <summary>
    /// 生成獎勵選項
    /// </summary>
    private List<string> GenerateRewardOptions()
    {
        // 這裡實現從20個獎勵中隨機選擇5個的邏輯
        return new List<string>
        {
            "+1命中率",
            "+1攻擊力",
            "恢復30生命",
            "+10最大生命",
            "+1每回合恢復生命"
        };
    }
    
    /// <summary>
    /// 繼續到下一個敵人
    /// </summary>
    public void ContinueToNextEnemy()
    {
        PreloadNextEnemy();
        SetState(BattleState.Idle);
        uiManager.HideRewardPanel();
        uiManager.HideTalentPanel();
        uiManager.HideBattleResult();
    }
    
    /// <summary>
    /// 預加載下一個敵人
    /// </summary>
    private void PreloadNextEnemy()
    {
        currentEnemyID = playerProgress.currentEnemyID;
        isTalentEnemy = playerProgress.IsTalentEnemy(currentEnemyID);
        
        // 預先創建敵人數據用於UI顯示
        EntityStats nextEnemy = CreateEnemyStats(currentEnemyID);
        
        // 更新UI預覽
        uiManager.UpdateEnemyUI(
            nextEnemy.entityName,
            nextEnemy.currentHealth,
            nextEnemy.maxHealth,
            nextEnemy.ToDictionary(),
            "未知" // 戰鬥開始前不知道敵人行動
        );
    }
    
    /// <summary>
    /// 更新敵人下一次攻擊預測
    /// </summary>
    private void UpdateEnemyNextAttack()
    {
        enemyNextAttack = battleResolver.PredictEnemyNextAttack();
    }
    
    /// <summary>
    /// 獲取勝利獎勵文本
    /// </summary>
    private string GetVictoryRewardText()
    {
        if (isTalentEnemy)
        {
            return "獲得1個天賦點！";
        }
        else
        {
            return "獲得獎勵選擇機會";
        }
    }
    
    /// <summary>
    /// 推進到獎勵選擇
    /// </summary>
    private IEnumerator ProceedToRewardSelection()
    {
        yield return new WaitForSeconds(1.5f);
        SetState(BattleState.RewardSelection);
    }
    
    /// <summary>
    /// 推進到天賦選擇
    /// </summary>
    private IEnumerator ProceedToTalentSelection()
    {
        yield return new WaitForSeconds(1.5f);
        SetState(BattleState.TalentSelection);
    }
    
    /// <summary>
    /// 添加戰鬥日誌
    /// </summary>
    private void AddCombatLog(string message)
    {
        combatLog.Add(message);
        uiManager.AddBattleLog(message);
    }
    
    /// <summary>
    /// 強制結束當前戰鬥（用於測試）
    /// </summary>
    public void ForceEndCurrentBattle(bool victory)
    {
        battleResolver.ForceEndBattle(victory);
    }
    
    /// <summary>
    /// 重啟戰鬥系統
    /// </summary>
    public void RestartBattleSystem()
    {
        SetState(BattleState.Idle);
        InitializeBattleEnvironment();
    }
    
    private void OnDestroy()
    {
        // 取消訂閱事件
        if (battleResolver != null)
        {
            battleResolver.OnBattleResolved -= HandleBattleResult;
            battleResolver.OnCombatLog -= AddCombatLog;
        }
    }
}