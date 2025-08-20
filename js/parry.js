let parryWindow = false;
let playerParried = false;

function startParrySequence() {
  parryWindow = true;
  playerParried = false;
  showMessage("準備招架！請在 1 秒內按 ↑ 鍵");

  // 開啟招架時機
  setTimeout(() => {
    parryWindow = false;

    if (playerParried) {
      showMessage("成功招架！你閃避了攻擊");
      triggerParryEffect(); // UI 視覺反饋
    } else {
      playerHP -= 10;
      if (playerHP < 0) playerHP = 0;
      updateHPDisplay();
      showMessage("招架失敗！你受到 10 點傷害");
    }

    setTimeout(() => {
      endTurn(); // 回合結束
    }, 800);
  }, 1000); // 招架時機持續 1 秒
}

// 玩家輸入判定
document.addEventListener("keydown", function(e) {
  if (parryWindow && e.key === "ArrowUp") {
    playerParried = true;
  }
});
