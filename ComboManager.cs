using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 連擊管理器 - 處理連擊系統、技能序列和特殊效果組合
/// </summary>
public class ComboManager : MonoBehaviour
{
    // 單例模式
    public static ComboManager Instance { get; private set; }
    
    [Header("連擊設置")]
    public int maxSequenceLength = 10; // 最大序列記錄長度
    public float comboTimeWindow = 3.0f; // 連擊時間窗口（秒）
    
    // 連擊追蹤
    private List<string> attackSequence = new List<string>();
    private Dictionary<string, int> comboCounters = new Dictionary<string, int>();
    private Dictionary<string, float> comboTimers = new Dictionary<string, float>();
    private Dictionary<string, int> talentTriggerCounters = new Dictionary<string, int>();
    
    // 當前戰鬥數據
    private int currentEnemyID;
    private bool inCombat = false;
    
    // 事件
    public System.Action<string> OnComboTriggered; // 連擊觸發事件
    public System.Action<string> OnSequenceCompleted; // 序列完成事件
    public System.Action<string, int> OnComboCounterUpdated; // 連擊計數更新
    
    // 天賦冷卻追蹤
    private Dictionary<string, int> talentCooldowns = new Dictionary<string, int>();
    private Dictionary<string, bool> talentGlobalCooldowns = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeComboSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (inCombat)
        {
            UpdateComboTimers();
        }
    }
    
    /// <summary>
    /// 初始化連擊系統
    /// </summary>
    private void InitializeComboSystem()
    {
        // 初始化連擊計數器
        comboCounters = new Dictionary<string, int>
        {
            { "light", 0 },
            { "medium", 0 },
            { "heavy", 0 }
        };
        
        comboTimers = new Dictionary<string, float>
        {
            { "light", 0f },
            { "medium", 0f },
            { "heavy", 0f }
        };
        
        attackSequence = new List<string>();
        talentTriggerCounters = new Dictionary<string, int>();
        talentCooldowns = new Dictionary<string, int>();
        talentGlobalCooldowns = new Dictionary<string, bool>();
        
        Debug.Log("連擊系統初始化完成");
    }
    
    /// <summary>
    /// 戰鬥開始
    /// </summary>
    public void OnCombatStart(int enemyID)
    {
        inCombat = true;
        currentEnemyID = enemyID;
        ResetCombatCounters();
        
        Debug.Log($"連擊系統: 開始與敵人{enemyID}的戰鬥");
    }
    
    /// <summary>
    /// 戰鬥結束
    /// </summary>
    public void OnCombatEnd(bool victory)
    {
        inCombat = false;
        UpdateTalentCooldowns(true);
        
        Debug.Log($"連擊系統: 戰鬥結束，{(victory ? "勝利" : "失敗")}");
    }
    
    /// <summary>
    /// 重置戰鬥計數器
    /// </summary>
    private void ResetCombatCounters()
    {
        foreach (string key in comboCounters.Keys.ToList())
        {
            comboCounters[key] = 0;
            comboTimers[key] = 0f;
        }
        
        attackSequence.Clear();
        talentTriggerCounters.Clear();
    }
    
    /// <summary>
    /// 更新連擊計時器
    /// </summary>
    private void UpdateComboTimers()
    {
        foreach (string attackType in comboCounters.Keys.ToList())
        {
            if (comboTimers[attackType] > 0)
            {
                comboTimers[attackType] -= Time.deltaTime;
                if (comboTimers[attackType] <= 0)
                {
                    comboCounters[attackType] = 0;
                    OnComboCounterUpdated?.Invoke(attackType, 0);
                    Debug.Log($"{GetAttackDisplayName(attackType)}連擊超時重置");
                }
            }
        }
    }
    
    /// <summary>
    /// 記錄攻擊並檢查連擊
    /// </summary>
    public void RecordAttack(string attackType, bool isHit, bool isCritical)
    {
        if (!inCombat) return;
        
        // 更新連擊計數器
        comboCounters[attackType]++;
        comboTimers[attackType] = comboTimeWindow;
        
        // 更新攻擊序列
        attackSequence.Add(attackType);
        if (attackSequence.Count > maxSequenceLength)
        {
            attackSequence.RemoveAt(0);
        }
        
        // 通知計數更新
        OnComboCounterUpdated?.Invoke(attackType, comboCounters[attackType]);
        
        // 檢查連擊觸發
        CheckComboTriggers(attackType, isHit, isCritical);
        
        // 檢查序列觸發
        CheckSequenceTriggers();
        
        // 檢查天賦觸發條件
        CheckTalentTriggers(attackType, isHit, isCritical);
        
        Debug.Log($"{GetAttackDisplayName(attackType)}連擊: {comboCounters[attackType]}");
    }
    
    /// <summary>
    /// 檢查連擊觸發
    /// </summary>
    private void CheckComboTriggers(string attackType, bool isHit, bool isCritical)
    {
        int comboCount = comboCounters[attackType];
        
        // 檢查三次連擊天賦
        if (comboCount >= 3)
        {
            switch (attackType)
            {
                case "light" when CanTriggerTalent("輕擊"):
                    TriggerLightAttackCombo();
                    break;
                    
                case "medium" when CanTriggerTalent("破碎追擊"):
                    TriggerMediumAttackCombo();
                    break;
                    
                case "heavy" when CanTriggerTalent("終結連擊"):
                    TriggerHeavyAttackCombo();
                    break;
            }
        }
    }
    
    /// <summary>
    /// 檢查序列觸發
    /// </summary>
    private void CheckSequenceTriggers()
    {
        if (attackSequence.Count < 3) return;
        
        // 獲取最近三次攻擊
        string[] recentAttacks = attackSequence.Skip(attackSequence.Count - 3).ToArray();
        
        // 檢查特定序列模式
        CheckSequencePattern(recentAttacks);
    }
    
    /// <summary>
    /// 檢查序列模式
    /// </summary>
    private void CheckSequencePattern(string[] sequence)
    {
        string sequenceKey = string.Join("-", sequence);
        
        // 輕-中-重序列
        if (sequence[0] == "light" && sequence[1] == "medium" && sequence[2] == "heavy" &&
            CanTriggerTalent("記憶甦醒...草原戰士"))
        {
            TriggerSequenceCombo("草原戰士", sequenceKey);
            return;
        }
        
        // 重-中-輕序列
        if (sequence[0] == "heavy" && sequence[1] == "medium" && sequence[2] == "light" &&
            CanTriggerTalent("戰鬥自癒"))
        {
            TriggerSequenceCombo("戰鬥自癒", sequenceKey);
            return;
        }
        
        // 輕-中-輕序列（游離夢境）
        if (sequence[0] == "light" && sequence[1] == "medium" && sequence[2] == "light" &&
            CanTriggerTalent("游離夢境"))
        {
            TriggerSequenceCombo("游離夢境", sequenceKey);
            return;
        }
        
        // 中-輕-中序列（罪與罰的寂滅天使）
        if (sequence[0] == "medium" && sequence[1] == "light" && sequence[2] == "medium" &&
            CanTriggerTalent("罪與罰的寂滅天使"))
        {
            TriggerSequenceCombo("寂滅天使", sequenceKey);
            return;
        }
        
        // 其他序列模式...
    }
    
    /// <summary>
    /// 觸發輕攻擊連擊
    /// </summary>
    private void TriggerLightAttackCombo()
    {
        if (!CanTriggerTalent("輕擊", 2)) return; // 每場戰鬥限2次
        
        Debug.Log("觸發輕攻擊連擊天賦");
        comboCounters["light"] = 0;
        
        // 這裡實現具體的連擊效果
        BattleResolver.Instance.ForceAdditionalAttack("light");
        
        // 觸發事件
        OnComboTriggered?.Invoke("輕擊");
        IncrementTalentTriggerCount("輕擊");
    }
    
    /// <summary>
    /// 觸發中攻擊連擊
    /// </summary>
    private void TriggerMediumAttackCombo()
    {
        if (!CanTriggerTalent("破碎追擊", 2)) return;
        
        Debug.Log("觸發中攻擊連擊天賦");
        comboCounters["medium"] = 0;
        
        // 實現中攻擊連擊效果
        OnComboTriggered?.Invoke("破碎追擊");
        IncrementTalentTriggerCount("破碎追擊");
    }
    
    /// <summary>
    /// 觸發重攻擊連擊
    /// </summary>
    private void TriggerHeavyAttackCombo()
    {
        if (!CanTriggerTalent("終結連擊", 2)) return;
        
        Debug.Log("觸發重攻擊連擊天賦");
        comboCounters["heavy"] = 0;
        
        // 實現重攻擊連擊效果
        OnComboTriggered?.Invoke("終結連擊");
        IncrementTalentTriggerCount("終結連擊");
    }
    
    /// <summary>
    /// 觸發序列連擊
    /// </summary>
    private void TriggerSequenceCombo(string talentName, string sequenceKey)
    {
        if (!CanTriggerTalent(talentName, 1)) return; // 強力技能通常每場戰鬥限1次
        
        Debug.Log($"觸發序列連擊: {talentName} (序列: {sequenceKey})");
        
        // 根據天賦名稱觸發不同效果
        switch (talentName)
        {
            case "草原戰士":
                ExecuteGrasslandWarriorCombo();
                break;
                
            case "戰鬥自癒":
                ExecuteBattleHealCombo();
                break;
                
            case "游離夢境":
                ExecuteDreamingCombo();
                break;
                
            case "寂滅天使":
                ExecuteAngelCombo();
                break;
        }
        
        // 重置序列以避免重複觸發
        attackSequence.Clear();
        
        OnSequenceCompleted?.Invoke(talentName);
        IncrementTalentTriggerCount(talentName);
        
        // 設置冷卻
        SetTalentCooldown(talentName, 3); // 強力技能冷卻3場戰鬥
    }
    
    /// <summary>
    /// 執行草原戰士連擊
    /// </summary>
    private void ExecuteGrasslandWarriorCombo()
    {
        // 實現輕-中-重序列效果
        EntityStats enemy = BattleFlowManager.Instance.GetCurrentEnemy();
        if (enemy != null)
        {
            // 敵人命中率永久-10
            enemy.accuracy = Mathf.Max(5, enemy.accuracy - 10);
            Debug.Log("敵人命中率永久-10");
        }
        
        // 追加一次額外攻擊
        BattleResolver.Instance.ForceAdditionalAttack("combo");
    }
    
    /// <summary>
    /// 執行戰鬥自癒連擊
    /// </summary>
    private void ExecuteBattleHealCombo()
    {
        // 實現重-中-輕序列效果
        EntityStats player = PlayerProgress.Instance.playerStats;
        player.Heal(50);
        Debug.Log("恢復50點生命");
    }
    
    /// <summary>
    /// 執行游離夢境連擊
    /// </summary>
    private void ExecuteDreamingCombo()
    {
        // 實現輕-中-輕序列效果
        Debug.Log("觸發游離夢境強力技能");
        // 這裡應該實現具體的傷害效果
    }
    
    /// <summary>
    /// 執行寂滅天使連擊
    /// </summary>
    private void ExecuteAngelCombo()
    {
        // 實現中-輕-中序列效果
        Debug.Log("觸發寂滅天使強力技能");
        // 這裡應該實現具體的傷害效果
    }
    
    /// <summary>
    /// 檢查天賦觸發條件
    /// </summary>
    private void CheckTalentTriggers(string attackType, bool isHit, bool isCritical)
    {
        // 檢查命中且暴擊的天賦
        if (isHit && isCritical)
        {
            CheckHitAndCritTalents(attackType);
        }
        
        // 檢查連續命中的天賦
        if (isHit)
        {
            CheckConsecutiveHitTalents(attackType);
        }
        
        // 檢查連續未命中的天賦
        if (!isHit)
        {
            CheckConsecutiveMissTalents();
        }
    }
    
    /// <summary>
    /// 檢查命中且暴擊的天賦
    /// </summary>
    private void CheckHitAndCritTalents(string attackType)
    {
        switch (attackType)
        {
            case "light" when CanTriggerTalent("記憶甦醒...輕攻擊學徒"):
                // 輕攻擊命中且暴擊兩次
                if (GetConsecutiveHitAndCritCount("light") >= 2)
                {
                    TriggerHitAndCritTalent("輕攻擊學徒", "light");
                }
                break;
                
            case "medium" when CanTriggerTalent("機會尋覓者"):
                if (GetConsecutiveHitAndCritCount("medium") >= 2)
                {
                    TriggerHitAndCritTalent("機會尋覓者", "medium");
                }
                break;
                
            case "heavy" when CanTriggerTalent("記憶甦醒...撕裂愛好者"):
                if (GetConsecutiveHitAndCritCount("heavy") >= 2)
                {
                    TriggerHitAndCritTalent("撕裂愛好者", "heavy");
                }
                break;
        }
    }
    
    /// <summary>
    /// 觸發命中暴擊天賦
    /// </summary>
    private void TriggerHitAndCritTalent(string talentName, string attackType)
    {
        Debug.Log($"觸發{attackType}命中暴擊天賦: {talentName}");
        
        // 根據天賦提供永久加成
        switch (talentName)
        {
            case "輕攻擊學徒":
                PlayerProgress.Instance.playerStats.accuracy += 1;
                PlayerProgress.Instance.playerStats.evasion += 1;
                break;
                
            case "機會尋覓者":
                PlayerProgress.Instance.playerStats.resilience += 1;
                PlayerProgress.Instance.playerStats.maxHealth += 1;
                break;
                
            case "撕裂愛好者":
                PlayerProgress.Instance.playerStats.attack += 1;
                PlayerProgress.Instance.playerStats.critRate += 1;
                break;
        }
        
        IncrementTalentTriggerCount(talentName);
    }
    
    /// <summary>
    /// 檢查是否可以觸發天賦
    /// </summary>
    private bool CanTriggerTalent(string talentName, int maxTriggers = int.MaxValue)
    {
        // 檢查天賦是否激活
        if (!PlayerProgress.Instance.activatedTalents.Contains(talentName))
            return false;
        
        // 檢查冷卻
        if (IsTalentOnCooldown(talentName))
            return false;
        
        // 檢查觸發次數限制
        if (GetTalentTriggerCount(talentName) >= maxTriggers)
            return false;
        
        // 檢查全局冷卻
        if (talentGlobalCooldowns.ContainsKey(talentName) && talentGlobalCooldowns[talentName])
            return false;
        
        return true;
    }
    
    /// <summary>
    /// 獲取天賦觸發次數
    /// </summary>
    private int GetTalentTriggerCount(string talentName)
    {
        return talentTriggerCounters.ContainsKey(talentName) ? talentTriggerCounters[talentName] : 0;
    }
    
    /// <summary>
    /// 增加天賦觸發次數
    /// </summary>
    private void IncrementTalentTriggerCount(string talentName)
    {
        if (talentTriggerCounters.ContainsKey(talentName))
        {
            talentTriggerCounters[talentName]++;
        }
        else
        {
            talentTriggerCounters[talentName] = 1;
        }
    }
    
    /// <summary>
    /// 設置天賦冷卻
    /// </summary>
    private void SetTalentCooldown(string talentName, int cooldownBattles)
    {
        talentCooldowns[talentName] = cooldownBattles;
    }
    
    /// <summary>
    /// 檢查天賦是否在冷卻中
    /// </summary>
    private bool IsTalentOnCooldown(string talentName)
    {
        return talentCooldowns.ContainsKey(talentName) && talentCooldowns[talentName] > 0;
    }
    
    /// <summary>
    /// 更新天賦冷卻
    /// </summary>
    public void UpdateTalentCooldowns(bool combatEnded = false)
    {
        foreach (string talentName in talentCooldowns.Keys.ToList())
        {
            if (talentCooldowns[talentName] > 0)
            {
                talentCooldowns[talentName]--;
            }
        }
        
        if (combatEnded)
        {
            // 重置每場戰鬥的觸發次數
            talentTriggerCounters.Clear();
            talentGlobalCooldowns.Clear();
        }
    }
    
    /// <summary>
    /// 獲取攻擊顯示名稱
    /// </summary>
    private string GetAttackDisplayName(string attackType)
    {
        return attackType switch
        {
            "light" => "輕攻擊",
            "medium" => "中攻擊",
            "heavy" => "重攻擊",
            _ => attackType
        };
    }
    
    /// <summary>
    /// 獲取連續命中暴擊次數
    /// </summary>
    private int GetConsecutiveHitAndCritCount(string attackType)
    {
        // 這裡實現具體的連續命中暴擊追蹤邏輯
        return 0;
    }
    
    /// <summary>
    /// 檢查連續命中天賦
    /// </summary>
    private void CheckConsecutiveHitTalents(string attackType)
    {
        // 實現連續命中檢查
    }
    
    /// <summary>
    /// 檢查連續未命中天賦
    /// </summary>
    private void CheckConsecutiveMissTalents()
    {
        // 實現連續未命中檢查（如理智斷裂）
    }
    
    /// <summary>
    /// 獲取當前連擊數
    /// </summary>
    public int GetComboCount(string attackType)
    {
        return comboCounters.ContainsKey(attackType) ? comboCounters[attackType] : 0;
    }
    
    /// <summary>
    /// 獲取當前攻擊序列
    /// </summary>
    public List<string> GetAttackSequence()
    {
        return new List<string>(attackSequence);
    }
    
    /// <summary>
    /// 清除當前連擊
    /// </summary>
    public void ClearCombo(string attackType)
    {
        if (comboCounters.ContainsKey(attackType))
        {
            comboCounters[attackType] = 0;
            comboTimers[attackType] = 0f;
        }
    }
    
    /// <summary>
    /// 重置所有連擊
    /// </summary>
    public void ResetAllCombos()
    {
        foreach (string key in comboCounters.Keys.ToList())
        {
            comboCounters[key] = 0;
            comboTimers[key] = 0f;
        }
    }
}