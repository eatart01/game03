// src/components/Card/Card.tsx
import React from 'react';
import styles from './Card.module.css';
import { Card as CardType } from '../../types/card';
import { useGameStore } from '../../stores/gameStore';

interface CardProps {
  card: CardType;
  index: number;
  isFlipped: boolean;
  onClick: () => void;
}

const Card: React.FC<CardProps> = ({ card, index, isFlipped, onClick }) => {
  const { isGameStarted, selectedCardIndex } = useGameStore();

  return (
    <div 
      className={`${styles.card} ${isFlipped ? styles.flipped : ''}`}
      onClick={onClick}
    >
      <div className={styles['card-inner']}>
        <div className={styles['card-front']}>
          {/* 牌背 - 黑色色塊 */}
          <div className={styles['card-back']}></div>
        </div>
        <div className={styles['card-back']}>
          {/* 牌面內容 */}
          <h3>{card.name}</h3>
          <p>稀有度: {card.rarity}</p>
          <p>命中: {card.hitRate}%</p>
          <p>暴擊: {card.critRate}%</p>
          <p>傷害: {card.damage}</p>
          <p>{card.description}</p>
        </div>
      </div>
    </div>
  );
};

export default Card;