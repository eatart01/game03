// src/components/GameBoard/GameBoard.tsx
import React from 'react';
import styles from './GameBoard.module.css';
import Card from '../Card/Card';
import { useGameStore } from '../../stores/gameStore';

const GameBoard: React.FC = () => {
  const { 
    hand, 
    isGameStarted, 
    isChallengeButtonVisible, 
    playerHealth, 
    enemyHealth,
    startGame, 
    playCard,
    challengeEnemy 
  } = useGameStore();

  const handleCardClick = (index: number) => {
    if (isGameStarted) {
      playCard(index);
    }
  };

  return (
    <div className={styles.gameBoard}>
      {/* 敵人區域 */}
      <div className={styles.enemyArea}>
        <div className={styles.healthBar}>
          敵人生命: {enemyHealth}
        </div>
      </div>

      {/* 挑戰按鈕 */}
      {isChallengeButtonVisible && (
        <button 
          className={styles.challengeButton}
          onClick={challengeEnemy}
        >
          開始挑戰
        </button>
      )}

      {/* 玩家區域 */}
      <div className={styles.playerArea}>
        <div className={styles.healthBar}>
          玩家生命: {playerHealth}
        </div>
        
        {/* 手牌區域 */}
        <div className={styles.hand}>
          {hand.map((card, index) => (
            <Card
              key={index}
              card={card}
              index={index}
              isFlipped={!isGameStarted} // 遊戲未開始時牌是蓋著的
              onClick={() => handleCardClick(index)}
            />
          ))}
        </div>
      </div>
    </div>
  );
};

export default GameBoard;