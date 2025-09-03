class GameManager {
    constructor() {
        this.isBattleActive = false;
        this.isPlayerTurn = true;
        this.player = null;
        this.currentEnemy = null;
        this.enemySystem = new EnemySystem();
        this.rewardSystem = new RewardSystem();
        this.talentSystem = new TalentSystem();
        this.saveManager = new SaveManager(this);
        this.configManager = new ConfigManager(); // 新增
        this.showingRewards = false;
        this.talentPointsEarned = 0;
        this.battleStartTime = 0;
        this.totalPlayTime = 0;
        this.initializeGame();
    }

    initializeGame() {
        this.player = new Entity({ maxHealth: 100 });
        this.currentEnemy = this.enemySystem.createEnemy(1);
        
        this.setupEventListeners();
        this.initializeTalentGrid();
        this.updateUI();
        
        // 初始化存檔管理器
        this.saveManager.initialize();
        
        // 應用難度設定
        this.applyDifficultySettings();
        
        // 嘗試自動載入存檔
        this.autoLoadGame();

        // 初始化遊戲統計
        this.initializeGameStats();

        console.log('✅ 遊戲初始化完成');
    }

    applyDifficultySettings() {
        const multipliers = this.configManager.getDifficultyMultipliers();
        
        // 這裡可以根據難度調整遊戲參數
        console.log('難度設定已應用:', multipliers);
        
        // 發送難度變更事件
        this.dispatchGameEvent('difficultyChanged', {
            difficulty: this.configManager.getSetting('difficulty', 'level'),
            multipliers: multipliers
        });
    }

    // 在戰鬥計算中應用難度設定
    calculateDamageWithDifficulty(attacker, defender, attackType) {
        const result = BattleSystem.calculateDamage(attacker, defender, attackType);
        const multipliers = this.configManager.getDifficultyMultipliers();
        
        // 應用難度加成
        if (attacker === this.player) {
            result.damage = Math.ceil(result.damage * multipliers.damage);
        } else {
            result.damage = Math.ceil(result.damage / multipliers.damage);
        }
        
        return result;
    }

    // 更新 handleCardClick 方法使用新的傷害計算
    async handleCardClick(event) {
        if (!this.isBattleActive || !this.isPlayerTurn) return;
        
        const cardType = event.currentTarget.dataset.type;
        const attackName = BattleSystem.getAttackName(cardType);
        this.addCombatMessage(`🎯 使用了${attackName}攻擊`);
        
        this.setCardsEnabled(false);
        this.isPlayerTurn = false;
        
        await this.delay(500);
        
        // 使用帶難度設定的傷害計算
        const result = this.calculateDamageWithDifficulty(
            this.player,
            this.currentEnemy,
            cardType
        );
        
        // ... 其餘保持不變
    }

    // 添加設定變更監聽
    setupEventListeners() {
        // 現有監聽器...
        
        // 設定變更監聽
        document.addEventListener('configChanged', (e) => {
            this.handleConfigChange(e.detail.config);
        });
    }

    handleConfigChange(newConfig) {
        // 處理難度變更
        if (newConfig.difficulty) {
            this.applyDifficultySettings();
        }
        
        // 處理畫面設定變更
        if (newConfig.graphics) {
            this.applyGraphicsSettings();
        }
        
        // 處理遊戲設定變更
        if (newConfig.gameplay) {
            this.applyGameplaySettings();
        }
        
        this.addCombatMessage('⚙️ 遊戲設定已更新');
    }

    applyGraphicsSettings() {
        // 重新應用畫面設定
        this.configManager.applyGraphicsConfig();
    }

    applyGameplaySettings() {
        const { gameplay } = this.configManager.config;
        
        // 設置戰鬥速度
        document.documentElement.style.setProperty('--battle-speed', 
            gameplay.battleSpeed === 'slow' ? '1.5s' :
            gameplay.battleSpeed === 'fast' ? '0.5s' : '1s'
        );
        
        // 開關傷害數字
        document.body.classList.toggle('show-damage-numbers', gameplay.damageNumbers);
        
        // 開關戰鬥日誌
        const combatLog = document.getElementById('combat-messages');
        if (combatLog) {
            combatLog.style.display = gameplay.combatLog ? 'block' : 'none';
        }
    }

    // ... 其他現有方法保持不變
}