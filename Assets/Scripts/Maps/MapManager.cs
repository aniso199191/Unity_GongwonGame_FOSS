using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GongWon.Core;

namespace GongWon.Maps
{
    /// <summary>
    /// 地图管理器 — 地图选择、加载、规则管理
    /// 地图：鬼灵之谷、江河地、江河地四队
    /// 模式：PVP、PVPVE、摸金模式、多队模式
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("当前地图")]
        public string currentMapName;
        public GameMode currentMode = GameMode.PVE;

        [Header("地图配置")]
        public Dictionary<string, MapConfig> mapConfigs = new Dictionary<string, MapConfig>();

        public enum GameMode
        {
            PVE,        // 纯PVE打怪
            PVP,        // 玩家对战
            PVPVE,      // 玩家+怪物混战
            MoJin,      // 摸金模式（开棺椁）
            MultiTeam   // 多队模式（4队x4人）
        }

        [System.Serializable]
        public class MapConfig
        {
            public string mapName;
            public string sceneName;
            public int maxPlayers;
            public int teamCount;
            public int playersPerTeam;
            public int[] monsterIds;
            public bool hasCoffins;
            public string description;
            public Sprite mapThumbnail;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeMaps();
        }

        /// <summary>
        /// 初始化所有地图配置
        /// </summary>
        private void InitializeMaps()
        {
            // 鬼灵之谷
            mapConfigs["鬼灵之谷"] = new MapConfig
            {
                mapName = "鬼灵之谷",
                sceneName = "Map_GhostValley",
                maxPlayers = 4,
                teamCount = 1,
                playersPerTeam = 4,
                monsterIds = new int[] { 30024, 32111, 45273, 15766 },
                hasCoffins = true,
                description = "阴森山谷，怨灵出没。击败所有怪物后，中央将出现5个棺椁。"
            };

            // 江河地
            mapConfigs["江河地"] = new MapConfig
            {
                mapName = "江河地",
                sceneName = "Map_RiverLand",
                maxPlayers = 4,
                teamCount = 1,
                playersPerTeam = 4,
                monsterIds = new int[] { 5764649, 578864, 576, 46945 },
                hasCoffins = true,
                description = "古老河畔，阴气极重。强大的怪物在此游荡。"
            };

            // 江河地四队
            mapConfigs["江河地四队"] = new MapConfig
            {
                mapName = "江河地四队",
                sceneName = "Map_RiverLand_4Team",
                maxPlayers = 16,
                teamCount = 4,
                playersPerTeam = 4,
                monsterIds = new int[] { 1, 24675, 454679, 5764649, 46945 },
                hasCoffins = true,
                description = "4队混战模式，每队4人。PVPVE，击败对手和怪物，争夺棺椁资源。"
            };

            Debug.Log("[MapManager] 地图初始化完成: 鬼灵之谷, 江河地, 江河地四队");
        }

        /// <summary>
        /// 选择地图并进入
        /// </summary>
        public void SelectMap(string mapName, GameMode mode)
        {
            if (!mapConfigs.ContainsKey(mapName))
            {
                Debug.LogError($"[MapManager] 地图不存在: {mapName}");
                return;
            }

            currentMapName = mapName;
            currentMode = mode;
            Debug.Log($"[MapManager] 选择地图: {mapName}, 模式: {mode}");

            // 加载地图场景
            LoadMapScene(mapName);
        }

        /// <summary>
        /// 加载地图场景
        /// </summary>
        public void LoadMapScene(string mapName)
        {
            if (mapConfigs.TryGetValue(mapName, out MapConfig config))
            {
                GameManager.Instance?.ChangeState(GameManager.GameState.Loading);
                SceneManager.LoadScene(config.sceneName);
                Debug.Log($"[MapManager] 加载场景: {config.sceneName}");
            }
        }

        /// <summary>
        /// 地图加载完成后初始化
        /// </summary>
        public void OnMapLoaded()
        {
            if (!mapConfigs.TryGetValue(currentMapName, out MapConfig config)) return;

            GameManager.Instance?.ChangeState(GameManager.GameState.InGame);

            // 生成怪物
            var monsterManager = GongWon.Monsters.MonsterManager.Instance;
            if (monsterManager != null)
            {
                bool isTutorial = GameManager.Instance.currentState == GameManager.GameState.Boot;
                monsterManager.SpawnMonsterWave(config.monsterIds, isTutorial);
            }

            // 如果有棺椁，生成棺椁
            if (config.hasCoffins)
            {
                var coffinManager = FindObjectOfType<GongWon.Maps.CoffinManager>();
                if (coffinManager != null)
                {
                    coffinManager.SpawnCoffins();
                }
            }

            Debug.Log($"[MapManager] 地图 {currentMapName} 初始化完成");
        }

        /// <summary>
        /// 获取所有可用地图
        /// </summary>
        public string[] GetAllMaps()
        {
            string[] maps = new string[mapConfigs.Count];
            mapConfigs.Keys.CopyTo(maps, 0);
            return maps;
        }

        /// <summary>
        /// 获取地图配置
        /// </summary>
        public MapConfig GetMapConfig(string mapName)
        {
            return mapConfigs.TryGetValue(mapName, out MapConfig config) ? config : null;
        }

        /// <summary>
        /// 检查是否所有怪物已清除（触发棺椁出现）
        /// </summary>
        public void CheckAllMonstersCleared()
        {
            var monsterManager = GongWon.Monsters.MonsterManager.Instance;
            if (monsterManager != null && monsterManager.GetAliveMonsterCount() == 0)
            {
                Debug.Log("[MapManager] 所有怪物已清除！棺椁出现");
                var coffinManager = FindObjectOfType<CoffinManager>();
                coffinManager?.RevealCoffins();
            }
        }
    }
}
