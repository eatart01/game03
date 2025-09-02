using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 遊戲主控制器 - 負責初始化、場景管理和全局遊戲流程
/// </summary>
public class Main : MonoBehaviour
{
    // 單例模式
    public static Main Instance { get; private set; }
    
    [Header("遊戲設置")]
    public string mainSceneName = "MainScene";
    public string battleSceneName = "BattleScene";
    public string menuSceneName = "MenuScene";
    
    [Header("系統預製體")]
    public GameObject playerProgressPrefab;
    public GameObject uiManagerPrefab;
    public GameObject battleFlowPrefab;
    public GameObject talentManagerPrefab;
    public GameObject deckManagerPrefab;
    public GameObject comboManagerPrefab;
    
    // 系統實例引用
    private PlayerProgress playerProgress;
    private UIManager uiManager;
    private BattleFlowManager battleFlow;
    private TalentManager talentManager;
    private DeckManager deckManager;
    private ComboManager comboManager;
    private BattleResolver battleResolver;
    
    // 遊戲狀態
    private GameState currentGameState = GameState.Menu;
    public enum GameState
    {
        Menu,           // 主菜單
        Initializing,   // 初始化中
        Exploring,      // 探索中（主場景）
        Battling,       // 戰鬥中
        Paused,         // 暫停
        GameOver,       // 遊戲結束
        Victory         // 遊戲勝利
    }
    
    // 事件
    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGameInitialized;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        HandleGlobalInput();
    }
    
    /// <summary>
    /// 初始化遊戲
    /// </summary>
    private void InitializeGame()
    {
        SetGameState(GameState.Initializing);
        Debug.Log("遊戲初始化開始...");
        
        // 確保在正確的場景
        if (SceneManager.GetActiveScene().name != mainSceneName)
        {
            StartCoroutine(LoadMainScene());
        }
        else
        {
            InitializeAllSystems();
        }
    }
    
    /// <summary>
    /// 加載主場景
    /// </summary>
    private IEnumerator LoadMainScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainSceneName);
        
        while (!asyncLoad.isDone)
        {
            // 可以在這裡顯示加載進度
            yield return null;
        }
        
        InitializeAllSystems();
    }
    
    /// <summary>
    /// 初始化所有系統
    /// </summary>
    private void InitializeAllSystems()
    {
        Debug.Log("初始化所有遊戲系統...");
        
        // 創建並初始化所有管理器
        InitializePlayerProgress();
        InitializeUIManager();
        InitializeTalentManager();
        InitializeDeckManager();
        InitializeComboManager();
        InitializeBattleSystems();
        
        // 連接系統之間的依賴
        ConnectSystems();
        
        // 完成初始化
        OnGameInitialized?.Invoke();
        SetGameState(GameState.Exploring);
        
        Debug.Log("遊戲初始化完成！");
    }
    
    /// <summary>
    /// 初始化玩家進度系統
    /// </summary>
    private void InitializePlayerProgress()
    {
        if (PlayerProgress.Instance == null && playerProgressPrefab != null)
        {
            GameObject ppGO = Instantiate(playerProgressPrefab);
            ppGO.name = "PlayerProgress";
            playerProgress = ppGO.GetComponent<PlayerProgress>();
        }
        else
        {
            playerProgress = PlayerProgress.Instance;
        }
        
        Debug.Log("玩家進度系統初始化完成");
    }
    
    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    private void InitializeUIManager()
    {
        if (UIManager.Instance == null && uiManagerPrefab != null)
        {
            GameObject uiGO = Instantiate(uiManagerPrefab);
            uiGO.name = "UIManager";
            uiManager = uiGO.GetComponent<UIManager>();
        }
        else
        {
            uiManager = UIManager.Instance;
        }
        
        Debug.Log("UI管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化天賦管理器
    /// </summary>
    private void InitializeTalentManager()
    {
        if (TalentManager.Instance == null && talentManagerPrefab != null)
        {
            GameObject tmGO = Instantiate(talentManagerPrefab);
            tmGO.name = "TalentManager";
            talentManager = tmGO.GetComponent<TalentManager>();
        }
        else
        {
            talentManager = TalentManager.Instance;
        }
        
        Debug.Log("天賦管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化卡牌管理器
    /// </summary>
    private void InitializeDeckManager()
    {
        if (DeckManager.Instance == null && deckManagerPrefab != null)
        {
            GameObject dmGO = Instantiate(deckManagerPrefab);
            dmGO.name = "DeckManager";
            deckManager = dmGO.GetComponent<DeckManager>();
        }
        else
        {
            deckManager = DeckManager.Instance;
        }
        
        Debug.Log("卡牌管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化連擊管理器
    /// </summary>
    private void InitializeComboManager()
    {
        if (ComboManager.Instance == null && comboManagerPrefab != null)
        {
            GameObject cmGO = Instantiate(comboManagerPrefab);
            cmGO.name = "ComboManager";
            comboManager = cmGO.GetComponent<ComboManager>();
        }
        else
        {
            comboManager = ComboManager.Instance;
        }
        
        Debug.Log("連擊管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化戰鬥系統
    /// </summary>
    private void InitializeBattleSystems()
    {
        // 初始化戰鬥流程管理器
        if (BattleFlowManager.Instance == null && battleFlowPrefab != null)
        {
            GameObject bfGO = Instantiate(battleFlowPrefab);
            bfGO.name = "BattleFlowManager";
            battleFlow = bfGO.GetComponent<BattleFlowManager>();
        }
        else
        {
            battleFlow = BattleFlowManager.Instance;
        }
        
        // 初始化戰鬥解析器
        battleResolver = battleFlow.GetComponent<BattleResolver>();
        
        Debug.Log("戰鬥系統初始化完成");
    }
    
    /// <summary>
    /// 連接所有系統
    /// </summary>
    private void ConnectSystems()
    {
        // 訂閱戰鬥流程事件
        if (battleFlow != null)
        {
            battleFlow.OnBattleStarted += OnBattleStarted;
            battleFlow.OnBattleEnded += OnBattleEnded;
            battleFlow.OnEnemySpawned += OnEnemySpawned;
        }
        
        // 訂閱玩家進度事件
        if (playerProgress != null)
        {
            playerProgress.OnPlayerStatsUpdated += OnPlayerStatsUpdated;
            playerProgress.OnTalentPointsChanged += OnTalentPointsChanged;
        }
        
        // 訂閱UI事件
        if (uiManager != null)
        {
            uiManager.startBattleButton.onClick.AddListener(StartBattle);
            uiManager.continueButton.onClick.AddListener(ContinueGame);
        }
        
        Debug.Log("系統連接完成");
    }
    
    /// <summary>
    /// 處理全局輸入
    /// </summary>
    private void HandleGlobalInput()
    {
        // ESC鍵暫停/恢復遊戲
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
            else if (currentGameState == GameState.Exploring || currentGameState == GameState.Battling)
            {
                PauseGame();
            }
        }
        
        // 快速存檔/讀檔（測試用）
        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            QuickLoad();
        }
    }
    
    /// <summary>
    /// 設置遊戲狀態
    /// </summary>
    private void SetGameState(GameState newState)
    {
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        Debug.Log($"遊戲狀態變化: {previousState} -> {newState}");
        OnGameStateChanged?.Invoke(newState);
        
        HandleGameStateChange(previousState, newState);
    }
    
    /// <summary>
    /// 處理遊戲狀態變化
    /// </summary>
    private void HandleGameStateChange(GameState previousState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
                
            case GameState.Battling:
                Time.timeScale = 1f;
                break;
                
            case GameState.Exploring:
                Time.timeScale = 1f;
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                ShowGameOverScreen();
                break;
                
            case GameState.Victory:
                Time.timeScale = 0f;
                ShowVictoryScreen();
                break;
        }
    }
    
    /// <summary>
    /// 開始新遊戲
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("開始新遊戲");
        
        // 重置所有系統
        playerProgress.InitializeNewGame();
        talentManager.ResetTalents();
        deckManager.InitializeDeck();
        comboManager.InitializeComboSystem();
        
        SetGameState(GameState.Exploring);
        
        // 更新UI
        uiManager.UpdatePlayerUI(
            playerProgress.playerStats.currentHealth,
            playerProgress.playerStats.maxHealth,
            playerProgress.playerStats.shield,
            playerProgress.playerStats.ToDictionary()
        );
    }
    
    /// <summary>
    /// 開始戰鬥
    /// </summary>
    public void StartBattle()
    {
        if (currentGameState != GameState.Exploring) return;
        
        Debug.Log("開始戰鬥");
        SetGameState(GameState.Battling);
        
        // 通知戰鬥系統開始戰鬥
        battleFlow.StartBattle();
    }
    
    /// <summary>
    /// 繼續遊戲
    /// </summary>
    public void ContinueGame()
    {
        if (currentGameState == GameState.Paused)
        {
            ResumeGame();
        }
        else if (currentGameState == GameState.Exploring)
        {
            // 繼續探索或進入下一個敵人
            battleFlow.ContinueToNextEnemy();
        }
    }
    
    /// <summary>
    /// 暫停遊戲
    /// </summary>
    public void PauseGame()
    {
        if (currentGameState == GameState.Battling || currentGameState == GameState.Exploring)
        {
            SetGameState(GameState.Paused);
            OnGamePaused?.Invoke();
            
            // 顯示暫停菜單
            uiManager.ShowPauseMenu();
        }
    }
    
    /// <summary>
    /// 恢復遊戲
    /// </summary>
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.Exploring);
            OnGameResumed?.Invoke();
            
            // 隱藏暫停菜單
            uiManager.HidePauseMenu();
        }
    }
    
    /// <summary>
    /// 快速存檔
    /// </summary>
    public void QuickSave()
    {
        if (currentGameState == GameState.Exploring)
        {
            playerProgress.SaveGame();
            Debug.Log("快速存檔完成");
        }
    }
    
    /// <summary>
    /// 快速讀檔
    /// </summary>
    public void QuickLoad()
    {
        if (currentGameState != GameState.Battling)
        {
            playerProgress.LoadGame();
            Debug.Log("快速讀檔完成");
            
            // 更新UI
            uiManager.UpdatePlayerUI(
                playerProgress.playerStats.currentHealth,
                playerProgress.playerStats.maxHealth,
                playerProgress.playerStats.shield,
                playerProgress.playerStats.ToDictionary()
            );
        }
    }
    
    /// <summary>
    /// 返回主菜單
    /// </summary>
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.Menu);
        SceneManager.LoadScene(menuSceneName);
        
        // 清理戰鬥相關狀態
        battleFlow.RestartBattleSystem();
        
        Debug.Log("返回主菜單");
    }
    
    /// <summary>
    /// 退出遊戲
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出遊戲");
        
        // 保存遊戲
        playerProgress.SaveGame();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 顯示遊戲結束畫面
    /// </summary>
    private void ShowGameOverScreen()
    {
        uiManager.ShowGameOverScreen();
        Debug.Log("遊戲結束");
    }
    
    /// <summary>
    /// 顯示勝利畫面
    /// </summary>
    private void ShowVictoryScreen()
    {
        uiManager.ShowVictoryScreen();
        Debug.Log("遊戲勝利！");
    }
    
    // ===== 事件處理方法 =====
    
    private void OnBattleStarted()
    {
        Debug.Log("主控制器: 戰鬥開始");
        comboManager.OnCombatStart(playerProgress.currentEnemyID);
    }
    
    private void OnBattleEnded()
    {
        Debug.Log("主控制器: 戰鬥結束");
        comboManager.OnCombatEnd(true); // 假設總是勝利，實際應該根據結果傳參
    }
    
    private void OnEnemySpawned(EntityStats enemy)
    {
        Debug.Log($"主控制器: 敵人生成 - {enemy.entityName}");
    }
    
    private void OnPlayerStatsUpdated()
    {
        Debug.Log("主控制器: 玩家狀態更新");
    }
    
    private void OnTalentPointsChanged()
    {
        Debug.Log("主控制器: 天賦點變化");
    }
    
    /// <summary>
    /// 檢查遊戲完成條件
    /// </summary>
    public void CheckGameCompletion()
    {
        if (playerProgress.currentEnemyID >= 100)
        {
            SetGameState(GameState.Victory);
        }
        else if (playerProgress.deaths > 0 && !PlayerCanContinue())
        {
            SetGameState(GameState.GameOver);
        }
    }
    
    /// <summary>
    /// 檢查玩家是否可以繼續遊戲
    /// </summary>
    private bool PlayerCanContinue()
    {
        // 檢查是否有復活天賦或其他繼續遊戲的方式
        return playerProgress.activatedTalents.Contains("深層執念...肉與骨") && !playerProgress.HasUsedRevive();
    }
    
    /// <summary>
    /// 獲取當前遊戲狀態
    /// </summary>
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
    
    /// <summary>
    /// 檢查遊戲是否正在運行
    /// </summary>
    public bool IsGameRunning()
    {
        return currentGameState == GameState.Exploring || currentGameState == GameState.Battling;
    }
    
    /// <summary>
    /// 獲取系統實例（用於其他腳本訪問）
    /// </summary>
    public T GetSystem<T>() where T : class
    {
        if (typeof(T) == typeof(PlayerProgress)) return playerProgress as T;
        if (typeof(T) == typeof(UIManager)) return uiManager as T;
        if (typeof(T) == typeof(BattleFlowManager)) return battleFlow as T;
        if (typeof(T) == typeof(TalentManager)) return talentManager as T;
        if (typeof(T) == typeof(DeckManager)) return deckManager as T;
        if (typeof(T) == typeof(ComboManager)) return comboManager as T;
        if (typeof(T) == typeof(BattleResolver)) return battleResolver as T;
        
        return null;
    }
    
    private void OnDestroy()
    {
        // 取消訂閱所有事件
        if (battleFlow != null)
        {
            battleFlow.OnBattleStarted -= OnBattleStarted;
            battleFlow.OnBattleEnded -= OnBattleEnded;
            battleFlow.OnEnemySpawned -= OnEnemySpawned;
        }
        
        if (playerProgress != null)
        {
            playerProgress.OnPlayerStatsUpdated -= OnPlayerStatsUpdated;
            playerProgress.OnTalentPointsChanged -= OnTalentPointsChanged;
        }
    }
    
    /// <summary>
    /// 重啟遊戲系統（用於開發測試）
    /// </summary>
    [ContextMenu("重啟遊戲系統")]
    public void RestartGameSystems()
    {
        Debug.Log("重啟所有遊戲系統...");
        InitializeAllSystems();
    }
}