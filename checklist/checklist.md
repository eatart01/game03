# 🧠 game03 全局錯誤檢查 Checklist

## 📦 檔案結構
- [ ] 所有 JS 檔案已模組化（main.js / combat.js / parry.js / ui.js）
- [ ] index.html 已正確引入所有模組，順序無誤
- [ ] assets/images/ 無未使用或重複素材
- [ ] talents.json 結構正確，已通過 JSON lint

## 🧪 功能邏輯
- [ ] combat.js：攻擊計算無 NaN、undefined、負值
- [ ] parry.js：格擋判定條件清楚，無死循環
- [ ] ui.js：所有 UI 元素皆有對應 DOM，無 null reference
- [ ] main.js：初始化流程完整，模組載入順序正確

## 🖼️ UI/DOM
- [ ] 所有按鈕、圖像、特效皆有綁定事件
- [ ] 無 console error（可用 DevTools 檢查）
- [ ] 所有互動皆有視覺回饋（hover、click、damage）

## 📊 資料結構
- [ ] talents.json：每個 talent 皆有 id、name、effect 欄位
- [ ] 所有 JSON 資料皆能被 JS 正確讀取與解析

## 🤖 AI 協作友善
- [ ] 每個 JS 檔案皆有註解（至少每個 function 一句）
- [ ] 所有錯誤訊息皆有明確提示（方便 AI debug）
- [ ] README.md 有模組說明與執行方式
- [ ] 可直接貼 repo 給 AI 進行分析與優化
