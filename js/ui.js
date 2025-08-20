// UI初始化函数
function initUI() {
    // 初始化天赋格子
    initTalentsGrid();
    
    // 初始化攻击卡片
    generateInitialCards();
    
    // 初始化游戏信息
    updateGameInfo();
    
    // 初始化血量显示
    updateHP();
    
    // 隐藏不需要显示的元素
    document.getElementById('turn-indicator').style.display = 'none';
    document.getElementById('start-button').style.display = 'none';
    document.getElementById('continue-button').style.display = 'none';
    document.getElementById('minigame-action-button').style.display = 'none';
    document.getElementById('parry-minigame').style.display = 'none';
    document.getElementById('ultra-sensation-minigame').style.display = 'none';
    
    // 设置天赋提示功能
    setupTalentTooltips();
    
    console.log("UI初始化完成 - 纯外观模式");
}

// 初始化天赋格子
function initTalentsGrid() {
    const talentsGrid = document.getElementById('talents-grid');
    talentsGrid.innerHTML = '';
    
    for (let i = 0; i < 100; i++) {
        const cell = document.createElement('div');
        const talentIndex = i + 1;
        
        cell.classList.add('talent-cell');
        cell.textContent = talentIndex;
        cell.dataset.index = talentIndex;
        
        // 根据序号设置不同的视觉状态（仅外观）
        if (talentIndex <= 5) {
            cell.classList.add('available');
        } else if (talentIndex <= 10) {
            cell.classList.add('activated');
        } else {
            cell.classList.add('locked');
        }
        
        talentsGrid.appendChild(cell);
    }
}

// 设置天赋提示
function setupTalentTooltips() {
    const talentCells = document.querySelectorAll('.talent-cell');
    const tooltip = document.getElementById('talent-tooltip');
    
    talentCells.forEach(cell => {
        cell.addEventListener('mouseover', function(e) {
            const index = e.target.dataset.index;
            if (index) {
                tooltip.textContent = `天赋 ${index}: 这是一个功能强大的天賦效果！`;
                tooltip.style.display = 'block';
                tooltip.style.left = `${e.target.offsetLeft + e.target.offsetWidth + 5}px`;
                tooltip.style.top = `${e.target.offsetTop + e.target.offsetHeight / 2}px`;
            }
        });
        
        cell.addEventListener('mouseout', function() {
            tooltip.style.display = 'none';
        });
    });
}

// 生成初始攻击卡片（仅外观）
function generateInitialCards() {
    const attackCardsContainer = document.getElementById('attack-cards');
    attackCardsContainer.innerHTML = '';
    
    const cardTypes = [
        { type: 'light', name: '輕攻擊', damage: 16, hitChance: 82, critChance: 5 },
        { type: 'medium', name: '中攻擊', damage: 24, hitChance: 74, critChance: 10 },
        { type: 'heavy', name: '重攻擊', damage: 30, hitChance: 67, critChance: 30 }
    ];
    
    cardTypes.forEach(cardData => {
        const cardElement = document.createElement('div');
        cardElement.classList.add('card', cardData.type);
        cardElement.dataset.type = cardData.type;
        
        cardElement.innerHTML = `
            <div class="card-mask"></div>
            <h3>${cardData.name}</h3>
            <p>命中率: ${cardData.hitChance}%</p>
            <p>暴擊率: ${cardData.critChance}%</p>
            <div class="damage">${cardData.damage}</div>
            <p>傷害</p>
            <p style="font-size: 10px; margin-top: 5px; color: #ff6666;">失手可能承受反擊</p>
        `;
        
        // 添加悬停效果（仅视觉）
        cardElement.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
            this.style.boxShadow = '0 8px 25px rgba(42, 140, 255, 0.4)';
            this.style.borderColor = '#2a8cff';
        });
        
        cardElement.addEventListener('mouseleave', function() {
            this.style.transform = '';
            this.style.boxShadow = '';
            this.style.borderColor = '#2a2a6a';
        });
        
        attackCardsContainer.appendChild(cardElement);
    });
}

// 更新游戏信息（静态显示）
function updateGameInfo() {
    document.getElementById('turn-counter').textContent = '1';
    document.getElementById('player-level').textContent = '0';
    document.getElementById('player-level-display').textContent = '0';
    document.getElementById('enemy-id').textContent = '001';
    document.getElementById('enemy-id-display').textContent = '001';
    document.getElementById('available-points').textContent = '0';
}

// 更新血量显示（静态显示）
function updateHP() {
    document.getElementById('player-hp-bar').style.width = '100%';
    document.getElementById('player-hp-text').textContent = '100/100';
    document.getElementById('enemy-hp-bar').style.width = '100%';
    document.getElementById('enemy-hp-text').textContent = '100/100';
}

// 页面加载完成后初始化UI
document.addEventListener('DOMContentLoaded', function() {
    // 延迟一点执行以确保所有元素都已加载
    setTimeout(initUI, 100);
});