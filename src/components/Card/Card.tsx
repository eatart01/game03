import React from 'react';
import styles from './Card.module.css';
import { Card as CardType } from '../../types/card';

interface CardProps {
  card: CardType;
  index: number;
  isFlipped: boolean;
  onClick: () => void;
}

const Card: React.FC<CardProps> = ({ card, index, isFlipped, onClick }) => {
  // 如果卡片正在被替換，顯示黑色遮蓋
  const showBlackCover = card.isBeingReplaced;

  return (
    <div 
      className={`${styles.card} ${isFlipped ? styles.flipped : ''}`}
      onClick={onClick}
    >
      <div className={styles['card-inner']}>
        <div className={styles['card-front']}>
          {/* 牌背 - 黑色色塊 */}
          <div className={styles['card-back']}>
            {showBlackCover && (
              <div className={styles['black-cover']}>
                <span>抽牌中...</span>
              </div>
            )}
          </div>
        </div>
        <div className={styles['card-back']}>
          {/* 牌面內容 */}
          {!showBlackCover ? (
            <div className={styles['card-content']}>
              <h3 className={styles['card-name']}>{card.name}</h3>
              <p className={styles['card-rarity']}>
                <span className={`${styles.rarity} ${styles[`rarity-${card.rarity.toLowerCase()}`]}`}>
                  稀有度: {card.rarity}
                </span>
              </p>
              <div className={styles['card-stats']}>
                <p>命中: <span className={styles.value}>{card.hitRate}%</span></p>
                <p>暴擊: <span className={styles.value}>{card.critRate}%</span></p>
                <p>傷害: <span className={styles.value}>{card.damage}</span></p>
              </div>
              <p className={styles['card-description']}>{card.description}</p>
            </div>
          ) : (
            <div className={styles['black-cover']}>
              <span>抽牌中...</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Card;