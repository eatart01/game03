// main.js
import { loadTalents } from './ui.js';

document.addEventListener('DOMContentLoaded', () => {
  // 載入天賦格子
  loadTalents();

  // 初始化攻擊卡片
  const cardContainer = document.getElementById('card-container');
  const cards = [
    {
      name: '火球術',
      description: '造成 30 點火焰傷害',
      type: 'magic',
      damage: 30
    },
    {
      name: '劍擊',
      description: '造成 20 點物理傷害',
      type: 'melee',
      damage: 20
    },
    {
      name: '毒箭',
      description: '造成 15 點持續傷害',
      type: 'ranged',
      damage: 15
    }
  ];

  cards.forEach(card => {
    const div = document.createElement('div');
    div.className = 'card';
    div.innerHTML = `
      <h3>${card.name}</h3>
      <p>${card.description}</p>
    `;

    div.addEventListener('click', () => {
      applyDamage(card.damage);
    });

    cardContainer.appendChild(div);
  });

  // 初始化敵人血量
  const enemyHP = {
    max: 100,
    current: 100
  };

  function applyDamage(damage) {
    enemyHP.current = Math.max(0, enemyHP.current - damage);
    updateEnemyHP();
  }

  function updateEnemyHP() {
    const fill = document.getElementById('enemy-hp-fill');
    const text = document.getElementById('enemy-hp-text');
    const percent = (enemyHP.current / enemyHP.max) * 100;

    fill.style.width = `${percent}%`;
    text.textContent = `HP: ${enemyHP.current} / ${enemyHP.max}`;
  }

  updateEnemyHP(); // 初始渲染
});
