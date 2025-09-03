class RewardSystem {
    constructor() {
        this.rewardOptions = [];
        this.initializeRewards();
    }

    initializeRewards() {
        this.allRewards = [
            // 基礎屬性獎勵
            {
                id: 1,
                name: "精準訓練",
                description: "+1命中率",
                type: "attribute",
                effect: { hitChance: 1 },
                rarity: "common"
            },
            {
                id: 2,
                name: "攻擊提升",
                description: "+1攻擊力", 
                type: "attribute",
                effect: { attack: 1 },
                rarity: "common"
            },
            {
                id: 3,
                name: "防禦提升",
                description: "+1防禦力",
                type: "attribute",
                effect: { defense: 1 },
                rarity: "common"
            },
            {
                id: 4,
                name: "暴擊訓練",
                description: "+1暴擊率",
                type: "attribute", 
                effect: { critChance: 1 },
                rarity: "common"
            },
            {
                id: 5,
                name: "堅韌鍛鍊",
                description: "+1堅韌",
                type: "attribute",
                effect: { toughness: 1 },
                rarity: "common"
            },
            {
                id: 6,
                name: "閃避訓練",
                description: "+1閃避率",
                type: "attribute",
                effect: { dodge: 1 },
                rarity: "common"
            },

            // 生命相關獎勵
            {
                id: 7,
                name: "生命恢復",
                description: "恢復30生命",
                type: "heal",
                effect: { heal: 30 },
                rarity: "common"
            },
            {
                id: 8,
                name: "最大生命提升",
                description: "最大生命+5",
                type: "maxHealth",
                effect: { maxHealth: 5 },
                rarity: "common"
            },
            {
                id: 9,
                name: "每回合恢復",
                description: "+1每回合恢復生命",
                type: "passive",
                effect: { healPerTurn: 1 },
                rarity: "uncommon"
            },

            // 特殊獎勵
            {
                id: 10,
                name: "戰鬥恢復",
                description: "擊敗敵人時恢復生命+5",
                type: "passive",
                effect: { healOnVictory: 5 },
                rarity: "uncommon"
            },
            {
                id: 11,
                name: "臨時護盾",
                description: "獲得10護盾",
                type: "shield",
                effect: { shield: 10 },
                rarity: "uncommon"
            },

            // 負面效果獎勵（高風險高回報）
            {
                id: 12,
                name: "狂暴姿態",
                description: "+2攻擊力 -1防禦",
                type: "mixed",
                effect: { attack: 2, defense: -1 },
                rarity: "uncommon"
            },
            {
                id: 13,
                name: "犧牲防禦",
                description: "+2命中 -5最大生命 -1防禦",
                type: "mixed",
                effect: { hitChance: 2, maxHealth: -5, defense: -1 },
                rarity: "rare"
            },
            {
                id: 14,
                name: "生命轉化",
                description: "-1暴擊率 +20最大生命",
                type: "mixed",
                effect: { critChance: -1, maxHealth: 20 },
                rarity: "rare"
            }
        ];
    }

    getRandomRewards(count = 5, enemyLevel = 1) {
        // 根據敵人等級調整獎勵品質
        let availableRewards = [...this.allRewards];
        
        // 過濾掉不適合低等級的獎勵
        if (enemyLevel < 5) {
            availableRewards = availableRewards.filter(reward => 
                reward.rarity !== "rare" && !reward.effect.maxHealth || reward.effect.maxHealth > 0
            );
        }

        // 隨機選擇獎勵
        const selectedRewards = [];
        const usedIds = new Set();

        while (selectedRewards.length < count && availableRewards.length > 0) {
            const randomIndex = Math.floor(Math.random() * availableRewards.length);
            const reward = availableRewards[randomIndex];
            
            if (!usedIds.has(reward.id)) {
                selectedRewards.push(reward);
                usedIds.add(reward.id);
                availableRewards.splice(randomIndex, 1);
            }
        }

        return selectedRewards;
    }

    applyReward(reward, player) {
        const effect = reward.effect;
        let message = "";

        Object.keys(effect).forEach(key => {
            const value = effect[key];
            
            switch (key) {
                case 'attack':
                    player.attack += value;
                    message += `攻擊力${value >= 0 ? '+' : ''}${value} `;
                    break;
                case 'defense':
                    player.defense += value;
                    message += `防禦力${value >= 0 ? '+' : ''}${value} `;
                    break;
                case 'hitChance':
                    player.hitChance += value;
                    message += `命中率${value >= 0 ? '+' : ''}${value}% `;
                    break;
                case 'critChance':
                    player.critChance += value;
                    message += `暴擊率${value >= 0 ? '+' : ''}${value}% `;
                    break;
                case 'toughness':
                    player.toughness += value;
                    message += `堅韌${value >= 0 ? '+' : ''}${value} `;
                    break;
                case 'dodge':
                    player.dodge += value;
                    message += `閃避率${value >= 0 ? '+' : ''}${value}% `;
                    break;
                case 'maxHealth':
                    player.maxHealth += value;
                    player.currentHealth = Math.min(player.currentHealth, player.maxHealth);
                    message += `最大生命${value >= 0 ? '+' : ''}${value} `;
                    break;
                case 'heal':
                    player.heal(value);
                    message += `恢復${value}生命 `;
                    break;
                case 'healPerTurn':
                    // 需要額外系統支持，暫時記錄
                    player.healPerTurn = (player.healPerTurn || 0) + value;
                    message += `每回合恢復+${value} `;
                    break;
                case 'healOnVictory':
                    player.healOnVictory = (player.healOnVictory || 0) + value;
                    message += `擊敗恢復+${value} `;
                    break;
                case 'shield':
                    player.shield = (player.shield || 0) + value;
                    message += `獲得${value}護盾 `;
                    break;
            }
        });

        return message.trim();
    }

    createRewardElement(reward, index) {
        const element = document.createElement('div');
        element.className = `reward-option ${reward.rarity}`;
        element.innerHTML = `
            <div class="reward-header">
                <span class="reward-name">${reward.name}</span>
                <span class="reward-rarity ${reward.rarity}">${this.getRarityName(reward.rarity)}</span>
            </div>
            <div class="reward-description">${reward.description}</div>
            <div class="reward-type">${this.getTypeName(reward.type)}</div>
        `;
        element.dataset.rewardId = reward.id;
        return element;
    }

    getRarityName(rarity) {
        const names = {
            common: '普通',
            uncommon: '稀有', 
            rare: '史詩'
        };
        return names[rarity] || rarity;
    }

    getTypeName(type) {
        const names = {
            attribute: '屬性提升',
            heal: '生命恢復',
            maxHealth: '生命提升',
            passive: '被動效果',
            shield: '護盾',
            mixed: '混合效果'
        };
        return names[type] || type;
    }
}