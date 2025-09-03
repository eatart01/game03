class BattleSystem {
    static calculateDamage(attacker, defender, attackType) {
        const attackConfig = GAME_CONFIG.attackTypes[attackType];
        if (!attackConfig) return { damage: 0, isHit: false, isCrit: false };

        // 計算命中率 (5%-95%範圍限制)
        let hitRate = attackConfig.hitChance + attacker.hitChance - defender.dodge;
        hitRate = Math.max(5, Math.min(95, hitRate));
        
        const isHit = Math.random() * 100 <= hitRate;
        if (!isHit) {
            return { 
                damage: 1, 
                isHit: false, 
                isCrit: false, 
                hitRate,
                critRate: 0
            };
        }

        // 計算暴擊率 (5%-95%範圍限制)
        let critRate = attackConfig.critChance + attacker.critChance - defender.toughness;
        critRate = Math.max(5, Math.min(95, critRate));
        const isCrit = Math.random() * 100 <= critRate;

        // 計算基礎傷害
        let damage = attackConfig.damage + attacker.attack;
        if (isCrit) {
            damage = damage * 2;
        }

        // 計算防禦減傷
        damage = Math.max(1, damage - defender.defense);
        damage = Math.ceil(damage);

        return {
            damage,
            isHit: true,
            isCrit,
            hitRate,
            critRate
        };
    }

    static getAttackName(type) {
        const names = { 
            light: '輕', 
            medium: '中', 
            heavy: '重' 
        };
        return names[type] || '未知';
    }
}