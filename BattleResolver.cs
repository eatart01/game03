using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 戰鬥解析器 - 處理所有戰鬥計算和邏輯
/// </summary>
public class BattleResolver : MonoBehaviour
{
    // 單例模式
    public static BattleResolver Instance { get; private set; }
    
    // 戰鬥事件
    public System.Action<BattleResult> OnBattleResolved;
    public System.Action<string> OnCombatLog; // 戰鬥日誌事件
    
    // 當前戰鬥狀態
    private EntityStats currentPlayer;
    private EntityStats currentEnemy;
    private string lastPlayerAction = "";
    private string lastEnemyAction = "";
    private int turnCount = 0;
    
    // 連擊追蹤
    private int lightAttackCount = 0;
    private int mediumAttackCount = 0;
    private int heavyAttackCount = 0;
    private List<string> attackSequence = new List<string>();
    
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
    }
    
    /// <summary>
    /// 開始新的戰鬥
    /// </summary>
    public void StartBattle(EntityStats player, EntityStats enemy)
    {
        currentPlayer = player;
        currentEnemy = enemy;
        turnCount = 0;
        lightAttackCount = 0;
        mediumAttackCount = 0;
        heavyAttackCount = 0;
        attackSequence.Clear();
        
        // 重置臨時狀態
        currentPlayer.ResetTemporaryStats();
        currentEnemy.ResetTemporaryStats();
        
        AddCombatLog($"開始與 {currentEnemy.entityName} 的戰鬥");
    }
    
    /// <summary>
    /// 處理玩家行動
    /// </summary>
    public void ResolvePlayerAction(string attackType)
    {
        turnCount++;
        lastPlayerAction = attackType;
        
        // 更新連擊計數
        UpdateComboCounters(attackType);
        
        // 解析攻擊
        AttackResult playerAttackResult = ResolveAttack(currentPlayer, currentEnemy, attackType, true);
        
        // 檢查天賦觸發
        CheckTalentTriggers(playerAttackResult);
        
        // 檢查敵人是否死亡
        if (!currentEnemy.isAlive)
        {
            BattleResult victoryResult = new BattleResult
            {
                isVictory = true,
                playerFinalStats = currentPlayer.Clone(),
                enemyFinalStats = currentEnemy.Clone(),
                combatLog = GetCombatLog()
            };
            
            OnBattleResolved?.Invoke(victoryResult);
            return;
        }
        
        // 敵人回合
        ResolveEnemyAction();
    }
    
    /// <summary>
    /// 處理敵人行動
    /// </summary>
    private void ResolveEnemyAction()
    {
        // 簡單的AI：隨機選擇攻擊類型
        string[] attackTypes = { "light", "medium", "heavy" };
        string enemyAttackType = attackTypes[Random.Range(0, attackTypes.Length)];
        lastEnemyAction = enemyAttackType;
        
        AttackResult enemyAttackResult = ResolveAttack(currentEnemy, currentPlayer, enemyAttackType, false);
        
        // 檢查玩家是否死亡
        if (!currentPlayer.isAlive)
        {
            BattleResult defeatResult = new BattleResult
            {
                isVictory = false,
                playerFinalStats = currentPlayer.Clone(),
                enemyFinalStats = currentEnemy.Clone(),
                combatLog = GetCombatLog()
            };
            
            OnBattleResolved?.Invoke(defeatResult);
        }
    }
    
    /// <summary>
    /// 解析單次攻擊
    /// </summary>
    private AttackResult ResolveAttack(EntityStats attacker, EntityStats defender, string attackType, bool isPlayerAttacking)
    {
        AttackResult result = new AttackResult
        {
            attackType = attackType,
            isPlayerAttacking = isPlayerAttacking,
            baseDamage = attacker.GetAttackDamage(attackType),
            baseAccuracy = attacker.GetAttackAccuracy(attackType),
            baseCritRate = attacker.GetAttackCritRate(attackType)
        };
        
        // 計算命中率（考慮防守方閃避）
        int finalAccuracy = Mathf.Clamp(result.baseAccuracy - defender.GetFinalStat("evasion"), 5, 95);
        result.isHit = Random.Range(0, 100) < finalAccuracy;
        
        // 計算暴擊率（考慮防守方堅韌）
        int finalCritRate = Mathf.Clamp(result.baseCritRate - defender.GetFinalStat("resilience"), 5, 95);
        result.isCritical = result.isHit && Random.Range(0, 100) < finalCritRate;
        
        // 計算最終傷害
        if (result.isHit)
        {
            result.finalDamage = result.isCritical ? 
                Mathf.CeilToInt(result.baseDamage * 1.5f) : result.baseDamage;
            
            // 應用攻擊力加成
            result.finalDamage += attacker.GetFinalStat("attack");
            
            // 造成傷害
            defender.TakeDamage(result.finalDamage);
        }
        else
        {
            // 未命中時造成1點傷害
            result.finalDamage = 1;
            defender.TakeDamage(1, true);
        }
        
        // 記錄戰鬥日誌
        string attackerName = isPlayerAttacking ? "玩家" : currentEnemy.entityName;
        string defenderName = isPlayerAttacking ? currentEnemy.entityName : "玩家";
        string attackName = GetAttackName(attackType);
        
        if (result.isHit)
        {
            string critText = result.isCritical ? "暴擊！" : "";
            AddCombatLog($"{attackerName} 使用{attackName} {critText}對 {defenderName} 造成 {result.finalDamage} 點傷害");
        }
        else
        {
            AddCombatLog($"{attackerName} 的{attackName} 未命中，造成 1 點傷害");
        }
        
        AddCombatLog($"{defenderName} 生命值: {defender.currentHealth}/{defender.maxHealth}");
        
        return result;
    }
    
    /// <summary>
    /// 更新連擊計數器
    /// </summary>
    private void UpdateComboCounters(string attackType)
    {
        attackSequence.Add(attackType);
        
        switch (attackType)
        {
            case "light": 
                lightAttackCount++;
                mediumAttackCount = 0;
                heavyAttackCount = 0;
                break;
            case "medium": 
                mediumAttackCount++;
                lightAttackCount = 0;
                heavyAttackCount = 0;
                break;
            case "heavy": 
                heavyAttackCount++;
                lightAttackCount = 0;
                mediumAttackCount = 0;
                break;
        }
        
        // 限制序列長度
        if (attackSequence.Count > 10)
            attackSequence.RemoveAt(0);
    }
    
    /// <summary>
    /// 檢查天賦觸發
    /// </summary>
    private void CheckTalentTriggers(AttackResult attackResult)
    {
        if (!attackResult.isPlayerAttacking) return;
        
        // 這裡實現各種天賦的觸發檢查
        // 例如：連擊天賦、序列天賦、暴擊天賦等
        
        // 示例：檢查三次輕攻擊連擊
        if (lightAttackCount >= 3 && PlayerProgress.Instance.activatedTalents.Contains("輕擊"))
        {
            AddCombatLog("觸發天賦【輕擊】：追加一次輕攻擊！");
            lightAttackCount = 0;
            // 這裡可以追加一次攻擊
        }
        
        // 示例：檢查輕-中-重序列
        if (attackSequence.Count >= 3 && 
            attackSequence[attackSequence.Count - 3] == "light" &&
            attackSequence[attackSequence.Count - 2] == "medium" && 
            attackSequence[attackSequence.Count - 1] == "heavy" &&
            PlayerProgress.Instance.activatedTalents.Contains("記憶甦醒...草原戰士"))
        {
            AddCombatLog("觸發天賦【記憶甦醒...草原戰士】：追加一次額外攻擊！");
            // 觸發額外攻擊邏輯
        }
    }
    
    /// <summary>
    /// 獲取攻擊類型的中文名稱
    /// </summary>
    private string GetAttackName(string attackType)
    {
        return attackType switch
        {
            "light" => "輕攻擊",
            "medium" => "中攻擊",
            "heavy" => "重攻擊",
            _ => "攻擊"
        };
    }
    
    /// <summary>
    /// 添加戰鬥日誌
    /// </summary>
    private void AddCombatLog(string message)
    {
        OnCombatLog?.Invoke(message);
        Debug.Log($"戰鬥日誌: {message}");
    }
    
    /// <summary>
    /// 獲取完整的戰鬥日誌
    /// </summary>
    private string GetCombatLog()
    {
        // 這裡可以返回格式化的戰鬥日誌
        return "戰鬥日誌記錄";
    }
    
    /// <summary>
    /// 預測敵人下次攻擊（用於惡性交易天賦）
    /// </summary>
    public string PredictEnemyNextAttack()
    {
        if (!PlayerProgress.Instance.hasMaliciousTrade)
            return "未知";
        
        // 簡單預測：根據當前回合數選擇攻擊類型
        string[] attacks = { "輕攻擊", "中攻擊", "重攻擊" };
        return attacks[turnCount % 3];
    }
    
    /// <summary>
    /// 強制結束戰鬥（用於測試或特殊情況）
    /// </summary>
    public void ForceEndBattle(bool playerVictory)
    {
        BattleResult result = new BattleResult
        {
            isVictory = playerVictory,
            playerFinalStats = currentPlayer.Clone(),
            enemyFinalStats = currentEnemy.Clone(),
            combatLog = "戰鬥被強制結束"
        };
        
        OnBattleResolved?.Invoke(result);
    }
}

/// <summary>
/// 單次攻擊結果數據結構
/// </summary>
public struct AttackResult
{
    public string attackType;
    public bool isPlayerAttacking;
    public int baseDamage;
    public int baseAccuracy;
    public int baseCritRate;
    public bool isHit;
    public bool isCritical;
    public int finalDamage;
}

/// <summary>
/// 戰鬥最終結果數據結構
/// </summary>
public struct BattleResult
{
    public bool isVictory;
    public EntityStats playerFinalStats;
    public EntityStats enemyFinalStats;
    public string combatLog;
}