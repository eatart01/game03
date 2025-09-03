class EnemySystem {
    constructor() {
        this.enemyCounter = 1;
        this.attributeCycle = ['attack', 'defense', 'hitChance', 'critChance', 'toughness', 'dodge'];
        this.cycleIndex = 0;
        this.enemyAttributes = this.initializeEnemyAttributes();
    }

    initializeEnemyAttributes() {
        // 敵人001的基礎屬性
        return {
            attack: 0,
            defense: 0,
            hitChance: 0,
            critChance: 0,
            toughness: 0,
            dodge: 0
        };
    }

    createEnemy(level) {
        const baseHealth = 100 + (level - 1) * 30;
        
        // 計算屬性成長
        const attributes = this.calculateEnemyAttributes(level);
        
        return new Entity({
            maxHealth: baseHealth,
            ...attributes
        });
    }

    calculateEnemyAttributes(level) {
        if (level === 1) {
            return this.initializeEnemyAttributes();
        }

        // 複製當前屬性
        const attributes = { ...this.enemyAttributes };

        // 計算應該增長的屬性索引
        const growthCount = level - 2; // 敵人002開始成長
        if (growthCount >= 0) {
            const attributeIndex = growthCount % this.attributeCycle.length;
            const attributeToIncrease = this.attributeCycle[attributeIndex];
            attributes[attributeToIncrease] += 1;
        }

        return attributes;
    }

    getNextEnemy() {
        this.enemyCounter++;
        
        // 更新全局屬性記錄
        if (this.enemyCounter > 1) {
            const growthCount = this.enemyCounter - 2;
            if (growthCount >= 0) {
                const attributeIndex = growthCount % this.attributeCycle.length;
                const attributeToIncrease = this.attributeCycle[attributeIndex];
                this.enemyAttributes[attributeToIncrease] += 1;
            }
        }

        return this.createEnemy(this.enemyCounter);
    }

    getCurrentEnemyAttributes() {
        return { ...this.enemyAttributes };
    }

    getAttributeCyclePosition() {
        const growthCount = Math.max(0, this.enemyCounter - 2);
        const cyclePosition = growthCount % this.attributeCycle.length;
        return {
            currentAttribute: this.attributeCycle[cyclePosition],
            nextAttribute: this.attributeCycle[(cyclePosition + 1) % this.attributeCycle.length],
            cycleIndex: cyclePosition,
            totalCycle: this.attributeCycle.length
        };
    }

    getAttributeName(attribute) {
        const names = {
            attack: '攻擊力',
            defense: '防禦力',
            hitChance: '命中率',
            critChance: '暴擊率',
            toughness: '堅韌',
            dodge: '閃避率'
        };
        return names[attribute] || attribute;
    }

    reset() {
        this.enemyCounter = 1;
        this.cycleIndex = 0;
        this.enemyAttributes = this.initializeEnemyAttributes();
    }
}