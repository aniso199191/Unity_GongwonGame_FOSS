using System;
using System.Collections;
using UnityEngine;
using GongWon.Net;
using GongWon.Monsters;
using GongWon.Player;
using GongWon.Items;
using GongWon.Characters;
using GongWon.Maps;
using GongWon.Multiplayer;
using GongWon.UI;

namespace GongWon.Core
{
    /// <summary>
    /// 游戏主管理器 — 单例，管理所有子系统生命周期
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("系统引用")]
        public MonsterManager monsterManager;
        public PlayerController playerController;
        public ItemManager itemManager;
        public CharacterManager characterManager;
        public MapManager mapManager;
        public NetworkManager networkManager;
        public UIManager uiManager;

        [Header("游戏状态")]
        public GameState currentState = GameState.Boot;
        public string currentPlayerName = "";
        public int currentCharacterId = 155; // 默认燐无

        public enum GameState
        {
            Boot,          // 启动动画
            Login,         // 输入名称登录
            MainMenu,      // 主菜单
            MapSelect,     // 地图选择
            Loading,       // 加载中
            InGame,        // 游戏中
            Paused,        // 暂停
            GameOver       // 结算
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }

        private void Start()
        {
            StartCoroutine(BootSequence());
        }

        /// <summary>
        /// 初始化所有子系统
        /// </summary>
        private void InitializeSystems()
        {
            // 确保各管理器存在
            if (monsterManager == null) monsterManager = gameObject.AddComponent<MonsterManager>();
            if (itemManager == null) itemManager = gameObject.AddComponent<ItemManager>();
            if (characterManager == null) characterManager = gameObject.AddComponent<CharacterManager>();
            if (mapManager == null) mapManager = gameObject.AddComponent<MapManager>();
            if (networkManager == null) networkManager = gameObject.AddComponent<NetworkManager>();
            if (uiManager == null) uiManager = gameObject.AddComponent<UIManager>();

            Debug.Log("[GameManager] 所有系统初始化完成");
        }

        /// <summary>
        /// 启动序列：开场动画 -> 检查更新 -> 检查公告 -> 登录
        /// </summary>
        private IEnumerator BootSequence()
        {
            currentState = GameState.Boot;
            Debug.Log("[GameManager] 启动开场动画: 공원 血字");

            // 播放开场动画（공원血字）
            yield return PlayIntroAnimation();

            // 检查更新（第二QQ链接）
            yield return UpdateSystem.Instance.CheckUpdate();

            // 检查公告（第一QQ链接）
            yield return AnnouncementSystem.Instance.CheckAnnouncement();

            // 进入登录界面
            currentState = GameState.Login;
            uiManager.ShowLoginPanel();
            Debug.Log("[GameManager] 启动完成，进入登录界面");
        }

        /// <summary>
        /// 开场动画：공원 血字带血液流淌效果
        /// </summary>
        private IEnumerator PlayIntroAnimation()
        {
            // 由UI层实际播放动画，这里等待动画完成
            uiManager?.PlayIntroBloodText();
            yield return new WaitForSeconds(3.5f);
        }

        /// <summary>
        /// 玩家登录 — 创建本地账号文件夹
        /// </summary>
        public void Login(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("[GameManager] 玩家名称不能为空");
                return;
            }

            currentPlayerName = playerName;

            // 在SD卡/本地存储创建玩家文件夹作为凭证
            string playerDir = System.IO.Path.Combine(Application.persistentDataPath, "players", playerName);
            if (!System.IO.Directory.Exists(playerDir))
            {
                System.IO.Directory.CreateDirectory(playerDir);
                // 写入凭证文件
                string credential = System.IO.Path.Combine(playerDir, "credential.dat");
                System.IO.File.WriteAllText(credential, $"{playerName}|{DateTime.Now:yyyyMMddHHmmss}|{GameConfig.APP_VERSION}");
                Debug.Log($"[GameManager] 新账号创建: {playerName}, 凭证路径: {credential}");
            }
            else
            {
                Debug.Log($"[GameManager] 已有账号登录: {playerName}");
            }

            currentState = GameState.MainMenu;
            uiManager.ShowMainMenu();
        }

        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] 状态切换: {newState}");
        }

        public event Action<GameState> OnStateChanged;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
