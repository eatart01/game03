using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 管理玩家進度、永久屬性和遊戲狀態
/// </summary>
public class PlayerProgress : MonoBehaviour
{
    // 單例模式
    public static PlayerProgress Instance { get; private set; }
    
    [Header("玩家狀態")]
    public EntityStats playerStats;
    public int currentEnemyID = 1;
    public int battlesWon = 0;
    
    [Header("天賦系統")]
    public List<string> unlockedTalents = new List<string>(); // 已解鎖的天賦ID或名稱
    public List<string> activatedTalents = new List<string>(); // 已激活的天賦
    public int availableTalentPoints = 0;
    public int totalTalentPointsEarned = 0;
    
    [Header("獎勵衰減")]
    public bool rewardReductionPhase1 = false; // 51-80敵人
    public bool rewardReductionPhase2 = false; // 81-100敵人
    
    [Header("遊戲狀態")]
    public bool hasMaliciousTrade = false; // 是否擁有"惡性交易"天賦
    public int consecutiveWins = 0;
    public int deaths = 0;
    
    // 事件定義
    public System.Action OnPlayerStatsUpdated;
    public System.Action OnTalentPointsChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNewGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化新遊戲
    /// </summary>
    private void InitializeNewGame()
    {
        playerStats = EntityStats.CreateInitialPlayerStats();
        currentEnemyID = 1;
        battlesWon = 0;
        unlockedTalents.Clear();
        activatedTalents.Clear();
        availableTalentPoints = 0;
        totalTalentPointsEarned = 0;
        rewardReductionPhase1 = false;
        rewardReductionPhase2 = false;
        hasMaliciousTrade = false;
        consecutiveWins = 0;
        deaths = 0;
        
        Debug.Log("新遊戲初始化完成");
    }
    
    /// <summary>
    /// 擊敗敵人後的處理
    /// </summary>
    public void OnEnemyDefeated(int enemyID, bool isTalentEnemy)
    {
        battlesWon++;
        consecutiveWins++;
        
        // 更新當前敵人ID
        currentEnemyID = enemyID + 1;
        
        // 檢查獎勵衰減階段
        CheckRewardReductionPhases();
        
        // 如果是天賦點敵人
        if (isTalentEnemy)
        {
            AddTalentPoint();
        }
        
        // 玩家戰鬥勝利恢復
        playerStats.OnVictory();
        
        OnPlayerStatsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 檢查並更新獎勵衰減階段
    /// </summary>
    private void CheckRewardReductionPhases()
    {
        if (currentEnemyID >= 51 && currentEnemyID <= 80)
        {
            rewardReductionPhase1 = true;
            rewardReductionPhase2 = false;
        }
        else if (currentEnemyID >= 81)
        {
            rewardReductionPhase1 = false;
            rewardReductionPhase2 = true;
        }
        else
        {
            rewardReductionPhase1 = false;
            rewardReductionPhase2 = false;
        }
    }
    
    /// <summary>
    /// 添加天賦點
    /// </summary>
    public void AddTalentPoint()
    {
        availableTalentPoints++;
        totalTalentPointsEarned++;
        OnTalentPointsChanged?.Invoke();
        
        Debug.Log($"獲得天賦點，當前可用: {availableTalentPoints}");
    }
    
    /// <summary>
    /// 解鎖新天賦
    /// </summary>
    public void UnlockTalent(string talentId)
    {
        if (!unlockedTalents.Contains(talentId))
        {
            unlockedTalents.Add(talentId);
            Debug.Log($"解鎖天賦: {talentId}");
        }
    }
    
    /// <summary>
    /// 激活天賦
    /// </summary>
    public bool ActivateTalent(string talentId)
    {
        if (availableTalentPoints > 0 && unlockedTalents.Contains(talentId) && !activatedTalents.Contains(talentId))
        {
            activatedTalents.Add(talentId);
            availableTalentPoints--;
            OnTalentPointsChanged?.Invoke();
            
            // 特殊天賦處理
            if (talentId == "惡性交易")
            {
                hasMaliciousTrade = true;
            }
            
            Debug.Log($"激活天賦: {talentId}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 應用獎勵效果
    /// </summary>
    public void ApplyRewardEffect(string rewardType, int value)
    {
        switch (rewardType)
        {
            case "命中率":
                playerStats.accuracy += GetRewardValue(value);
                break;
            case "閃避率":
                playerStats.evasion += GetRewardValue(value);
                break;
            case "攻擊力":
                playerStats.attack += GetRewardValue(value);
                break;
            case "防禦力":
                playerStats.defense += GetRewardValue(value);
                break;
            case "暴擊率":
                playerStats.critRate += GetRewardValue(value);
                break;
            case "堅韌":
                playerStats.resilience += GetRewardValue(value);
                break;
            case "最大生命":
                playerStats.maxHealth += GetRewardValue(value);
                break;
            case "每回合恢復":
                playerStats.healthRegenPerTurn += GetRewardValue(value);
                break;
            case "勝利恢復":
                playerStats.healthOnVictory += GetRewardValue(value);
                break;
        }
        
        OnPlayerStatsUpdated?.Invoke();
    }
    
    /// <summary>
    /// 根據當前階段獲取獎勵數值（處理衰減）
    /// </summary>
    private int GetRewardValue(int baseValue)
    {
        if (rewardReductionPhase2) // 81-100敵人
        {
            return Mathf.CeilToInt(baseValue * 0.2f); // 1/5
        }
        else if (rewardReductionPhase1) // 51-80敵人
        {
            return Mathf.CeilToInt(baseValue * 0.5f); // 1/2
        }
        return baseValue; // 1-50敵人
    }
    
    /// <summary>
    /// 玩家死亡處理
    /// </summary>
    public void OnPlayerDeath()
    {
        deaths++;
        consecutiveWins = 0;
        
        // 檢查是否有復活天賦
        bool hasReviveTalent = activatedTalents.Contains("深層執念...肉與骨");
        
        if (!hasReviveTalent)
        {
            // 遊戲結束處理
            Debug.Log("遊戲結束");
            // 這裡可以觸發遊戲結束UI
        }
        else
        {
            // 復活邏輯
            Debug.Log("觸發復活天賦");
            // 根據天賦描述實現復活效果
        }
    }
    
    /// <summary>
    /// 獲取當前獎勵衰減描述
    /// </summary>
    public string GetRewardReductionDescription()
    {
        if (rewardReductionPhase2)
            return "你感受到洞窟氧氣的極度缺乏（獎勵效果為1/5）";
        else if (rewardReductionPhase1)
            return "你感受到洞窟的氧氣減少（獎勵效果為1/2）";
        return "正常獎勵效果";
    }
    
    /// <summary>
    /// 保存遊戲進度
    /// </summary>
    public void SaveGame()
    {
        // 這裡實現存檔邏輯
        Debug.Log("遊戲進度已保存");
        /*
        PlayerPrefs.SetInt("CurrentEnemyID", currentEnemyID);
        PlayerPrefs.SetInt("BattlesWon", battlesWon);
        // ... 其他需要保存的數據
        PlayerPrefs.Save();
        */
    }
    
    /// <summary>
    /// 加載遊戲進度
    /// </summary>
    public void LoadGame()
    {
        // 這裡實現讀檔邏輯
        Debug.Log("遊戲進度已加載");
        /*
        currentEnemyID = PlayerPrefs.GetInt("CurrentEnemyID", 1);
        battlesWon = PlayerPrefs.GetInt("BattlesWon", 0);
        // ... 其他需要加載的數據
        */
    }
    
    /// <summary>
    /// 重置當前戰鬥的臨時狀態
    /// </summary>
    public void ResetBattleStatus()
    {
        playerStats.ResetTemporaryStats();
    }
    
    /// <summary>
    /// 檢查是否是天賦點敵人
    /// </summary>
    public bool IsTalentEnemy(int enemyID)
    {
        // 001, 004, 007, 010, ..., 079
        return (enemyID - 1) % 3 == 0 && enemyID <= 79;
    }
}