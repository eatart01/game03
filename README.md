
---

## 🚀 執行方式

1. 使用本地伺服器開啟 `index.html`（建議使用 VS Code + Live Server）
2. 確保所有 JS 模組已正確載入，順序為：main → combat → parry → ui
3. 開啟瀏覽器 DevTools，檢查 console 是否有錯誤訊息
4. 若有錯誤，可參考 checklist 系統進行排查

---

## 🔍 錯誤檢查入口

- 📋 [Markdown 錯誤檢查清單](./checklist/checklist.md)  
- 🖥️ [視覺化錯誤檢查介面](./checklist/checklist.html)

---

## 🤖 AI 協作說明

本專案支援 AI 分析與模組優化，請參考 checklist 系統進行快速檢查。  
AI 可根據 checklist.json 自動判斷模組健康狀態，並提供錯誤排查與邏輯建議。

建議每個 JS 檔案皆加入註解，並保持模組責任清晰，以利 AI 協作與未來擴充。

---

## 🧩 模組設計原則

- 每個功能模組獨立拆分（combat, parry, ui）
- 所有資料結構集中管理（如 talents.json）
- UI 元素與邏輯分離，支援視覺回饋與互動擴充
- 錯誤提示明確，方便人類與 AI 快速定位問題

---

## 📈 未來擴充方向

- 整合 Construct 3 或 Unity 進行視覺化編輯
- 加入特效模組（如 hit flash, parry glow）
- 建立 `docs/` 資料夾記錄設計邏輯與 AI 協作備忘錄
- 支援 GitHub Pages 預覽遊戲原型

---

> 本專案由 Wen 主導設計，強調模組化、錯誤透明化與 AI 協作友善性。  
> 若有任何錯誤、建議或擴充需求，請參考 checklist 系統或聯繫 Wen 本人。
