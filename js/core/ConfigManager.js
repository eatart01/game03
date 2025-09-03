class ConfigManager {
    constructor() {
        this.config = this.getDefaultConfig();
        this.loadConfig();
    }

    getDefaultConfig() {
        return {
            // 遊戲難度設定
            difficulty: {
                level: 'normal', // easy, normal, hard, nightmare
                damageMultiplier: 1.0,
                enemyHealthMultiplier: 1.0,
                rewardMultiplier: 1.0
            },
            
            // 畫面設定
            graphics: {
                quality: 'medium', // low, medium, high
                animations: true,
                particleEffects: true,
                screenShake: true,
                fontSize: 'normal' // small, normal, large
            },
            
            // 音效設定
            audio: {
                masterVolume: 0.8,
                musicVolume: 0.7,
                sfxVolume: 0.8,
                uiVolume: 0.8,
                mute: false
            },
            
            // 遊戲設定
            gameplay: {
                autoSave: true,
                autoSaveInterval: 5, // 分鐘
                battleSpeed: 'normal', // slow, normal, fast
                tooltips: true,
                damageNumbers: true,
                combatLog: true
            },
            
            // 控制設定
            controls: {
                keybinds: {
                    attackLight: '1',
                    attackMedium: '2', 
                    attackHeavy: '3',
                    startBattle: 'Enter',
                    openMenu: 'Escape'
                },
                mouseSensitivity: 1.0,
                touchControls: true
            },
            
            // 界面設定
            interface: {
                theme: 'dark', // dark, light, blue, red
                language: 'zh-TW',
                showFps: false,
                compactMode: false,
                healthBars: true
            }
        };
    }

    loadConfig() {
        try {
            const savedConfig = localStorage.getItem('game_config');
            if (savedConfig) {
                const parsedConfig = JSON.parse(savedConfig);
                this.config = { ...this.getDefaultConfig(), ...parsedConfig };
                console.log('✅ 設定載入成功');
            }
        } catch (error) {
            console.error('❌ 設定載入失敗:', error);
            this.config = this.getDefaultConfig();
        }
        
        this.applyConfig();
    }

    saveConfig() {
        try {
            localStorage.setItem('game_config', JSON.stringify(this.config));
            console.log('✅ 設定保存成功');
            return true;
        } catch (error) {
            console.error('❌ 設定保存失敗:', error);
            return false;
        }
    }

    applyConfig() {
        // 應用畫面設定
        this.applyGraphicsConfig();
        
        // 應用音效設定
        this.applyAudioConfig();
        
        // 應用界面設定
        this.applyInterfaceConfig();
        
        // 發送設定變更事件
        this.dispatchConfigChangeEvent();
    }

    applyGraphicsConfig() {
        const { graphics } = this.config;
        
        // 設置字體大小
        document.documentElement.style.setProperty('--font-size', 
            graphics.fontSize === 'small' ? '12px' :
            graphics.fontSize === 'large' ? '16px' : '14px'
        );
        
        // 開關動畫
        document.documentElement.style.setProperty('--animations-enabled', 
            graphics.animations ? '1' : '0'
        );
        
        // 根據畫質調整效果
        if (graphics.quality === 'low') {
            document.body.classList.add('quality-low');
            document.body.classList.remove('quality-medium', 'quality-high');
        } else if (graphics.quality === 'high') {
            document.body.classList.add('quality-high');
            document.body.classList.remove('quality-low', 'quality-medium');
        } else {
            document.body.classList.add('quality-medium');
            document.body.classList.remove('quality-low', 'quality-high');
        }
    }

    applyAudioConfig() {
        const { audio } = this.config;
        
        // 這裡可以添加音效控制邏輯
        // 目前先記錄設定，實際音效系統實現後再整合
        console.log('音效設定已應用:', audio);
    }

    applyInterfaceConfig() {
        const { interface: ui } = this.config;
        
        // 應用主題
        document.documentElement.setAttribute('data-theme', ui.theme);
        
        // 顯示/隱藏FPS
        const fpsElement = document.querySelector('#fps-display');
        if (fpsElement) {
            fpsElement.style.display = ui.showFps ? 'block' : 'none';
        }
        
        // 緊湊模式
        document.body.classList.toggle('compact-mode', ui.compactMode);
        
        // 生命條顯示
        const healthBars = document.querySelectorAll('.health-bar');
        healthBars.forEach(bar => {
            bar.style.display = ui.healthBars ? 'block' : 'none';
        });
    }

    getDifficultyMultipliers() {
        const { difficulty } = this.config;
        
        const multipliers = {
            easy: { damage: 1.2, health: 0.8, reward: 1.2 },
            normal: { damage: 1.0, health: 1.0, reward: 1.0 },
            hard: { damage: 0.8, health: 1.2, reward: 0.8 },
            nightmare: { damage: 0.6, health: 1.5, reward: 0.6 }
        };
        
        return multipliers[difficulty.level] || multipliers.normal;
    }

    updateSetting(category, key, value) {
        if (this.config[category] && this.config[category][key] !== undefined) {
            this.config[category][key] = value;
            this.applyConfig();
            this.saveConfig();
            return true;
        }
        return false;
    }

    resetToDefaults() {
        this.config = this.getDefaultConfig();
        this.applyConfig();
        this.saveConfig();
        console.log('✅ 設定已重置為預設值');
    }

    dispatchConfigChangeEvent() {
        const event = new CustomEvent('configChanged', {
            detail: { config: this.config }
        });
        document.dispatchEvent(event);
    }

    // 獲取特定設定值
    getSetting(category, key, defaultValue = null) {
        return this.config[category]?.[key] ?? defaultValue;
    }

    // 批量更新設定
    updateSettings(updates) {
        let changed = false;
        
        for (const [category, settings] of Object.entries(updates)) {
            if (this.config[category]) {
                for (const [key, value] of Object.entries(settings)) {
                    if (this.config[category][key] !== undefined) {
                        this.config[category][key] = value;
                        changed = true;
                    }
                }
            }
        }
        
        if (changed) {
            this.applyConfig();
            this.saveConfig();
        }
        
        return changed;
    }

    // 匯出設定
    exportConfig() {
        const dataStr = JSON.stringify(this.config, null, 2);
        const dataUri = 'data:application/json;charset=utf-8,' + encodeURIComponent(dataStr);
        return dataUri;
    }

    // 匯入設定
    importConfig(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => {
                try {
                    const importedConfig = JSON.parse(e.target.result);
                    this.config = { ...this.getDefaultConfig(), ...importedConfig };
                    this.applyConfig();
                    this.saveConfig();
                    resolve(true);
                } catch (error) {
                    reject('設定文件格式錯誤');
                }
            };
            reader.onerror = () => reject('讀取文件失敗');
            reader.readAsText(file);
        });
    }
}