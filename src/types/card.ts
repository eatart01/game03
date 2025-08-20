// src/types/card.ts
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