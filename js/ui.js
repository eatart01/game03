function updateHPDisplay() {
  document.getElementById("player-hp").textContent = playerHP;
  document.getElementById("enemy-hp").textContent = enemyHP;
}

function updateTurnDisplay() {
  document.getElementById("turn-count").textContent = turnCount;
}

function updateTalentDisplay() {
  document.getElementById("talent-points").textContent = availableTalentPoints;
}

function showMessage(text) {
  const messageEl = document.getElementById("game-message");
  messageEl.textContent = text;
}

function triggerParryEffect() {
  const container = document.getElementById("game-container");
  container.classList.add("parry-success");

  setTimeout(() => {
    container.classList.remove("parry-success");
  }, 300);
}
