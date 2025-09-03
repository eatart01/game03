class SaveManager {
    constructor(gameManager) {
        this.gameManager = gameManager;
        this.maxSaveSlots = 3;
        this.autoSaveInterval = null;
    }

    initialize() {
        this.setupAutoSave();
        this.setupEventListeners();
        console.log('💾 存檔管理器初始化完成');
    }

    setupAutoSave() {
        // 每5分鐘自動存檔一次
        this.autoSaveInterval = setInterval(() => {
            if (this.gameManager.isBattleActive) {
                this.autoSave();
            }
        }, 5 * 60 * 1000);
    }

    setupEventListeners() {
        // 手動存檔按鈕
        const saveBtn = document.getElementById('save-game-btn');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => this.manualSave());
        }

        // 載入存檔按鈕
        const loadBtn = document.getElementById('load-game-btn');
        if (loadBtn) {
            loadBtn.addEventListener('click', () => this.showLoadMenu());
        }

        // 重置遊戲按鈕
        const resetBtn = document.getElementById('reset-game-btn');
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.confirmReset());
        }

        // 頁面卸載前自動存檔
        window.addEventListener('beforeunload', () => {
            if (this.gameManager.isBattleActive) {
                this.autoSave();
            }
        });
    }

    createSaveData() {
        return {
            version: GAME_CONFIG.version,
            timestamp: Date.now(),
            gameData: {
                enemyCounter: this.gameManager.enemySystem.enemyCounter,
                player: {
                    maxHealth: this.gameManager.player.maxHealth,
                    currentHealth: this.gameManager.player.currentHealth,
                    attack: this.gameManager.player.attack,
                    defense: this.gameManager.player.defense,
                    hitChance: this.gameManager.player.hitChance,
                    critChance: this.gameManager.player.critChance,
                    toughness: this.gameManager.player.toughness,
                    dodge: this.gameManager.player.dodge,
                    healPerTurn: this.gameManager.player.healPerTurn || 0,
                    healOnVictory: this.gameManager.player.healOnVictory || 0,
                    shield: this.gameManager.player.shield || 0
                },
                enemyAttributes: { ...this.gameManager.enemySystem.enemyAttributes },
                talentSystem: this.gameManager.talentSystem.save(),
                rewardsObtained: this.getObtainedRewards()
            },
            metadata: {
                playTime: window.gameAnalytics ? window.gameAnalytics.playSession().playTime : 0,
                enemiesDefeated: window.performanceMonitor ? window.performanceMonitor.stats.enemiesDefeated : 0
            }
        };
    }

    getObtainedRewards() {
        // 記錄已獲得的獎勵（用於統計）
        const rewards = [];
        // 這裡可以添加獎勵追踪邏輯
        return rewards;
    }

    saveToSlot(slotNumber = 0) {
        try {
            const saveData = this.createSaveData();
            const saveKey = `dark_caverns_save_${slotNumber}`;
            
            localStorage.setItem(saveKey, JSON.stringify(saveData));
            
            // 更新最近存檔時間
            localStorage.setItem('last_save_slot', slotNumber.toString());
            localStorage.setItem('last_save_time', Date.now().toString());
            
            console.log(`✅ 遊戲已保存到槽位 ${slotNumber}`);
            this.showSaveNotification(`遊戲已保存到槽位 ${slotNumber + 1}`);
            
            return true;
        } catch (error) {
            console.error('❌ 保存失敗:', error);
            this.showSaveNotification('保存失敗', true);
            return false;
        }
    }

    loadFromSlot(slotNumber = 0) {
        try {
            const saveKey = `dark_caverns_save_${slotNumber}`;
            const savedData = localStorage.getItem(saveKey);
            
            if (!savedData) {
                console.log(`❌ 槽位 ${slotNumber} 沒有存檔`);
                this.showSaveNotification('該存檔槽位為空', true);
                return false;
            }

            const saveData = JSON.parse(savedData);
            
            // 檢查版本兼容性
            if (saveData.version !== GAME_CONFIG.version) {
                if (!this.handleVersionMigration(saveData)) {
                    this.showSaveNotification('存檔版本不兼容', true);
                    return false;
                }
            }

            // 載入遊戲數據
            this.gameManager.loadGame(saveData.gameData);
            
            console.log(`✅ 從槽位 ${slotNumber} 載入遊戲成功`);
            this.showSaveNotification('遊戲載入成功');
            
            return true;
        } catch (error) {
            console.error('❌ 載入失敗:', error);
            this.showSaveNotification('載入失敗', true);
            return false;
        }
    }

    handleVersionMigration(saveData) {
        console.warn(`存檔版本遷移: ${saveData.version} -> ${GAME_CONFIG.version}`);
        
        // 這裡可以添加版本遷移邏輯
        // 目前簡單處理：允許載入，但顯示警告
        this.showSaveNotification('存檔來自舊版本，可能有不兼容風險', true);
        return true;
    }

    manualSave() {
        if (this.gameManager.isBattleActive) {
            this.showSaveNotification('戰鬥中無法保存', true);
            return false;
        }

        // 使用第一個存檔槽
        return this.saveToSlot(0);
    }

    autoSave() {
        if (!this.gameManager.isBattleActive) return;
        
        console.log('🔄 自動保存中...');
        return this.saveToSlot(0);
    }

    showLoadMenu() {
        if (this.gameManager.isBattleActive) {
            this.showSaveNotification('戰鬥中無法載入', true);
            return;
        }

        this.createLoadMenu();
    }

    createLoadMenu() {
        const modal = document.createElement('div');
        modal.className = 'save-modal';
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3>載入遊戲</h3>
                    <button class="close-btn">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="save-slots">
                        ${this.generateSaveSlotsHTML()}
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="secondary-btn" id="import-save-btn">匯入存檔</button>
                    <button class="secondary-btn" id="export-save-btn">匯出存檔</button>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // 添加事件監聽器
        modal.querySelector('.close-btn').addEventListener('click', () => modal.remove());
        modal.querySelectorAll('.save-slot').forEach((slot, index) => {
            slot.addEventListener('click', () => {
                this.loadFromSlot(index);
                modal.remove();
            });
        });

        // 匯出/匯入功能
        modal.querySelector('#export-save-btn').addEventListener('click', () => {
            this.exportSave();
        });

        modal.querySelector('#import-save-btn').addEventListener('click', () => {
            this.showImportDialog();
        });
    }

    generateSaveSlotsHTML() {
        let html = '';
        for (let i = 0; i < this.maxSaveSlots; i++) {
            const saveData = this.getSaveSlotInfo(i);
            html += `
                <div class="save-slot ${saveData.exists ? 'exists' : 'empty'}" data-slot="${i}">
                    <div class="save-slot-header">
                        <span class="save-slot-title">存檔槽位 ${i + 1}</span>
                        ${saveData.exists ? `<span class="save-slot-time">${saveData.time}</span>` : ''}
                    </div>
                    ${saveData.exists ? `
                        <div class="save-slot-info">
                            <div>敵人: ${saveData.enemyCounter}</div>
                            <div>遊玩時間: ${saveData.playTime}</div>
                            <div>擊敗數: ${saveData.enemiesDefeated}</div>
                        </div>
                    ` : '<div class="save-slot-empty">空槽位</div>'}
                </div>
            `;
        }
        return html;
    }

    getSaveSlotInfo(slotNumber) {
        const saveKey = `dark_caverns_save_${slotNumber}`;
        const savedData = localStorage.getItem(saveKey);
        
        if (!savedData) {
            return { exists: false };
        }

        try {
            const saveData = JSON.parse(savedData);
            return {
                exists: true,
                time: this.formatTime(saveData.timestamp),
                enemyCounter: saveData.gameData.enemyCounter,
                playTime: this.formatPlayTime(saveData.metadata.playTime),
                enemiesDefeated: saveData.metadata.enemiesDefeated || 0
            };
        } catch (error) {
            return { exists: false };
        }
    }

    formatTime(timestamp) {
        return new Date(timestamp).toLocaleString('zh-TW');
    }

    formatPlayTime(seconds) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        return `${hours}時${minutes}分`;
    }

    showSaveNotification(message, isError = false) {
        // 移除現有的通知
        const existingNotification = document.querySelector('.save-notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        const notification = document.createElement('div');
        notification.className = `save-notification ${isError ? 'error' : 'success'}`;
        notification.textContent = message;
        
        document.body.appendChild(notification);

        // 3秒後自動消失
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 3000);
    }

    confirmReset() {
        if (confirm('確定要重置遊戲嗎？所有進度將會丟失！')) {
            this.resetGame();
        }
    }

    resetGame() {
        // 清除所有存檔
        for (let i = 0; i < this.maxSaveSlots; i++) {
            localStorage.removeItem(`dark_caverns_save_${i}`);
        }
        
        // 重置遊戲狀態
        this.gameManager.initializeGame();
        
        console.log('🔄 遊戲已重置');
        this.showSaveNotification('遊戲已重置');
    }

    exportSave() {
        const dataUri = SaveSystem.exportSave();
        if (dataUri) {
            const exportFileDefaultName = `dark-caverns-save-${Date.now()}.json`;
            
            const linkElement = document.createElement('a');
            linkElement.setAttribute('href', dataUri);
            linkElement.setAttribute('download', exportFileDefaultName);
            linkElement.click();
            this.showSaveNotification('存檔匯出成功');
        }
    }

    showImportDialog() {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.json';
        
        input.onchange = (e) => {
            const file = e.target.files[0];
            if (file) {
                this.importSave(file);
            }
        };
        
        input.click();
    }

    importSave(file) {
        SaveSystem.importSave(file)
            .then(() => {
                this.showSaveNotification('存檔匯入成功');
                // 自動重新載入遊戲
                setTimeout(() => location.reload(), 1000);
            })
            .catch(error => {
                this.showSaveNotification(`匯入失敗: ${error}`, true);
            });
    }

    cleanup() {
        if (this.autoSaveInterval) {
            clearInterval(this.autoSaveInterval);
        }
    }
}