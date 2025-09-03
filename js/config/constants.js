// 游戏常量配置
const CONSTANTS = {
    UI: {
        TALENT_SLOTS_COUNT: 60,
        MAX_HEALTH: 100,
        CARD_TYPES: ['light', 'medium', 'heavy'],
        ATTRIBUTE_NAMES: {
            attack: '攻擊',
            defense: '防禦', 
            hitChance: '命中',
            critChance: '暴擊',
            toughness: '堅韌',
            dodge: '閃避'
        }
    },
    SELECTORS: {
        TALENTS_GRID: '#talents-grid',
        START_BATTLE_BTN: '#start-battle-btn',
        CARD_HAND: '#card-hand',
        ENEMY_HEALTH: '#enemy-container .health-fill',
        PLAYER_HEALTH: '#player-container .health-fill',
        ENEMY_HEALTH_TEXT: '#enemy-container .health-text',
        PLAYER_HEALTH_TEXT: '#player-container .health-text',
        COMBAT_MESSAGES: '#combat-messages',
        ENEMY_NEXT_ATTACK: '#enemy-next-attack',
        PLAYER_STATS: '.entity-stats',
        ENEMY_LEVEL: '.entity-level',
        ENEMY_ATTRIBUTES: '.enemy-attributes'
    }
};

// 游戏配置
const GAME_CONFIG = {
    baseHealth: 100,
    attackTypes: {
        light: { damage: 16, hitChance: 72, critChance: 0 },
        medium: { damage: 24, hitChance: 64, critChance: 5 },
        heavy: { damage: 49, hitChance: 57, critChance: 10 }
    },
    baseStats: {
        attack: 0,
        defense: 0,
        hitChance: 0,
        critChance: 0,
        toughness: 0,
        dodge: 0
    },
    attributeCycle: ['attack', 'defense', 'hitChance', 'critChance', 'toughness', 'dodge']
};