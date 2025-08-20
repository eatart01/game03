// src/data/cards.ts
import { Card } from '../types/card';

export const CARD_LIBRARY: { [key in CardRarity]: Card[] } = {
  C: [
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
  ],
  B: [
    // 這裡預留給B級卡牌，之後可以添加
    // { id: 'b_card_1', name: 'B級卡', ... },
  ],
  A: [
    // A級卡牌
  ],
  S: [
    // S級卡牌
  ]
};