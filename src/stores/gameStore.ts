// src/stores/gameStore.ts
import { create } from 'zustand';
import { GameState, Card, CardRarity } from '../types/card';
import { CARD_LIBRARY } from '../data/cards';

interface GameStore extends GameState {
  // 動作 (Actions)
  startGame: () => void;
  drawCard: (rarity: CardRarity) => Card | null;
  playCard: (cardIndex: number) => void;
  challengeEnemy: () => void;
  resetGame: () => void;
}

// 抽牌邏輯：按照 S->A->B->C 的優先級抽取
const drawCardByPriority = (): Card => {
  const rarities: CardRarity[] = ['S', 'A', 'B', 'C'];
  
  for (const rarity of rarities) {
    const availableCards = CARD_LIBRARY[rarity];
    if (availableCards && availableCards.length > 0) {
      // 隨機選擇一張該稀有度的卡牌
      const randomIndex = Math.floor(Math.random() * availableCards.length);
      return { ...availableCards[randomIndex] };
    }
  }
  
  // 如果所有牌庫都空了，返回一張隨機的C級卡
  const cCards = CARD_LIBRARY.C;
  const randomIndex = Math.floor(Math.random() * cCards.length);
  return { ...cCards[randomIndex] };
};

export const useGameStore = create<GameStore>((set, get) => ({
  // 初始狀態
  playerHealth: 100,
  enemyHealth: 100,
  isGameStarted: false,
  isPlayerTurn: true,
  deck: [],
  hand: [],
  selectedCardIndex: null,
  isChallengeButtonVisible: true,

  // 開始遊戲
  startGame: () => {
    // 初始化手牌：C階級三種卡各一張
    const initialHand = CARD_LIBRARY.C.map(card => ({ ...card }));
    
    set({
      isGameStarted: true,
      hand: initialHand,
      isChallengeButtonVisible: false,
      playerHealth: 100,
      enemyHealth: 100
    });
  },

  // 抽牌邏輯
  drawCard: (rarity: CardRarity) => {
    const availableCards = CARD_LIBRARY[rarity];
    if (!availableCards || availableCards.length === 0) {
      return null;
    }
    const randomIndex = Math.floor(Math.random() * availableCards.length);
    return { ...availableCards[randomIndex] };
  },

  // 出牌邏輯
  playCard: (cardIndex: number) => {
    const { hand, enemyHealth } = get();
    
    if (cardIndex >= hand.length) return;

    const card = hand[cardIndex];
    
    // 計算命中
    const isHit = Math.random() * 100 < card.hitRate;
    const isCrit = isHit && (Math.random() * 100 < card.critRate);
    
    let damageDealt = 0;
    if (isHit) {
      damageDealt = isCrit ? card.damage * 2 : card.damage;
    }

    // 更新手牌：將使用的牌替換為新的C級卡
    const newHand = [...hand];
    const newCard = drawCardByPriority();
    newHand[cardIndex] = newCard;

    set({
      enemyHealth: Math.max(0, enemyHealth - damageDealt),
      hand: newHand,
      selectedCardIndex: null
    });

    // 這裡可以添加敵人反擊邏輯
  },

  // 挑戰敵人
  challengeEnemy: () => {
    const { hand } = get();
    
    // 重置手牌為C階級三種卡
    const newHand = CARD_LIBRARY.C.map(card => ({ ...card }));
    
    set({
      hand: newHand,
      enemyHealth: 100,
      isChallengeButtonVisible: false
    });
  },

  // 重置遊戲
  resetGame: () => {
    set({
      playerHealth: 100,
      enemyHealth: 100,
      isGameStarted: false,
      hand: [],
      selectedCardIndex: null,
      isChallengeButtonVisible: true
    });
  }
}));