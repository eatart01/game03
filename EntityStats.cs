using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 實體屬性數據類，用於玩家和敵人
/// </summary>
[System.Serializable]
public class EntityStats
{
    [Header("基本資訊")]
    public string entityName;
    public int entityID; // 敵人編號或玩家ID
    
    [Header("生命值相關")]
    public int maxHealth;
    public int currentHealth;
    public int shield; // 護盾值
    
    [Header("核心屬性")]
    public int attack;       // 攻擊力
    public int defense;      // 防禦力
    public int accuracy;     // 命中率
    public int critRate;     // 暴擊率
    public int resilience;   // 堅韌（減少被暴擊機率）
    public int evasion;      // 閃避率
    
    [Header("戰鬥屬性加成")]
    public int lightAttackDamage = 16;    // 輕攻擊傷害
    public int mediumAttackDamage = 24;   // 中攻擊傷害
    public int heavyAttackDamage = 49;    // 重攻擊傷害
    
    public int lightAttackAccuracy = 72;  // 輕攻擊命中率
    public int mediumAttackAccuracy = 64; // 中攻擊命中率
    public int heavyAttackAccuracy = 57;  // 重攻擊命中率
    
    public int lightAttackCritRate = 0;   // 輕攻擊暴擊率
    public int mediumAttackCritRate = 5;  // 中攻擊暴擊率
    public int heavyAttackCritRate = 10;  // 重攻擊暴擊率
    
    [Header("恢復屬性")]
    public int healthRegenPerTurn = 1;    // 每回合恢復生命
    public int healthOnVictory = 20;      // 戰鬥勝利恢復生命
    public int maxHealthIncreaseOnVictory = 5; // 戰鬥勝利增加最大生命
    
    [Header("狀態標誌")]
    public bool isPlayer;
    public bool isAlive = true;
    
    // 天賦和效果相關的臨時屬性（用於單場戰鬥）
    [System.NonSerialized] public Dictionary<string, int> temporaryBonuses = new Dictionary<string, int>();
    [System.NonSerialized] public Dictionary<string, int> temporaryPenalties = new Dictionary<string, int>();
    
    /// <summary>
    /// 默認構造函數
    /// </summary>
    public EntityStats() { }
    
    /// <summary>
    /// 創建玩家初始狀態
    /// </summary>
    public static EntityStats CreateInitialPlayerStats()
    {
        return new EntityStats
        {
            entityName = "玩家",
            isPlayer = true,
            maxHealth = 100,
            currentHealth = 100,
            shield = 0,
            attack = 0,
            defense = 0,
            accuracy = 0,
            critRate = 0,
            resilience = 0,
            evasion = 0,
            healthRegenPerTurn = 1,
            healthOnVictory = 20,
            maxHealthIncreaseOnVictory = 5
        };
    }
    
    /// <summary>
    /// 創建敵人初始狀態（根據敵人編號）
    /// </summary>
    public static EntityStats CreateEnemyStats(int enemyID, int baseHealth, int[] baseStats)
    {
        var stats = new EntityStats
        {
            entityName = $"敵人{enemyID:000}",
            entityID = enemyID,
            isPlayer = false,
            maxHealth = baseHealth,
            currentHealth = baseHealth,
            shield = 0,
            isAlive = true
        };
        
        // 應用基礎屬性
        if (baseStats != null && baseStats.Length >= 6)
        {
            stats.attack = baseStats[0];
            stats.defense = baseStats[1];
            stats.accuracy = baseStats[2];
            stats.critRate = baseStats[3];
            stats.resilience = baseStats[4];
            stats.evasion = baseStats[5];
        }
        
        return stats;
    }
    
    /// <summary>
    /// 獲取最終屬性值（包含臨時加成和減益）
    /// </summary>
    public int GetFinalStat(string statName)
    {
        int baseValue = 0;
        
        switch (statName.ToLower())
        {
            case "attack": baseValue = attack; break;
            case "defense": baseValue = defense; break;
            case "accuracy": baseValue = accuracy; break;
            case "critrate": baseValue = critRate; break;
            case "resilience": baseValue = resilience; break;
            case "evasion": baseValue = evasion; break;
            default: return 0;
        }
        
        // 應用臨時加成
        if (temporaryBonuses.ContainsKey(statName))
            baseValue += temporaryBonuses[statName];
        
        // 應用臨時減益
        if (temporaryPenalties.ContainsKey(statName))
            baseValue -= temporaryPenalties[statName];
        
        return baseValue;
    }
    
    /// <summary>
    /// 獲取攻擊傷害（根據攻擊類型）
    /// </summary>
    public int GetAttackDamage(string attackType)
    {
        return attackType.ToLower() switch
        {
            "light" => lightAttackDamage,
            "medium" => mediumAttackDamage,
            "heavy" => heavyAttackDamage,
            _ => 0
        };
    }
    
    /// <summary>
    /// 獲取攻擊命中率（根據攻擊類型，包含屬性計算）
    /// </summary>
    public int GetAttackAccuracy(string attackType)
    {
        int baseAccuracy = attackType.ToLower() switch
        {
            "light" => lightAttackAccuracy,
            "medium" => mediumAttackAccuracy,
            "heavy" => heavyAttackAccuracy,
            _ => 0
        };
        
        return Mathf.Clamp(baseAccuracy + GetFinalStat("accuracy"), 5, 95);
    }
    
    /// <summary>
    /// 獲取攻擊暴擊率（根據攻擊類型，包含屬性計算）
    /// </summary>
    public int GetAttackCritRate(string attackType)
    {
        int baseCritRate = attackType.ToLower() switch
        {
            "light" => lightAttackCritRate,
            "medium" => mediumAttackCritRate,
            "heavy" => heavyAttackCritRate,
            _ => 0
        };
        
        return Mathf.Clamp(baseCritRate + GetFinalStat("critrate"), 5, 95);
    }
    
    /// <summary>
    /// 承受傷害計算（考慮防禦和護盾）
    /// </summary>
    public void TakeDamage(int rawDamage, bool isTrueDamage = false)
    {
        int finalDamage = isTrueDamage ? rawDamage : Mathf.Max(1, rawDamage - GetFinalStat("defense"));
        
        // 先扣除護盾
        if (shield > 0)
        {
            int shieldDamage = Mathf.Min(shield, finalDamage);
            shield -= shieldDamage;
            finalDamage -= shieldDamage;
        }
        
        // 剩餘傷害扣除生命值
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isAlive = false;
            }
        }
    }
    
    /// <summary>
    /// 治療實體
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    /// <summary>
    /// 增加護盾
    /// </summary>
    public void AddShield(int amount)
    {
        shield += amount;
    }
    
    /// <summary>
    /// 每回合開始時調用（恢復生命等）
    /// </summary>
    public void OnTurnStart()
    {
        Heal(healthRegenPerTurn);
    }
    
    /// <summary>
    /// 戰鬥勝利時調用
    /// </summary>
    public void OnVictory()
    {
        Heal(healthOnVictory);
        maxHealth += maxHealthIncreaseOnVictory;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// 重置臨時狀態（每場戰鬥開始時調用）
    /// </summary>
    public void ResetTemporaryStats()
    {
        temporaryBonuses.Clear();
        temporaryPenalties.Clear();
        shield = 0; // 根據規則，護盾不跨回合保留
    }
    
    /// <summary>
    /// 添加臨時加成
    /// </summary>
    public void AddTemporaryBonus(string statName, int value)
    {
        if (temporaryBonuses.ContainsKey(statName))
            temporaryBonuses[statName] += value;
        else
            temporaryBonuses[statName] = value;
    }
    
    /// <summary>
    /// 添加臨時減益
    /// </summary>
    public void AddTemporaryPenalty(string statName, int value)
    {
        if (temporaryPenalties.ContainsKey(statName))
            temporaryPenalties[statName] += value;
        else
            temporaryPenalties[statName] = value;
    }
    
    /// <summary>
    /// 轉換為字典格式（用於UI顯示）
    /// </summary>
    public Dictionary<string, int> ToDictionary()
    {
        return new Dictionary<string, int>
        {
            { "Attack", GetFinalStat("attack") },
            { "Defense", GetFinalStat("defense") },
            { "Accuracy", GetFinalStat("accuracy") },
            { "CritRate", GetFinalStat("critrate") },
            { "Resilience", GetFinalStat("resilience") },
            { "Evasion", GetFinalStat("evasion") }
        };
    }
    
    /// <summary>
    /// 複製當前狀態（用於創建快照）
    /// </summary>
    public EntityStats Clone()
    {
        return (EntityStats)this.MemberwiseClone();
    }
}