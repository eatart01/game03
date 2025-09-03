/* 天賦點顯示 */
.talent-points {
    background: var(--color-accent);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    font-weight: bold;
    margin-bottom: 10px;
    text-align: center;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

/* 天賦槽位樣式 */
.talent-slot {
    background: rgba(255, 255, 255, 0.1);
    border: 2px solid transparent;
    border-radius: 6px;
    padding: 8px;
    cursor: pointer;
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 60px;
    position: relative;
}

.talent-slot:hover {
    background: rgba(255, 255, 255, 0.15);
    transform: translateY(-2px);
}

.talent-slot.available {
    border-color: #00b894;
    background: rgba(0, 184, 148, 0.1);
    cursor: pointer;
}

.talent-slot.available:hover {
    border-color: #00b894;
    background: rgba(0, 184, 148, 0.2);
    box-shadow: 0 0 10px rgba(0, 184, 148, 0.3);
}

.talent-slot.active {
    border-color: #6c5ce7;
    background: rgba(108, 92, 231, 0.1);
}

.talent-slot.locked {
    border-color: #636e72;
    background: rgba(99, 110, 114, 0.1);
    cursor: not-allowed;
    opacity: 0.6;
}

.talent-number {
    font-size: 10px;
    color: rgba(255, 255, 255, 0.6);
    position: absolute;
    top: 4px;
    left: 4px;
}

.talent-name {
    font-size: 11px;
    font-weight: bold;
    text-align: center;
    margin-bottom: 4px;
    line-height: 1.2;
}

.talent-status {
    font-size: 9px;
    padding: 2px 4px;
    border-radius: 3px;
    text-align: center;
}

.talent-slot.available .talent-status {
    background: #00b894;
    color: white;
}

.talent-slot.active .talent-status {
    background: #6c5ce7;
    color: white;
}

.talent-slot.locked .talent-status {
    background: #636e72;
    color: white;
}

/* 天賦提示框 */
.talent-tooltip {
    position: absolute;
    background: rgba(0, 0, 0, 0.9);
    border: 1px solid var(--color-accent);
    border-radius: 6px;
    padding: 12px;
    z-index: 1000;
    max-width: 250px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
    pointer-events: none;
}

.talent-tooltip-header {
    color: var(--color-accent);
    font-weight: bold;
    margin-bottom: 8px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.2);
    padding-bottom: 4px;
}

.talent-tooltip-description {
    font-size: 12px;
    line-height: 1.4;
    margin-bottom: 8px;
}

.talent-tooltip-type {
    font-size: 11px;
    color: rgba(255, 255, 255, 0.7);
    font-style: italic;
}

.talent-tooltip-cost {
    font-size: 11px;
    color: #f9ca24;
    margin-top: 8px;
}

/* 天賦激活效果 */
@keyframes talentActivate {
    0% { transform: scale(1); box-shadow: 0 0 0 rgba(108, 92, 231, 0); }
    50% { transform: scale(1.1); box-shadow: 0 0 20px rgba(108, 92, 231, 0.5); }
    100% { transform: scale(1); box-shadow: 0 0 0 rgba(108, 92, 231, 0); }
}

.talent-slot.active {
    animation: talentActivate 0.5s ease;
}

/* 天賦類型顏色 */
.talent-type-offensive {
    border-left: 3px solid #ff6b6b;
}

.talent-type-defensive {
    border-left: 3px solid #4ecdc4;
}

.talent-type-healing {
    border-left: 3px solid #2ed573;
}

.talent-type-special {
    border-left: 3px solid #f9ca24;
}

/* 天賦網格滾動條 */
#talents-grid::-webkit-scrollbar {
    width: 4px;
}

#talents-grid::-webkit-scrollbar-track {
    background: rgba(0, 0, 0, 0.1);
    border-radius: 2px;
}

#talents-grid::-webkit-scrollbar-thumb {
    background: var(--color-accent);
    border-radius: 2px;
}

/* 響應式設計 */
@media (max-width: 900px) {
    .talent-slot {
        min-height: 50px;
        padding: 6px;
    }
    
    .talent-name {
        font-size: 10px;
    }
    
    .talent-status {
        font-size: 8px;
    }
    
    .talent-points {
        font-size: 14px;
        padding: 6px 10px;
    }
}

/* 天賦點獲得動畫 */
@keyframes talentPointGlow {
    0% { box-shadow: 0 0 0 rgba(233, 69, 96, 0); }
    50% { box-shadow: 0 0 20px rgba(233, 69, 96, 0.5); }
    100% { box-shadow: 0 0 0 rgba(233, 69, 96, 0); }
}

.talent-points.new-point {
    animation: talentPointGlow 1s ease;
}