function handleAttack() {
  const damage = calculatePlayerDamage();
  enemyHP -= damage;
  if (enemyHP < 0) enemyHP = 0;

  updateHPDisplay();
  showMessage(`你造成了 ${damage} 點傷害！`);

  setTimeout(() => {
    endTurn(); // 由 main.js 控制回合流程
  }, 800);
}

function enemyAction() {
  const damage = calculateEnemyDamage();
  playerHP -= damage;
  if (playerHP < 0) playerHP = 0;

  updateHPDisplay();
  showMessage(`敵人攻擊造成 ${damage} 點傷害！`);
}

function calculatePlayerDamage() {
  // 可根據天賦點數或技能強化
  let base = 20;
  if (availableTalentPoints <= 0) return base;
  return base + availableTalentPoints * 2;
}

function calculateEnemyDamage() {
  // 可根據回合數或敵人強化
  let base = 15;
  if (turnCount >= 5) base += 5;
  return base;
}

function configureTalent() {
  if (availableTalentPoints <= 0) {
    showMessage("你沒有可配置的天賦點數！");
    return;
  }

  availableTalentPoints--;
  updateTalentDisplay();
  showMessage("你強化了攻擊力！");
}
