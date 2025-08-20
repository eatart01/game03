import { Card, CardRarity } from '../types/card';

export const CARD_LIBRARY: { [key in CardRarity]: Card[] } = {
  S: [
    // S級卡牌 - 最高優先級
    {
      id: 's_super_attack',
      name: '超重擊',
      rarity: 'S',
      description: '傳說中的一擊必殺技',
      hitRate: 60,
      critRate: 50,
      damage: 45
    },
    {
      id: 's_divine_strike',
      name: '神聖打擊',
      rarity: 'S',
      description: '蘊含神聖力量的攻擊',
      hitRate: 75,
      critRate: 40,
      damage: 38
    }
  ],
  A: [
    // A級卡牌
    {
      id: 'a_precision_strike',
      name: '精準打擊',
      rarity: 'A',
      description: '經過精密計算的攻擊',
      hitRate: 85,
      critRate: 25,
      damage: 32
    },
    {
      id: 'a_swift_attack',
      name: '迅捷連擊',
      rarity: 'A',
      description: '快速的連續攻擊',
      hitRate: 90,
      critRate: 15,
      damage: 28
    }
  ],
  B: [
    // B級卡牌
    {
      id: 'b_power_attack',
      name: '力量攻擊',
      rarity: 'B',
      description: '注入更多力量的攻擊',
      hitRate: 78,
      critRate: 18,
      damage: 26
    },
    {
      id: 'b_balanced_strike',
      name: '平衡打擊',
      rarity: 'B',
      description: '攻守兼備的招式',
      hitRate: 80,
      critRate: 12,
      damage: 24
    }
  ],
  C: [
    // C級卡牌 - 最基礎
    {
      id: 'c_light_attack',
      name: '輕攻擊',
      rarity: 'C',
      description: '快速的基礎攻擊',
      hitRate: 82,
      critRate: 5,
      damage: 20
    },
    {
      id: 'c_medium_attack',
      name: '中攻擊',
      rarity: 'C',
      description: '平衡的攻擊',
      hitRate: 74,
      critRate: 10,
      damage: 24
    },
    {
      id: 'c_heavy_attack',
      name: '重攻擊',
      rarity: 'C',
      description: '強力但不易命中的攻擊',
      hitRate: 67,
      critRate: 30,
      damage: 30
    }
  ]
};

// 輔助函數：根據優先級抽牌
export const drawCardByPriority = (): Card => {
  const rarities: CardRarity[] = ['S', 'A', 'B', 'C'];
  
  for (const rarity of rarities) {
    const availableCards = CARD_LIBRARY[rarity];
    if (availableCards && availableCards.length > 0) {
      const randomIndex = Math.floor(Math.random() * availableCards.length);
      return { ...availableCards[randomIndex] };
    }
  }
  
  // 備用方案：如果所有牌庫都空了，返回隨機C級卡
  const cCards = CARD_LIBRARY.C;
  const randomIndex = Math.floor(Math.random() * cCards.length);
  return { ...cCards[randomIndex] };
};

// 獲取初始手牌（C階級三種卡各一張）
export const getInitialHand = (): Card[] => {
  return CARD_LIBRARY.C.map(card => ({ ...card }));
};