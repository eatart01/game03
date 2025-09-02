using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 戰鬥結果數據結構 - 用於傳遞戰鬥結果信息
/// </summary>
[System.Serializable]
public class BattleResult
{
    [Header("戰鬥結果")]
    public bool isVictory;                   // 是否勝利
    public bool isTalentEnemy;               // 是否是天賦點敵人
    public int enemyID;                      // 敵人編號
    public string enemyName;                 // 敵人名稱
    
    [Header("玩家狀態")]
    public int playerFinalHealth;            // 玩家最終生命值
    public int playerFinalMaxHealth;         // 玩家最終最大生命值
    public int playerFinalShield;            // 玩家最終護盾值
    public int healthRecovered;              // 生命恢復量
    public int maxHealthIncreased;           // 最大生命增加量
    
    [Header("獎勵信息")]
    public List<string> availableRewards;    // 可選獎勵列表
    public int talentPointsAwarded;          // 獲得的天賦點數
    public bool hasRewardSelection;          // 是否有獎勵選擇
    
    [Header("戰鬥統計")]
    public int turnsTaken;                   // 消耗回合數
    public int damageDealt;                  // 總造成傷害
    public int damageTaken;                  // 總承受傷害
    public int criticalHits;                 // 暴擊次數
    public int misses;                       // 未命中次數
    public List<string> talentsTriggered;    // 觸發的天賦列表
    
    [Header("戰鬥日誌")]
    public List<string> combatLog;           // 戰鬥過程日誌
    public string battleSummary;             // 戰鬥總結文本
    
    /// <summary>
    /// 默認構造函數
    /// </summary>
    public BattleResult()
    {
        availableRewards = new List<string>();
        talentsTriggered = new List<string>();
        combatLog = new List<string>();
    }
    
    /// <summary>
    /// 創建勝利結果
    /// </summary>
    public static BattleResult CreateVictoryResult(EntityStats player, EntityStats enemy, bool isTalentEnemy)
    {
        BattleResult result = new BattleResult
        {
            isVictory = true,
            isTalentEnemy = isTalentEnemy,
            enemyID = enemy.entityID,
            enemyName = enemy.entityName,
            playerFinalHealth = player.currentHealth,
            playerFinalMaxHealth = player.maxHealth,
            playerFinalShield = player.shield,
            talentPointsAwarded = isTalentEnemy ? 1 : 0,
            hasRewardSelection = !isTalentEnemy
        };
        
        return result;
    }
    
    /// <summary>
    /// 創建失敗結果
    /// </summary>
    public static BattleResult CreateDefeatResult(EntityStats player, EntityStats enemy)
    {
        BattleResult result = new BattleResult
        {
            isVictory = false,
            enemyID = enemy.entityID,
            enemyName = enemy.entityName,
            playerFinalHealth = player.currentHealth,
            playerFinalMaxHealth = player.maxHealth,
            playerFinalShield = player.shield
        };
        
        return result;
    }
    
    /// <summary>
    /// 設置戰鬥統計數據
    /// </summary>
    public void SetBattleStatistics(int turns, int dealt, int taken, int crits, int missCount)
    {
        turnsTaken = turns;
        damageDealt = dealt;
        damageTaken = taken;
        criticalHits = crits;
        misses = missCount;
        
        GenerateBattleSummary();
    }
    
    /// <summary>
    /// 生成戰鬥總結文本
    /// </summary>
    private void GenerateBattleSummary()
    {
        if (isVictory)
        {
            battleSummary = $"勝利！擊敗{enemyName}\n" +
                          $"回合數: {turnsTaken} | 傷害: {damageDealt} | 承受: {damageTaken}\n" +
                          $"暴擊: {criticalHits}次 | 未命中: {misses}次";
            
            if (isTalentEnemy)
            {
                battleSummary += $"\n獲得天賦點: {talentPointsAwarded}";
            }
        }
        else
        {
            battleSummary = $"失敗！被{enemyName}擊敗\n" +
                          $"堅持了{turnsTaken}回合 | 造成{damageDealt}點傷害";
        }
    }
    
    /// <summary>
    /// 添加觸發的天賦
    /// </summary>
    public void AddTriggeredTalent(string talentName)
    {
        if (!talentsTriggered.Contains(talentName))
        {
            talentsTriggered.Add(talentName);
        }
    }
    
    /// <summary>
    /// 設置獎勵選項
    /// </summary>
    public void SetRewardOptions(List<string> rewards)
    {
        availableRewards = new List<string>(rewards);
    }
    
    /// <summary>
    /// 添加戰鬥日誌
    /// </summary>
    public void AddCombatLog(string logEntry)
    {
        combatLog.Add(logEntry);
    }
    
    /// <summary>
    /// 獲取簡要結果描述
    /// </summary>
    public string GetResultDescription()
    {
        if (isVictory)
        {
            return isTalentEnemy ? 
                $"擊敗{enemyName}，獲得1個天賦點！" : 
                $"擊敗{enemyName}，選擇獎勵！";
        }
        return $"被{enemyName}擊敗...";
    }
    
    /// <summary>
    /// 獲取獎勵描述
    /// </summary>
    public string GetRewardsDescription()
    {
        if (availableRewards.Count == 0) return "沒有獎勵";
        
        string description = "可選獎勵:\n";
        for (int i = 0; i < availableRewards.Count; i++)
        {
            description += $"{i + 1}. {availableRewards[i]}\n";
        }
        return description;
    }
    
    /// <summary>
    /// 轉換為字典格式（用於序列化）
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "isVictory", isVictory },
            { "enemyID", enemyID },
            { "enemyName", enemyName },
            { "playerHealth", playerFinalHealth },
            { "playerMaxHealth", playerFinalMaxHealth },
            { "talentPoints", talentPointsAwarded },
            { "turns", turnsTaken },
            { "damageDealt", damageDealt },
            { "criticalHits", criticalHits }
        };
    }
    
    /// <summary>
    /// 從字典加載數據
    /// </summary>
    public void FromDictionary(Dictionary<string, object> data)
    {
        if (data.ContainsKey("isVictory")) isVictory = (bool)data["isVictory"];
        if (data.ContainsKey("enemyID")) enemyID = (int)data["enemyID"];
        if (data.ContainsKey("enemyName")) enemyName = (string)data["enemyName"];
        if (data.ContainsKey("playerHealth")) playerFinalHealth = (int)data["playerHealth"];
        if (data.ContainsKey("playerMaxHealth")) playerFinalMaxHealth = (int)data["playerMaxHealth"];
        if (data.ContainsKey("talentPoints")) talentPointsAwarded = (int)data["talentPoints"];
    }
    
    /// <summary>
    /// 複製戰鬥結果
    /// </summary>
    public BattleResult Clone()
    {
        return (BattleResult)this.MemberwiseClone();
    }
}

/// <summary>
/// 獎勵選擇結果
/// </summary>
[System.Serializable]
public class RewardSelectionResult
{
    public string selectedReward;            // 選擇的獎勵
    public int rewardIndex;                  // 獎勵索引
    public Dictionary<string, int> statChanges; // 屬性變化
    
    public RewardSelectionResult()
    {
        statChanges = new Dictionary<string, int>();
    }
}

/// <summary>
/// 天賦選擇結果
/// </summary>
[System.Serializable]
public class TalentSelectionResult
{
    public string selectedTalent;            // 選擇的天賦
    public bool wasUnlocked;                 // 是否是解鎖操作
    public bool wasActivated;                // 是否是激活操作
    
    public TalentSelectionResult(string talent, bool unlock, bool activate)
    {
        selectedTalent = talent;
        wasUnlocked = unlock;
        wasActivated = activate;
    }
}