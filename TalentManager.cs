using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 天賦管理器 - 處理天賦的解鎖、激活和效果管理
/// </summary>
public class TalentManager : MonoBehaviour
{
    // 單例模式
    public static TalentManager Instance { get; private set; }
    
    [Header("天賦設置")]
    public List<TalentData> allTalents = new List<TalentData>();
    public int talentsUnlockedPerPoint = 5; // 每次獲得天賦點解鎖的天賦數量
    
    // 天賦數據
    private List<TalentData> availableTalents = new List<TalentData>(); // 可選天賦池
    private Dictionary<string, TalentData> talentDictionary = new Dictionary<string, TalentData>();
    
    // 事件
    public System.Action<List<TalentData>> OnNewTalentsUnlocked;
    public System.Action<TalentData> OnTalentActivated;
    public System.Action OnTalentsUpdated;
    
    // 冷卻追蹤
    private Dictionary<string, int> talentCooldowns = new Dictionary<string, int>();
    private Dictionary<string, int> combatCounters = new Dictionary<string, int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTalentSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化天賦系統
    /// </summary>
    private void InitializeTalentSystem()
    {
        CreateTalentDatabase();
        InitializeDictionaries();
        
        Debug.Log($"天賦系統初始化完成，共{allTalents.Count}個天賦");
    }
    
    /// <summary>
    /// 創建天賦數據庫
    /// </summary>
    private void CreateTalentDatabase()
    {
        // 這裡創建所有60個天賦的數據
        allTalents = new List<TalentData>
        {
            // 前20個天賦
            new TalentData
            {
                id = "talent_01",
                name = "強制招架",
                description = "+2防禦，承受攻擊時20%機率抵銷所有傷害（每場戰鬥最多兩次）",
                requirements = "",
                isActive = false,
                isUnlocked = false,
                talentType = TalentType.Passive,
                cooldown = 0
            },
            new TalentData
            {
                id = "talent_02",
                name = "超感知追擊",
                description = "+2攻擊，每次攻擊時20%機率再額外攻擊一次（每場戰鬥最多兩次）",
                requirements = "",
                isActive = false,
                isUnlocked = false,
                talentType = TalentType.Passive,
                cooldown = 0
            },
            // 這裡應該繼續添加所有60個天賦...
            // 為節省空間，這裡只顯示結構，實際需要完整添加
            
            // 第50個天賦 - 記憶重鑄
            new TalentData
            {
                id = "talent_50",
                name = "記憶重鑄",
                description = "+3攻擊 +3防禦 +20最大生命，捨棄兩個已激活的天賦學習兩個新的天賦（只能使用一次）",
                requirements = "",
                isActive = false,
                isUnlocked = false,
                talentType = TalentType.Special,
                cooldown = 0,
                isOneTimeUse = true
            }
        };
        
        // 創建字典以便快速查找
        foreach (var talent in allTalents)
        {
            talentDictionary[talent.id] = talent;
        }
    }
    
    /// <summary>
    /// 初始化計數器字典
    /// </summary>
    private void InitializeDictionaries()
    {
        // 初始化冷卻計數器
        foreach (var talent in allTalents.Where(t => t.cooldown > 0))
        {
            talentCooldowns[talent.id] = 0;
        }
        
        // 初始化戰鬥計數器
        combatCounters = new Dictionary<string, int>();
    }
    
    /// <summary>
    /// 獲得天賦點時解鎖新天賦
    /// </summary>
    public List<TalentData> UnlockNewTalents()
    {
        if (PlayerProgress.Instance.totalTalentPointsEarned >= 12)
        {
            Debug.Log("所有天賦已解鎖完畢");
            return new List<TalentData>();
        }
        
        // 獲取尚未解鎖的天賦
        var lockedTalents = allTalents.Where(t => !t.isUnlocked).ToList();
        
        // 隨機選擇5個天賦
        var newTalents = new List<TalentData>();
        for (int i = 0; i < Mathf.Min(talentsUnlockedPerPoint, lockedTalents.Count); i++)
        {
            int randomIndex = Random.Range(0, lockedTalents.Count);
            TalentData selectedTalent = lockedTalents[randomIndex];
            
            selectedTalent.isUnlocked = true;
            lockedTalents.RemoveAt(randomIndex);
            newTalents.Add(selectedTalent);
            
            Debug.Log($"解鎖天賦: {selectedTalent.name}");
        }
        
        // 添加到可選池
        availableTalents.AddRange(newTalents);
        
        OnNewTalentsUnlocked?.Invoke(newTalents);
        OnTalentsUpdated?.Invoke();
        
        return newTalents;
    }
    
    /// <summary>
    /// 激活天賦
    /// </summary>
    public bool ActivateTalent(string talentId)
    {
        if (!talentDictionary.ContainsKey(talentId))
        {
            Debug.LogError($"找不到天賦: {talentId}");
            return false;
        }
        
        TalentData talent = talentDictionary[talentId];
        
        // 檢查條件
        if (!talent.isUnlocked || talent.isActive || !CanAffordTalent(talent))
        {
            Debug.Log($"無法激活天賦 {talent.name}");
            return false;
        }
        
        // 特殊處理：記憶重鑄
        if (talentId == "talent_50" && talent.isOneTimeUse && talent.hasBeenUsed)
        {
            Debug.Log("記憶重鑄只能使用一次");
            return false;
        }
        
        // 消耗天賦點
        PlayerProgress.Instance.availableTalentPoints--;
        talent.isActive = true;
        
        // 應用天賦效果
        ApplyTalentEffects(talent);
        
        // 添加到已激活列表
        PlayerProgress.Instance.activatedTalents.Add(talentId);
        
        OnTalentActivated?.Invoke(talent);
        OnTalentsUpdated?.Invoke();
        
        Debug.Log($"激活天賦: {talent.name}");
        return true;
    }
    
    /// <summary>
    /// 檢查是否可以激活天賦
    /// </summary>
    private bool CanAffordTalent(TalentData talent)
    {
        return PlayerProgress.Instance.availableTalentPoints > 0;
    }
    
    /// <summary>
    /// 應用天賦效果
    /// </summary>
    private void ApplyTalentEffects(TalentData talent)
    {
        EntityStats playerStats = PlayerProgress.Instance.playerStats;
        
        // 這裡根據天賦ID應用不同的效果
        switch (talent.id)
        {
            case "talent_01": // 強制招架
                playerStats.defense += 2;
                break;
                
            case "talent_02": // 超感知追擊
                playerStats.attack += 2;
                break;
                
            case "talent_03": // 結痂
                playerStats.resilience += 2;
                break;
                
            case "talent_50": // 記憶重鑄
                playerStats.attack += 3;
                playerStats.defense += 3;
                playerStats.maxHealth += 20;
                playerStats.Heal(20);
                break;
                
            // 其他天賦效果...
        }
        
        // 特殊天賦標記
        if (talent.id == "talent_36") // 惡性交易
        {
            PlayerProgress.Instance.hasMaliciousTrade = true;
        }
    }
    
    /// <summary>
    /// 移除天賦效果
    /// </summary>
    private void RemoveTalentEffects(TalentData talent)
    {
        EntityStats playerStats = PlayerProgress.Instance.playerStats;
        
        switch (talent.id)
        {
            case "talent_01": // 強制招架
                playerStats.defense -= 2;
                break;
                
            case "talent_02": // 超感知追擊
                playerStats.attack -= 2;
                break;
                
            // 其他天賦移除效果...
        }
    }
    
    /// <summary>
    /// 檢查天賦觸發條件
    /// </summary>
    public void CheckTalentTriggers(string triggerType, object context = null)
    {
        foreach (string activatedTalentId in PlayerProgress.Instance.activatedTalents)
        {
            if (!talentDictionary.ContainsKey(activatedTalentId)) continue;
            
            TalentData talent = talentDictionary[activatedTalentId];
            
            // 檢查冷卻
            if (IsOnCooldown(talent.id)) continue;
            
            // 根據觸發類型檢查天賦
            switch (triggerType)
            {
                case "OnAttack":
                    CheckAttackTalents(talent, context);
                    break;
                    
                case "OnDamageTaken":
                    CheckDamageTalents(talent, context);
                    break;
                    
                case "OnTurnStart":
                    CheckTurnStartTalents(talent);
                    break;
                    
                case "OnCombatStart":
                    CheckCombatStartTalents(talent);
                    break;
                    
                case "OnCardPlayed":
                    CheckCardTalents(talent, context);
                    break;
            }
        }
    }
    
    /// <summary>
    /// 檢查攻擊相關天賦
    /// </summary>
    private void CheckAttackTalents(TalentData talent, object context)
    {
        // 實現攻擊觸發的天賦邏輯
    }
    
    /// <summary>
    /// 檢查傷害相關天賦
    /// </summary>
    private void CheckDamageTalents(TalentData talent, object context)
    {
        // 實現傷害觸發的天賦邏輯
    }
    
    /// <summary>
    /// 檢查回合開始天賦
    /// </summary>
    private void CheckTurnStartTalents(TalentData talent)
    {
        // 實現回合開始觸發的天賦邏輯
    }
    
    /// <summary>
    /// 檢查戰鬥開始天賦
    /// </summary>
    private void CheckCombatStartTalents(TalentData talent)
    {
        // 實現戰鬥開始觸發的天賦邏輯
    }
    
    /// <summary>
    /// 檢查卡牌相關天賦
    /// </summary>
    private void CheckCardTalents(TalentData talent, object context)
    {
        // 實現卡牌觸發的天賦邏輯
    }
    
    /// <summary>
    /// 檢查天賦是否在冷卻中
    /// </summary>
    private bool IsOnCooldown(string talentId)
    {
        return talentCooldowns.ContainsKey(talentId) && talentCooldowns[talentId] > 0;
    }
    
    /// <summary>
    /// 更新天賦冷卻
    /// </summary>
    public void UpdateTalentCooldowns(bool combatEnded = false)
    {
        foreach (string talentId in talentCooldowns.Keys.ToList())
        {
            if (talentCooldowns[talentId] > 0)
            {
                talentCooldowns[talentId]--;
            }
        }
        
        // 如果戰鬥結束，重置戰鬥計數器
        if (combatEnded)
        {
            combatCounters.Clear();
        }
    }
    
    /// <summary>
    /// 獲取可選天賦列表
    /// </summary>
    public List<TalentData> GetAvailableTalents()
    {
        return availableTalents.Where(t => t.isUnlocked && !t.isActive).ToList();
    }
    
    /// <summary>
    /// 獲取已激活天賦列表
    /// </summary>
    public List<TalentData> GetActivatedTalents()
    {
        return availableTalents.Where(t => t.isActive).ToList();
    }
    
    /// <summary>
    /// 執行記憶重鑄特殊效果
    /// </summary>
    public void ExecuteMemoryReforging(List<string> talentsToRemove, List<string> talentsToAdd)
    {
        TalentData memoryReforge = talentDictionary["talent_50"];
        if (!memoryReforge.isActive || memoryReforge.hasBeenUsed)
        {
            Debug.LogError("無法執行記憶重鑄");
            return;
        }
        
        // 移除舊天賦
        foreach (string talentId in talentsToRemove)
        {
            if (talentDictionary.ContainsKey(talentId) && talentDictionary[talentId].isActive)
            {
                RemoveTalentEffects(talentDictionary[talentId]);
                talentDictionary[talentId].isActive = false;
                PlayerProgress.Instance.activatedTalents.Remove(talentId);
            }
        }
        
        // 退還天賦點
        PlayerProgress.Instance.availableTalentPoints += talentsToRemove.Count;
        
        // 添加新天賦
        foreach (string talentId in talentsToAdd)
        {
            if (talentDictionary.ContainsKey(talentId) && talentDictionary[talentId].isUnlocked && !talentDictionary[talentId].isActive)
            {
                ActivateTalent(talentId);
            }
        }
        
        // 標記為已使用
        memoryReforge.hasBeenUsed = true;
        memoryReforge.isActive = false;
        PlayerProgress.Instance.activatedTalents.Remove("talent_50");
        
        Debug.Log("記憶重鑄完成");
    }
    
    /// <summary>
    /// 重置天賦系統（用於新遊戲）
    /// </summary>
    public void ResetTalents()
    {
        foreach (TalentData talent in allTalents)
        {
            talent.isUnlocked = false;
            talent.isActive = false;
            talent.hasBeenUsed = false;
        }
        
        availableTalents.Clear();
        talentCooldowns.Clear();
        combatCounters.Clear();
        
        Debug.Log("天賦系統已重置");
    }
}

/// <summary>
/// 天賦數據結構
/// </summary>
[System.Serializable]
public class TalentData
{
    public string id;
    public string name;
    public string description;
    public string requirements;
    public bool isUnlocked;
    public bool isActive;
    public TalentType talentType;
    public int cooldown; // 冷卻場次
    public bool isOneTimeUse; // 是否只能使用一次
    public bool hasBeenUsed; // 是否已經使用過（針對一次性天賦）
}

/// <summary>
/// 天賦類型枚舉
/// </summary>
public enum TalentType
{
    Passive,    // 被動效果
    Active,     // 主動技能
    Special     // 特殊能力
}

/// <summary>
/// 天賦觸發上下文
/// </summary>
public class TalentTriggerContext
{
    public EntityStats source;
    public EntityStats target;
    public int damage;
    public string attackType;
    public bool isCritical;
    // 其他上下文信息...
}