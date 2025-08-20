// ui.js

export async function loadTalents() {
  try {
    const response = await fetch('data/talents.json');
    const talents = await response.json();

    const grid = document.getElementById('talents-grid');
    grid.innerHTML = ''; // 清空舊內容

    talents.forEach((talent, index) => {
      const cell = document.createElement('div');
      cell.className = 'talent-cell';
      cell.title = `${talent.name} (${talent.type})\n${talent.description}`;
      cell.dataset.index = index;

      // 可選：顯示編號或名稱
      cell.textContent = talent.name || `#${index + 1}`;

      // 點擊互動（可擴充）
      cell.addEventListener('click', () => {
        console.log(`選擇天賦：${talent.name}`);
        cell.classList.toggle('selected');
      });

      grid.appendChild(cell);
    });
  } catch (error) {
    console.error('載入 talents.json 失敗：', error);
    const grid = document.getElementById('talents-grid');
    grid.innerHTML = '<p style="color: red;">無法載入天賦資料</p>';
  }
}
