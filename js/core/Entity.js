class Entity {
    constructor(baseStats = {}) {
        this.maxHealth = baseStats.maxHealth || 100;
        this.currentHealth = this.maxHealth;
        this.attack = baseStats.attack || 0;
        this.defense = baseStats.defense || 0;
        this.hitChance = baseStats.hitChance || 0;
        this.critChance = baseStats.critChance || 0;
        this.toughness = baseStats.toughness || 0;
        this.dodge = baseStats.dodge || 0;
    }

    takeDamage(amount) {
        this.currentHealth = Math.max(0, this.currentHealth - amount);
        return this.currentHealth;
    }

    heal(amount) {
        this.currentHealth = Math.min(this.maxHealth, this.currentHealth + amount);
        return this.currentHealth;
    }

    increaseMaxHealth(amount) {
        this.maxHealth += amount;
        return this.maxHealth;
    }

    isAlive() {
        return this.currentHealth > 0;
    }

    getHealthPercentage() {
        return (this.currentHealth / this.maxHealth) * 100;
    }

    getStats() {
        return {
            attack: this.attack,
            defense: this.defense,
            hitChance: this.hitChance,
            critChance: this.critChance,
            toughness: this.toughness,
            dodge: this.dodge
        };
    }
}