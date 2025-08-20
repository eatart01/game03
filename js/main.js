// 主邏輯模組：初始化 UI，橋接邏輯與視覺模組

import { loadTalents } from './ui.js';

// 頁面載入後初始化
window.addEventListener('DOMContentLoaded', () => {
  loadTalents();
});
