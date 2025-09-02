using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 卡牌管理器 - 處理玩家牌組、抽牌和手牌管理
/// </summary>
public class DeckManager : MonoBehaviour
{
    // 單例模式
    public static DeckManager Instance { get; private set; }
    
    [Header("牌組設置")]
    public int deckSize = 30; // 牌組總大小
    public int handSize = 3;  // 手牌數量
    
    [Header("卡牌比例")]
    public int lightAttackCount = 15;  // 輕攻擊數量
    public int mediumAttackCount = 10; // 中攻擊數量
    public int heavyAttackCount = 5;   // 重攻擊數量
    
    // 牌組數據
    private List<string> drawPile = new List<string>();    // 抽牌堆
    private List<string> discardPile = new List<string>(); // 棄牌堆
    private List<string> currentHand = new List<string>(); // 當前手牌
    
    // 事件
    public System.Action<List<string>> OnHandUpdated;
    public System.Action OnDeckShuffled;
    public System.Action<string> OnCardDrawn;
    public System.Action<string> OnCardPlayed;
    
    // 天賦效果追蹤
    private Dictionary<string, int> cardPlayCountThisTurn = new Dictionary<string, int>();
    private Dictionary<string, int> cardPlayCountThisCombat = new Dictionary<string, int>();
    
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
        
        InitializeCardDictionaries();
    }
    
    /// <summary>
    /// 初始化卡牌計數器
    /// </summary>
    private void InitializeCardDictionaries()
    {
        cardPlayCountThisTurn = new Dictionary<string, int>
        {
            { "light", 0 },
            { "medium", 0 },
            { "heavy", 0 }
        };
        
        cardPlayCountThisCombat = new Dictionary<string, int>
        {
            { "light", 0 },
            { "medium", 0 },
            { "heavy", 0 }
        };
    }
    
    /// <summary>
    /// 初始化牌組
    /// </summary>
    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        currentHand.Clear();
        
        // 創建基本牌組
        for (int i = 0; i < lightAttackCount; i++) drawPile.Add("light");
        for (int i = 0; i < mediumAttackCount; i++) drawPile.Add("medium");
        for (int i = 0; i < heavyAttackCount; i++) drawPile.Add("heavy");
        
        // 檢查牌組大小
        if (drawPile.Count != deckSize)
        {
            Debug.LogWarning($"牌組大小不匹配！期望: {deckSize}, 實際: {drawPile.Count}");
            // 自動調整
            while (drawPile.Count > deckSize) drawPile.RemoveAt(drawPile.Count - 1);
            while (drawPile.Count < deckSize) drawPile.Add("light");
        }
        
        ShuffleDeck();
        DrawInitialHand();
        
        Debug.Log($"牌組初始化完成: {drawPile.Count}張牌");
    }
    
    /// <summary>
    /// 洗牌
    /// </summary>
    public void ShuffleDeck()
    {
        // Fisher-Yates 洗牌算法
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
        
        OnDeckShuffled?.Invoke();
        Debug.Log("牌組已洗牌");
    }
    
    /// <summary>
    /// 抽取初始手牌
    /// </summary>
    private void DrawInitialHand()
    {
        for (int i = 0; i < handSize; i++)
        {
            if (drawPile.Count == 0)
            {
                ReshuffleDiscardPile();
                if (drawPile.Count == 0) break; // 防止無限循環
            }
            
            string drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            currentHand.Add(drawnCard);
            
            OnCardDrawn?.Invoke(drawnCard);
        }
        
        OnHandUpdated?.Invoke(currentHand);
        Debug.Log($"初始手牌: {string.Join(", ", currentHand)}");
    }
    
    /// <summary>
    /// 玩家使用卡牌
    /// </summary>
    public void PlayCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= currentHand.Count)
        {
            Debug.LogError($"無效的手牌索引: {handIndex}");
            return;
        }
        
        string playedCard = currentHand[handIndex];
        
        // 移除使用的牌
        currentHand.RemoveAt(handIndex);
        discardPile.Add(playedCard);
        
        // 更新計數器
        UpdateCardCounters(playedCard);
        
        // 觸發天賦效果
        TriggerCardPlayEffects(playedCard);
        
        // 抽新牌
        DrawCard();
        
        OnCardPlayed?.Invoke(playedCard);
        OnHandUpdated?.Invoke(currentHand);
        
        Debug.Log($"使用卡牌: {GetCardDisplayName(playedCard)}");
    }
    
    /// <summary>
    /// 抽取一張牌
    /// </summary>
    private void DrawCard()
    {
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
            if (drawPile.Count == 0)
            {
                Debug.Log("無牌可抽");
                return;
            }
        }
        
        string drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        currentHand.Add(drawnCard);
        
        // 觸發抽牌天賦效果
        TriggerCardDrawEffects(drawnCard);
        
        OnCardDrawn?.Invoke(drawnCard);
        Debug.Log($"抽到: {GetCardDisplayName(drawnCard)}");
    }
    
    /// <summary>
    /// 重新洗入棄牌堆
    /// </summary>
    private void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.Log("棄牌堆為空，無法重新洗牌");
            return;
        }
        
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
        
        Debug.Log($"已將{discardPile.Count}張牌從棄牌堆洗回抽牌堆");
    }
    
    /// <summary>
    /// 更新卡牌計數器
    /// </summary>
    private void UpdateCardCounters(string cardType)
    {
        // 更新本回合計數
        cardPlayCountThisTurn[cardType]++;
        
        // 更新本場戰鬥計數
        cardPlayCountThisCombat[cardType]++;
        
        // 檢查連擊天賦
        CheckComboTalents(cardType);
    }
    
    /// <summary>
    /// 檢查連擊天賦
    /// </summary>
    private void CheckComboTalents(string cardType)
    {
        // 檢查三次輕攻擊
        if (cardType == "light" && cardPlayCountThisCombat["light"] >= 3 && 
            PlayerProgress.Instance.activatedTalents.Contains("輕擊"))
        {
            Debug.Log("觸發天賦【輕擊】：連續三次輕攻擊");
            // 這裡可以觸發追加攻擊等效果
        }
        
        // 檢查三次中攻擊
        if (cardType == "medium" && cardPlayCountThisCombat["medium"] >= 3 && 
            PlayerProgress.Instance.activatedTalents.Contains("破碎追擊"))
        {
            Debug.Log("觸發天賦【破碎追擊】：連續三次中攻擊");
        }
        
        // 檢查三次重攻擊
        if (cardType == "heavy" && cardPlayCountThisCombat["heavy"] >= 3 && 
            PlayerProgress.Instance.activatedTalents.Contains("終結連擊"))
        {
            Debug.Log("觸發天賦【終結連擊】：連續三次重攻擊");
        }
    }
    
    /// <summary>
    /// 觸發出牌效果
    /// </summary>
    private void TriggerCardPlayEffects(string cardType)
    {
        PlayerProgress progress = PlayerProgress.Instance;
        EntityStats player = progress.playerStats;
        
        // 檢查已激活的天賦
        foreach (string talent in progress.activatedTalents)
        {
            switch (talent)
            {
                case "靈巧心靈" when cardType == "light":
                    player.AddShield(1);
                    Debug.Log("觸發天賦【靈巧心靈】：獲得1點護盾");
                    break;
                    
                case "攻擊之魂":
                    switch (cardType)
                    {
                        case "light":
                            player.AddShield(1);
                            break;
                        case "medium":
                            player.Heal(1);
                            break;
                        case "heavy":
                            player.maxHealth += 1;
                            player.Heal(1);
                            break;
                    }
                    Debug.Log("觸發天賦【攻擊之魂】");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 觸發抽牌效果
    /// </summary>
    private void TriggerCardDrawEffects(string cardType)
    {
        // 這裡可以實現抽牌時觸發的天賦效果
        PlayerProgress progress = PlayerProgress.Instance;
        
        foreach (string talent in progress.activatedTalents)
        {
            // 未來添加抽牌相關天賦
        }
    }
    
    /// <summary>
    /// 回合開始處理
    /// </summary>
    public void OnTurnStart()
    {
        // 重置回合計數器
        foreach (string key in cardPlayCountThisTurn.Keys.ToList())
        {
            cardPlayCountThisTurn[key] = 0;
        }
        
        // 檢查回合開始天賦
        CheckTurnStartTalents();
    }
    
    /// <summary>
    /// 戰鬥開始處理
    /// </summary>
    public void OnCombatStart()
    {
        InitializeDeck();
        
        // 重置戰鬥計數器
        foreach (string key in cardPlayCountThisCombat.Keys.ToList())
        {
            cardPlayCountThisCombat[key] = 0;
        }
        
        // 檢查戰鬥開始天賦
        CheckCombatStartTalents();
    }
    
    /// <summary>
    /// 戰鬥結束處理
    /// </summary>
    public void OnCombatEnd()
    {
        // 清理棄牌堆
        discardPile.Clear();
        currentHand.Clear();
        
        Debug.Log("戰鬥結束，牌組重置");
    }
    
    /// <summary>
    /// 檢查回合開始天賦
    /// </summary>
    private void CheckTurnStartTalents()
    {
        // 實現回合開始時觸發的天賦
    }
    
    /// <summary>
    /// 檢查戰鬥開始天賦
    /// </summary>
    private void CheckCombatStartTalents()
    {
        PlayerProgress progress = PlayerProgress.Instance;
        
        // 檢查毒刺先攻天賦
        if (progress.activatedTalents.Contains("毒刺先攻"))
        {
            Debug.Log("觸發天賦【毒刺先攻】：敵人開局3回合承受真傷");
            // 這裡應該觸發對敵人的傷害效果
        }
    }
    
    /// <summary>
    /// 獲取卡牌顯示名稱
    /// </summary>
    public string GetCardDisplayName(string cardType)
    {
        return cardType switch
        {
            "light" => "輕攻擊",
            "medium" => "中攻擊",
            "heavy" => "重攻擊",
            _ => "未知卡牌"
        };
    }
    
    /// <summary>
    /// 獲取當前手牌
    /// </summary>
    public List<string> GetCurrentHand()
    {
        return new List<string>(currentHand);
    }
    
    /// <summary>
    /// 獲取牌組信息
    /// </summary>
    public void GetDeckInfo(out int drawPileCount, out int discardPileCount, out int handCount)
    {
        drawPileCount = drawPile.Count;
        discardPileCount = discardPile.Count;
        handCount = currentHand.Count;
    }
    
    /// <summary>
    /// 檢查特定類型的卡牌數量（用於天賦條件）
    /// </summary>
    public int GetCardTypeCountInHand(string cardType)
    {
        return currentHand.Count(card => card == cardType);
    }
    
    /// <summary>
    /// 獲取本場戰鬥中某類卡牌的使用次數
    /// </summary>
    public int GetCardPlayCountThisCombat(string cardType)
    {
        return cardPlayCountThisCombat.ContainsKey(cardType) ? cardPlayCountThisCombat[cardType] : 0;
    }
    
    /// <summary>
    /// 獲取本回合中某類卡牌的使用次數
    /// </summary>
    public int GetCardPlayCountThisTurn(string cardType)
    {
        return cardPlayCountThisTurn.ContainsKey(cardType) ? cardPlayCountThisTurn[cardType] : 0;
    }
    
    /// <summary>
    /// 強制替換手牌（用於天賦效果）
    /// </summary>
    public void ReplaceHand(List<string> newHand)
    {
        // 將當前手牌放入棄牌堆
        discardPile.AddRange(currentHand);
        currentHand = new List<string>(newHand);
        
        OnHandUpdated?.Invoke(currentHand);
        Debug.Log("手牌已被替換");
    }
}