// 游戏主入口文件
// 黑暗魔窟 - 主程序入口
// 版本: 1.3.0

/**
 * 游戏初始化配置
 * 这里定义了游戏启动前的全局配置
 */

};

/**
 * 性能监控器
 * 用于跟踪游戏性能指标
 */
class PerformanceMonitor {
    constructor() {
        this.fps = 0;
        this.frameCount = 0;
        this.lastTime = performance.now();
        this.fpsElement = null;
        this.stats = {
            totalBattles: 0,
            enemiesDefeated: 0,
            totalDamage: 0,
            criticalHits: 0,
            playerDeaths: 0,
            talentsActivated: 0
        };
    }

    init() {
        if (GAME_CONFIG.debugMode) {
            this.fpsElement = document.createElement('div');
            this.fpsElement.style.position = 'fixed';
            this.fpsElement.style.top = '10px';
            this.fpsElement.style.right = '10px';
            this.fpsElement.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
            this.fpsElement.style.color = 'white';
            this.fpsElement.style.padding = '5px 10px';
            this.fpsElement.style.borderRadius = '3px';
            this.fpsElement.style.fontFamily = 'monospace';
            this.fpsElement.style.zIndex = '1000';
            this.fpsElement.textContent = 'FPS: 0';
            document.body.appendChild(this.fpsElement);
        }
    }

    update() {
        if (!GAME_CONFIG.debugMode) return;

        this.frameCount++;
        const currentTime = performance.now();
        
        if (currentTime - this.lastTime >= 1000) {
            this.fps = Math.round((this.frameCount * 1000) / (currentTime - this.lastTime));
            this.frameCount = 0;
            this.lastTime = currentTime;
            
            if (this.fpsElement) {
                this.fpsElement.textContent = `FPS: ${this.fps} | 敵人: ${this.stats.enemiesDefeated} | 傷害: ${this.stats.totalDamage}`;
            }
        }
    }

    recordBattle() {
        this.stats.totalBattles++;
    }

    recordEnemyDefeated() {
        this.stats.enemiesDefeated++;
    }

    recordDamage(amount, isCrit = false) {
        this.stats.totalDamage += amount;
        if (isCrit) {
            this.stats.criticalHits++;
        }
    }

    recordPlayerDeath() {
        this.stats.playerDeaths++;
    }

    recordTalentActivated() {
        this.stats.talentsActivated++;
    }

    getStats() {
        return { ...this.stats };
    }

    resetStats() {
        this.stats = {
            totalBattles: 0,
            enemiesDefeated: 0,
            totalDamage: 0,
            criticalHits: 0,
            playerDeaths: 0,
            talentsActivated: 0
        };
    }
}

/**
 * 错误处理器
 * 捕获并处理游戏中的错误
 */
class ErrorHandler {
    static init() {
        // 捕获全局错误
        window.addEventListener('error', (event) => {
            console.error('全局错误:', event.error);
            this.showError('发生未知错误，请刷新页面重试');
        });

        // 捕获Promise rejection
        window.addEventListener('unhandledrejection', (event) => {
            console.error('Promise错误:', event.reason);
            this.showError('异步操作发生错误');
        });

        // 捕获资源加载错误
        window.addEventListener('load', () => {
            const resources = performance.getEntriesByType('resource');
            const failedResources = resources.filter(res => 
                res.duration === 0 || res.transferSize === 0
            );
            
            if (failedResources.length > 0) {
                console.warn('资源加载失败:', failedResources);
            }
        });
    }

    static showError(message) {
        // 创建错误提示UI
        const errorDiv = document.createElement('div');
        errorDiv.style.position = 'fixed';
        errorDiv.style.top = '20px';
        errorDiv.style.left = '50%';
        errorDiv.style.transform = 'translateX(-50%)';
        errorDiv.style.backgroundColor = '#ff4757';
        errorDiv.style.color = 'white';
        errorDiv.style.padding = '10px 20px';
        errorDiv.style.borderRadius = '5px';
        errorDiv.style.zIndex = '10000';
        errorDiv.style.fontFamily = 'Microsoft JhengHei, sans-serif';
        errorDiv.textContent = `错误: ${message}`;
        
        document.body.appendChild(errorDiv);
        
        // 3秒后自动消失
        setTimeout(() => {
            if (document.body.contains(errorDiv)) {
                document.body.removeChild(errorDiv);
            }
        }, 3000);
    }
}

/**
 * 游戏保存系统
 * 处理游戏数据的保存和加载
 */
class SaveSystem {
    static saveKey = 'dark_caverns_save_data';

    static saveGame(gameData) {
        try {
            const saveData = {
                timestamp: Date.now(),
                version: GAME_CONFIG.version,
                data: gameData
            };
            localStorage.setItem(this.saveKey, JSON.stringify(saveData));
            console.log('游戏已保存', saveData);
            return true;
        } catch (error) {
            console.error('保存失败:', error);
            return false;
        }
    }

    static loadGame() {
        try {
            const saved = localStorage.getItem(this.saveKey);
            if (!saved) return null;

            const saveData = JSON.parse(saved);
            
            // 检查版本兼容性
            if (saveData.version !== GAME_CONFIG.version) {
                console.warn('存档版本不匹配，可能需要进行数据迁移');
                this.handleVersionMigration(saveData);
            }

            return saveData.data;
        } catch (error) {
            console.error('加载失败:', error);
            return null;
        }
    }

    static handleVersionMigration(saveData) {
        // 版本迁移逻辑
        console.log(`正在迁移存档从版本 ${saveData.version} 到 ${GAME_CONFIG.version}`);
        
        // 这里可以添加具体的版本迁移逻辑
        // 例如：重命名字段、转换数据格式等
        
        return saveData.data;
    }

    static deleteSave() {
        try {
            localStorage.removeItem(this.saveKey);
            console.log('存档已删除');
            return true;
        } catch (error) {
            console.error('删除存档失败:', error);
            return false;
        }
    }

    static hasSave() {
        return localStorage.getItem(this.saveKey) !== null;
    }

    static getSaveInfo() {
        const saved = localStorage.getItem(this.saveKey);
        if (!saved) return null;
        
        try {
            const saveData = JSON.parse(saved);
            return {
                timestamp: new Date(saveData.timestamp),
                version: saveData.version,
                size: new Blob([saved]).size
            };
        } catch (error) {
            return null;
        }
    }

    static exportSave() {
        const saveData = localStorage.getItem(this.saveKey);
        if (!saveData) return null;

        const dataStr = JSON.stringify(JSON.parse(saveData), null, 2);
        const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr);
        return dataUri;
    }

    static importSave(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = function(e) {
                try {
                    const saveData = JSON.parse(e.target.result);
                    if (saveData && saveData.data) {
                        localStorage.setItem(SaveSystem.saveKey, JSON.stringify(saveData));
                        resolve(true);
                    } else {
                        reject('无效的存档文件');
                    }
                } catch (error) {
                    reject('存档文件格式错误');
                }
            };
            reader.onerror = reject;
            reader.readAsText(file);
        });
    }
}

/**
 * 游戏音频管理器
 * 处理游戏音效（预留接口）
 */
class AudioManager {
    static init() {
        // 音频系统预留
        console.log('音频系统初始化（预留）');
    }

    static playSound(soundName) {
        // 音频播放预留
        if (GAME_CONFIG.debugMode) {
            console.log('播放音效:', soundName);
        }
    }

    static playBattleSound() {
        this.playSound('battle');
    }

    static playVictorySound() {
        this.playSound('victory');
    }

    static playDefeatSound() {
        this.playSound('defeat');
    }

    static playTalentSound() {
        this.playSound('talent');
    }

    static playRewardSound() {
        this.playSound('reward');
    }
}

/**
 * 游戏初始化流程
 */
class GameInitializer {
    static async initialize() {
        console.log('🎮 开始初始化黑暗魔窟游戏...');
        console.log(`🔄 游戏版本: ${GAME_CONFIG.version}`);

        try {
            // 1. 初始化错误处理
            ErrorHandler.init();
            console.log('✅ 错误处理系统初始化完成');

            // 2. 初始化性能监控
            window.performanceMonitor = new PerformanceMonitor();
            window.performanceMonitor.init();
            console.log('✅ 性能监控初始化完成');

            // 3. 初始化音频系统
            AudioManager.init();
            console.log('✅ 音频系统初始化完成');

            // 4. 检查存档
            const hasSave = SaveSystem.hasSave();
            if (hasSave) {
                const saveInfo = SaveSystem.getSaveInfo();
                console.log('💾 发现存档:', saveInfo);
            } else {
                console.log('💾 无存档');
            }

            // 5. 初始化游戏管理器
            await this.initializeGameManager();

            // 6. 启动游戏循环
            this.startGameLoop();

            console.log('🎉 游戏初始化完成！');
            this.showWelcomeMessage();

            // 7. 触发就绪事件
            this.triggerGameReady();

        } catch (error) {
            console.error('❌ 游戏初始化失败:', error);
            ErrorHandler.showError('游戏初始化失败，请刷新页面');
        }
    }

    static async initializeGameManager() {
        return new Promise((resolve) => {
            // 等待DOM完全加载
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', () => {
                    this.createGameManager();
                    resolve();
                });
            } else {
                this.createGameManager();
                resolve();
            }
        });
    }

    static createGameManager() {
        try {
            window.game = new GameManager();
            console.log('✅ 游戏管理器初始化完成');
            
            // 绑定全局访问
            window.GameManager = GameManager;
            window.EnemySystem = EnemySystem;
            window.BattleSystem = BattleSystem;
            window.Entity = Entity;
            window.TalentSystem = TalentSystem;
            window.RewardSystem = RewardSystem;
            window.EnemyAI = EnemyAI;
            
        } catch (error) {
            console.error('❌ 游戏管理器创建失败:', error);
            throw error;
        }
    }

    static startGameLoop() {
        let lastTime = 0;
        
        const gameLoop = (timestamp) => {
            const deltaTime = lastTime ? timestamp - lastTime : 0;
            lastTime = timestamp;

            // 更新性能监控
            if (window.performanceMonitor) {
                window.performanceMonitor.update();
            }

            // 这里可以添加每帧需要更新的逻辑
            if (window.game && window.game.update) {
                window.game.update(deltaTime);
            }

            requestAnimationFrame(gameLoop);
        };

        requestAnimationFrame(gameLoop);
        console.log('🔄 游戏循环已启动');
    }

    static showWelcomeMessage() {
        if (GAME_CONFIG.debugMode) {
            const welcomeMessage = `
╔══════════════════════════════════════╗
║            黑暗魔窟 🎮               ║
║                                      ║
║  版本: ${GAME_CONFIG.version.padEnd(24)} ║
║  模式: ${(GAME_CONFIG.debugMode ? '调试模式' : '正式模式').padEnd(24)} ║
║                                      ║
║  输入 game 访问游戏管理器             ║
║  输入 help() 查看调试命令            ║
║  输入 stats() 查看游戏统计           ║
║  输入 save() 手动保存游戏            ║
║  输入 load() 手动加载游戏            ║
╚══════════════════════════════════════╝
            `;
            console.log(welcomeMessage);
        }
    }

    static triggerGameReady() {
        // 触发自定义事件，通知其他组件游戏已就绪
        const event = new CustomEvent('gameReady', {
            detail: {
                version: GAME_CONFIG.version,
                timestamp: new Date()
            }
        });
        document.dispatchEvent(event);
        
        // 更新页面状态
        document.documentElement.setAttribute('data-game-status', 'ready');
        document.documentElement.classList.add('game-loaded');
        
        // 隐藏加载画面
        const loadingScreen = document.getElementById('loading-screen');
        const gameContainer = document.getElementById('game-container');
        if (loadingScreen && gameContainer) {
            loadingScreen.style.display = 'none';
            gameContainer.style.display = 'grid';
        }
    }
}

/**
 * 调试工具函数
 */
function help() {
    const commands = `
调试命令列表:
- game: 访问游戏管理器实例
- enemySystem: 访问敌人系统实例
- talentSystem: 访问天赋系统实例
- rewardSystem: 访问奖励系统实例
- stats(): 查看游戏统计数据
- save(): 手动保存游戏
- load(): 手动加载游戏
- resetGame(): 重置游戏
- toggleDebug(): 切换调试模式
- exportSave(): 导出存档
- importSave(file): 导入存档

示例:
// 查看当前游戏状态
console.log(game);

// 查看敌人系统状态
console.log(enemySystem);

// 查看统计数据
stats();

// 手动保存游戏
save();

// 重置游戏
resetGame();
    `;
    console.log(commands);
}

function stats() {
    if (window.performanceMonitor) {
        const stats = window.performanceMonitor.getStats();
        console.log('📊 游戏统计数据:', stats);
        return stats;
    } else {
        console.log('❌ 性能监控器未初始化');
        return null;
    }
}

function save() {
    if (window.game && window.game.saveGame) {
        const success = window.game.saveGame();
        if (success) {
            console.log('✅ 游戏保存成功');
        } else {
            console.log('❌ 游戏保存失败');
        }
        return success;
    }
    console.log('❌ 游戏管理器未初始化');
    return false;
}

function load() {
    if (window.game && window.game.loadGame) {
        const success = window.game.loadGame();
        if (success) {
            console.log('✅ 游戏加载成功');
        } else {
            console.log('❌ 游戏加载失败');
        }
        return success;
    }
    console.log('❌ 游戏管理器未初始化');
    return false;
}

function resetGame() {
    if (confirm('确定要重置游戏吗？所有进度将会丢失！')) {
        SaveSystem.deleteSave();
        location.reload();
    }
}

function toggleDebug() {
    GAME_CONFIG.debugMode = !GAME_CONFIG.debugMode;
    console.log(`调试模式 ${GAME_CONFIG.debugMode ? '开启' : '关闭'}`);
    
    if (window.performanceMonitor && window.performanceMonitor.fpsElement) {
        window.performanceMonitor.fpsElement.style.display = 
            GAME_CONFIG.debugMode ? 'block' : 'none';
    }
}

function exportSave() {
    const dataUri = SaveSystem.exportSave();
    if (dataUri) {
        const exportFileDefaultName = `dark-caverns-save-${Date.now()}.json`;
        
        const linkElement = document.createElement('a');
        linkElement.setAttribute('href', dataUri);
        linkElement.setAttribute('download', exportFileDefaultName);
        linkElement.click();
        console.log('✅ 存档导出成功');
        return true;
    } else {
        console.log('❌ 没有找到存档');
        return false;
    }
}

function importSave(file) {
    if (!file) {
        console.log('❌ 请提供存档文件');
        return false;
    }
    
    SaveSystem.importSave(file)
        .then(() => {
            console.log('✅ 存档导入成功');
            if (load()) {
                console.log('✅ 游戏已重新加载');
            }
        })
        .catch(error => {
            console.log('❌ 存档导入失败:', error);
        });
}

function resetStats() {
    if (window.performanceMonitor) {
        window.performanceMonitor.resetStats();
        console.log('✅ 统计数据已重置');
        return true;
    }
    console.log('❌ 性能监控器未初始化');
    return false;
}

// 导出全局函数
window.help = help;
window.stats = stats;
window.save = save;
window.load = load;
window.resetGame = resetGame;
window.toggleDebug = toggleDebug;
window.exportSave = exportSave;
window.importSave = importSave;
window.resetStats = resetStats;

// 自动启动游戏
startGame().catch(error => {
    console.error('游戏启动失败:', error);
    ErrorHandler.showError('游戏启动失败，请刷新页面');
});

/**
 * 游戏主启动函数
 */
async function startGame() {
    console.log('🚀 启动黑暗魔窟游戏...');

    // 检查浏览器兼容性
    if (!checkCompatibility()) {
        return;
    }

    // 注册Service Worker（可选）
    await registerServiceWorker();

    // 初始化游戏
    await GameInitializer.initialize();

    // 游戏启动完成
    console.log('🎯 游戏启动完成！');
    document.documentElement.setAttribute('data-game-status', 'loaded');
}

/**
 * 浏览器兼容性检查
 */
function checkCompatibility() {
    const features = {
        localStorage: !!window.localStorage,
        requestAnimationFrame: !!window.requestAnimationFrame,
        performance: !!window.performance,
        console: !!window.console,
        classes: !!window.class,
        promise: !!window.Promise,
        arrowFunctions: !!(() => {}),
        templateLiterals: !!`${'test'}`
    };

    const missingFeatures = Object.entries(features)
        .filter(([_, supported]) => !supported)
        .map(([feature]) => feature);

    if (missingFeatures.length > 0) {
        const message = `您的浏览器不支持以下功能: ${missingFeatures.join(', ')}\n游戏可能无法正常运行。`;
        console.warn(message);
        alert(message);
        return false;
    }

    return true;
}

/**
 * 服务工作者注册（PWA支持）
 */
async function registerServiceWorker() {
    if ('serviceWorker' in navigator) {
        try {
            const registration = await navigator.serviceWorker.register('/sw.js');
            console.log('ServiceWorker 注册成功:', registration);
            return registration;
        } catch (error) {
            console.log('ServiceWorker 注册失败:', error);
            return null;
        }
    }
    return null;
}

/**
 * 游戏关闭清理
 */
function cleanupGame() {
    console.log('🧹 清理游戏资源...');
    
    // 这里可以添加资源清理逻辑
    if (window.performanceMonitor && window.performanceMonitor.fpsElement) {
        document.body.removeChild(window.performanceMonitor.fpsElement);
    }
    
    // 移除事件监听器等其他清理工作
}

/**
 * 页面卸载前的处理
 */
window.addEventListener('beforeunload', (event) => {
    if (window.game && window.game.isBattleActive) {
        // 如果战斗中进行刷新，提示用户
        event.preventDefault();
        event.returnValue = '战斗正在进行中，确定要离开吗？';
        return '战斗正在进行中，确定要离开吗？';
    }
    
    cleanupGame();
});

/**
 * 页面可见性变化处理
 */
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        console.log('游戏切换到后台');
        // 游戏暂停逻辑可以在这里实现
    } else {
        console.log('游戏恢复到前台');
        // 游戏恢复逻辑可以在这里实现
    }
});

/**
 * 键盘快捷键
 */
document.addEventListener('keydown', (event) => {
    // 禁用F12开发者工具（可选）
    if (event.key === 'F12') {
        if (!GAME_CONFIG.debugMode) {
            event.preventDefault();
        }
    }
    
    // 其他快捷键可以在这里添加
    if (event.ctrlKey && event.key === 's') {
        event.preventDefault();
        console.log('手动保存快捷键被触发');
        save();
    }
    
    if (event.ctrlKey && event.key === 'l') {
        event.preventDefault();
        console.log('手动加载快捷键被触发');
        load();
    }
});