export type CardRarity = 'C' | 'B' | 'A' | 'S';

export interface Card {
  id: string;
  name: string;
  rarity: CardRarity;
  description: string;
  // 戰鬥屬性
  hitRate: number;   // 命中率 %
  critRate: number;  // 暴擊率 %
  damage: number;    // 傷害值
  // UI 狀態標記
  isBeingReplaced?: boolean; // 用於標記正在被替換的牌（顯示黑色遮蓋）
}

export interface GameState {
  playerHealth: number;
  enemyHealth: number;
  isGameStarted: boolean;
  isPlayerTurn: boolean;
  // 卡牌相關狀態
  deck: Card[];              // 整個牌庫
  hand: Card[];              // 玩家當前手牌
  selectedCardIndex: number | null; // 玩家選擇的牌
  isChallengeButtonVisible: boolean;
}

// 攻擊結果類型
export interface AttackResult {
  isHit: boolean;
  isCrit: boolean;
  damageDealt: number;
  cardName: string;
}