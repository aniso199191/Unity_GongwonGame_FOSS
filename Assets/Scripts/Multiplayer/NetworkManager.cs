using System;
using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;

namespace GongWon.Multiplayer
{
    /// <summary>
    /// 多人联机管理器 — 使用Unity Netcode for GameObjects
    /// 4队x4人，自动匹配，组队模式
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("联机状态")]
        public bool isConnected = false;
        public bool isHost = false;
        public string playerId;
        public int teamId = -1;
        public int playerSlot = -1;

        [Header("队伍配置")]
        public int teamCount = GameConfig.TEAM_COUNT;        // 4队
        public int playersPerTeam = GameConfig.PLAYERS_PER_TEAM; // 每队4人

        [Header("玩家列表")]
        public Dictionary<string, NetworkPlayer> allPlayers = new Dictionary<string, NetworkPlayer>();
        public Dictionary<int, List<NetworkPlayer>> teams = new Dictionary<int, List<NetworkPlayer>>();

        [Header("匹配")]
        public bool isMatching = false;
        public float matchTimer = 0f;
        public const float MATCH_TIMEOUT = 30f;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public event Action<bool> OnMatchResult;

        [System.Serializable]
        public class NetworkPlayer
        {
            public string playerId;
            public string playerName;
            public int characterId;
            public int teamId;
            public int playerSlot;
            public bool isReady;
            public Vector3 position;
            public int currentHp;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeTeams();
        }

        /// <summary>
        /// 初始化队伍
        /// </summary>
        private void InitializeTeams()
        {
            for (int i = 0; i < teamCount; i++)
            {
                teams[i] = new List<NetworkPlayer>();
            }
            Debug.Log($"[NetworkManager] 队伍初始化: {teamCount}队 x {playersPerTeam}人");
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect()
        {
            // TODO: 实际项目中使用Unity Netcode / Mirror / Photon等
            // 这里是框架代码
            playerId = Guid.NewGuid().ToString("N").Substring(0, 8);
            isConnected = true;
            Debug.Log($"[NetworkManager] 已连接, 玩家ID: {playerId}");
            OnConnected?.Invoke();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
            isHost = false;
            allPlayers.Clear();
            foreach (var team in teams) team.Value.Clear();
            Debug.Log("[NetworkManager] 已断开连接");
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// 创建房间（作为主机）
        /// </summary>
        public void CreateRoom(string roomName)
        {
            if (!isConnected) Connect();
            isHost = true;
            Debug.Log($"[NetworkManager] 创建房间: {roomName}");
            JoinTeam(0); // 默认加入1队
        }

        /// <summary>
        /// 加入队伍
        /// </summary>
        public void JoinTeam(int teamId)
        {
            if (teamId < 0 || teamId >= teamCount)
            {
                Debug.LogWarning($"[NetworkManager] 无效队伍ID: {teamId}");
                return;
            }

            if (teams[teamId].Count >= playersPerTeam)
            {
                Debug.LogWarning($"[NetworkManager] 队伍{teamId}已满");
                return;
            }

            // 离开当前队伍
            if (this.teamId >= 0)
            {
                teams[this.teamId].RemoveAll(p => p.playerId == playerId);
            }

            this.teamId = teamId;
            playerSlot = teams[teamId].Count;

            NetworkPlayer player = new NetworkPlayer
            {
                playerId = this.playerId,
                playerName = GameManager.Instance?.currentPlayerName ?? "Player",
                characterId = GameManager.Instance?.currentCharacterId ?? 155,
                teamId = teamId,
                playerSlot = playerSlot,
                isReady = false
            };

            allPlayers[playerId] = player;
            teams[teamId].Add(player);

            Debug.Log($"[NetworkManager] 加入队伍{teamId}, 位置{playerSlot}");
        }

        /// <summary>
        /// 自动匹配 — 自动分配到人数最少的队伍
        /// </summary>
        public void AutoMatch()
        {
            if (!isConnected) Connect();
            isMatching = true;
            matchTimer = 0f;
            Debug.Log("[NetworkManager] 开始自动匹配...");

            // 找到人数最少的队伍
            int minTeam = 0;
            int minCount = int.MaxValue;
            for (int i = 0; i < teamCount; i++)
            {
                if (teams[i].Count < minCount && teams[i].Count < playersPerTeam)
                {
                    minCount = teams[i].Count;
                    minTeam = i;
                }
            }

            JoinTeam(minTeam);
            isMatching = false;
            OnMatchResult?.Invoke(true);
            Debug.Log($"[NetworkManager] 匹配成功，分配到队伍{minTeam}");
        }

        /// <summary>
        /// 玩家准备
        /// </summary>
        public void SetReady(bool ready)
        {
            if (allPlayers.TryGetValue(playerId, out NetworkPlayer player))
            {
                player.isReady = ready;
                Debug.Log($"[NetworkManager] 玩家{(ready ? "准备" : "取消准备")}");
                CheckAllReady();
            }
        }

        /// <summary>
        /// 检查所有玩家是否准备好
        /// </summary>
        private void CheckAllReady()
        {
            int totalPlayers = 0;
            int readyPlayers = 0;
            foreach (var team in teams)
            {
                foreach (var p in team.Value)
                {
                    totalPlayers++;
                    if (p.isReady) readyPlayers++;
                }
            }

            if (totalPlayers > 0 && readyPlayers == totalPlayers && isHost)
            {
                Debug.Log("[NetworkManager] 所有玩家已准备，开始游戏！");
                // 通知所有客户端开始游戏
            }
        }

        /// <summary>
        /// 获取队伍信息
        /// </summary>
        public List<NetworkPlayer> GetTeamPlayers(int teamId)
        {
            return teams.TryGetValue(teamId, out List<NetworkPlayer> team) ? team : new List<NetworkPlayer>();
        }

        /// <summary>
        /// 获取当前队伍总人数
        /// </summary>
        public int GetTotalPlayerCount()
        {
            int count = 0;
            foreach (var team in teams) count += team.Value.Count;
            return count;
        }

        private void Update()
        {
            if (isMatching)
            {
                matchTimer += Time.deltaTime;
                if (matchTimer >= MATCH_TIMEOUT)
                {
                    isMatching = false;
                    OnMatchResult?.Invoke(false);
                    Debug.LogWarning("[NetworkManager] 匹配超时");
                }
            }
        }
    }
}
