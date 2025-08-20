import { create } from 'zustand';
import { GameState, Card } from '../types/card';
import { CARD_LIBRARY, drawCardByPriority, getInitialHand } from '../data/cards';

interface GameStore extends GameState {
  // 動作 (Actions)
  startGame: () => void;
  playCard: (cardIndex: number) => void;
  challengeEnemy: () => void;
  resetGame: () => void;
  setSelectedCard: (index: number | null) => void;
}

// 計算攻擊結果
const calculateAttackResult = (card: Card) => {
  const isHit = Math.random() * 100 < card.hitRate;
  let damageDealt = 0;
  let isCrit = false;

  if (isHit) {
    isCrit = Math.random() * 100 < card.critRate;
    damageDealt = isCrit ? card.damage * 2 : card.damage;
  }

  return {
    isHit,
    isCrit,
    damageDealt,
    cardName: card.name
  };
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
    const initialHand = getInitialHand();
    
    set({
      isGameStarted: true,
      hand: initialHand,
      isChallengeButtonVisible: false,
      playerHealth: 100,
      enemyHealth: 100,
      selectedCardIndex: null
    });
  },

  // 出牌邏輯
  playCard: (cardIndex: number) => {
    const state = get();
    
    if (!state.isGameStarted || cardIndex >= state.hand.length) {
      return;
    }

    const card = state.hand[cardIndex];
    const attackResult = calculateAttackResult(card);

    // 顯示攻擊結果（實際遊戲中可以替換為更華麗的動畫）
    console.log(`使用: ${attackResult.cardName}`);
    console.log(attackResult.isHit ? 
      (attackResult.isCrit ? `暴擊！造成 ${attackResult.damageDealt} 傷害` : `命中！造成 ${attackResult.damageDealt} 傷害`) : 
      '未命中！'
    );

    // 更新敵人血量
    const newEnemyHealth = Math.max(0, state.enemyHealth - attackResult.damageDealt);

    // 替換使用的牌（黑色遮蓋效果後補牌）
    const newHand = [...state.hand];
    
    // 先將使用的牌標記為待替換（實際UI會顯示黑色遮蓋）
    newHand[cardIndex] = { 
      ...newHand[cardIndex], 
      isBeingReplaced: true 
    };

    set({
      enemyHealth: newEnemyHealth,
      hand: newHand,
      selectedCardIndex: null
    });

    // 延遲一秒後補牌（模擬黑色遮蓋效果）
    setTimeout(() => {
      const currentState = get();
      const updatedHand = [...currentState.hand];
      
      // 抽取新牌（按照 S->A->B->C 優先級）
      const newCard = drawCardByPriority();
      updatedHand[cardIndex] = newCard;

      set({
        hand: updatedHand,
        isPlayerTurn: true
      });

      // 檢查遊戲是否結束
      if (newEnemyHealth <= 0) {
        console.log('敵人被擊敗！');
        set({
          isChallengeButtonVisible: true,
          isGameStarted: false
        });
      }

    }, 1000); // 1秒後補牌
  },

  // 挑戰新敵人
  challengeEnemy: () => {
    const initialHand = getInitialHand();
    
    set({
      hand: initialHand,
      enemyHealth: 100,
      playerHealth: 100,
      isChallengeButtonVisible: false,
      isGameStarted: true,
      selectedCardIndex: null
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
  },

  // 設置選中的卡牌
  setSelectedCard: (index: number | null) => {
    set({ selectedCardIndex: index });
  }
}));