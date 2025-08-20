// 初始化遊戲狀態
let playerHP = 100;
let enemyHP = 100;
let turnCount = 1;
let availableTalentPoints = 3;

// 初始化 UI
updateHPDisplay();
updateTurnDisplay();
updateTalentDisplay();
showMessage("請選擇你的行動。");

// 綁定按鈕事件
document.getElementById("attack-btn").addEventListener("click", () => {
  handleAttack();
});

document.getElementById("parry-btn").addEventListener("click", () => {
  startParrySequence();
});

document.getElementById("talent-btn").addEventListener("click", () => {
  configureTalent();
});

// 回合結束邏輯（可由 combat.js 呼叫）
function endTurn() {
  turnCount++;
  updateTurnDisplay();
  enemyAction(); // 由 combat.js 控制敵人行動
}
