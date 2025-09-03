class EnemyAI {
    constructor(enemySystem) {
        this.enemySystem = enemySystem;
        this.attackPatterns = this.initializeAttackPatterns();
    }

    initializeAttackPatterns() {
        return {
            // 基礎攻擊模式
            basic: {
                weight: 70,
                getAttack: (enemy, player) => this.getBasicAttack(enemy, player)
            },
            // 防禦模式
            defensive: {
                weight: 20,
                getAttack: (enemy, player) => this.getDefensiveAttack(enemy, player)
            },
            // 狂暴模式
            aggressive: {
                weight: 10,
                getAttack: (enemy, player) => this.getAggressiveAttack(enemy, player)
            }
        };
    }

    decideNextAttack(enemy, player) {
        // 根據敵人和玩家狀態選擇攻擊模式
        const attackType = this.selectAttackType(enemy, player);
        return attackType.getAttack(enemy, player);
    }

    selectAttackType(enemy, player) {
        // 簡單的AI決策邏輯
        const playerHealthPercent = (player.currentHealth / player.maxHealth) * 100;
        const enemyHealthPercent = (enemy.currentHealth / enemy.maxHealth) * 100;

        // 玩家血量低時更傾向攻擊
        if (playerHealthPercent < 30) {
            this.attackPatterns.aggressive.weight = 30;
            this.attackPatterns.basic.weight = 50;
            this.attackPatterns.defensive.weight = 20;
        }
        // 敵人血量低時更傾向防禦
        else if (enemyHealthPercent < 30) {
            this.attackPatterns.defensive.weight = 40;
            this.attackPatterns.basic.weight = 40;
            this.attackPatterns.aggressive.weight = 20;
        }
        // 正常狀態
        else {
            this.attackPatterns.defensive.weight = 20;
            this.attackPatterns.basic.weight = 70;
            this.attackPatterns.aggressive.weight = 10;
        }

        // 根據權重隨機選擇攻擊模式
        const totalWeight = Object.values(this.attackPatterns).reduce((sum, pattern) => sum + pattern.weight, 0);
        let random = Math.random() * totalWeight;
        
        for (const pattern of Object.values(this.attackPatterns)) {
            random -= pattern.weight;
            if (random <= 0) {
                return pattern;
            }
        }

        return this.attackPatterns.basic;
    }

    getBasicAttack(enemy, player) {
        // 基礎攻擊邏輯 - 根據敵人屬性選擇合適的攻擊
        const enemyLevel = this.enemySystem.enemyCounter;
        
        if (enemyLevel < 10) {
            return { type: 'light', description: '輕攻擊' };
        } else if (enemyLevel < 30) {
            return { type: 'medium', description: '中攻擊' };
        } else {
            // 高等級敵人更傾向使用重攻擊
            return Math.random() < 0.6 ? 
                { type: 'heavy', description: '重攻擊' } : 
                { type: 'medium', description: '中攻擊' };
        }
    }

    getDefensiveAttack(enemy, player) {
        // 防禦型攻擊 - 可能包含特殊效果
        const specialChance = Math.random();
        
        if (specialChance < 0.3 && this.enemySystem.enemyCounter > 20) {
            return { 
                type: 'special', 
                description: '防禦姿態',
                effect: 'defense_buff'
            };
        }
        
        return { type: 'medium', description: '謹慎攻擊' };
    }

    getAggressiveAttack(enemy, player) {
        // 狂暴攻擊 - 高傷害但可能低命中
        const specialChance = Math.random();
        
        if (specialChance < 0.4 && this.enemySystem.enemyCounter > 15) {
            return { 
                type: 'special', 
                description: '狂暴打擊',
                effect: 'high_damage'
            };
        }
        
        return { type: 'heavy', description: '猛烈攻擊' };
    }

    calculateEnemyDamage(enemy, attackType, player) {
        let damage = 0;
        let isCrit = false;
        
        switch (attackType.type) {
            case 'light':
                damage = 10 + enemy.attack;
                break;
            case 'medium':
                damage = 18 + enemy.attack;
                isCrit = Math.random() < (0.05 + enemy.critChance / 100);
                break;
            case 'heavy':
                damage = 35 + enemy.attack;
                isCrit = Math.random() < (0.08 + enemy.critChance / 100);
                break;
            case 'special':
                if (attackType.effect === 'high_damage') {
                    damage = 45 + enemy.attack;
                    isCrit = Math.random() < 0.15;
                }
                break;
        }

        // 暴擊傷害
        if (isCrit) {
            damage *= 2;
        }

        // 計算命中率
        const hitChance = Math.max(5, Math.min(95, 70 + enemy.hitChance - player.dodge));
        const isHit = Math.random() * 100 <= hitChance;

        // 計算最終傷害（考慮玩家防禦）
        if (isHit) {
            damage = Math.max(1, damage - player.defense);
            damage = Math.ceil(damage);
        } else {
            damage = 1; // 未命中最低傷害
        }

        return {
            damage,
            isHit,
            isCrit,
            hitChance,
            attackDescription: attackType.description
        };
    }

    // 特殊攻擊效果處理
    handleSpecialAttackEffects(attackType, enemy, player) {
        switch (attackType.effect) {
            case 'defense_buff':
                // 敵人獲得防禦加成
                enemy.defense += 3;
                return '敵人進入防禦姿態，防禦力提升！';
            
            case 'high_damage':
                // 高傷害攻擊的特殊效果
                return '敵人發動狂暴打擊！';
            
            default:
                return '';
        }
    }
}