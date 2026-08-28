using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;

namespace GongWon.Characters
{
    /// <summary>
    /// 角色管理器 — 角色切换、头像、属性
    /// 角色：燐无(155)、久流(166)、安英(194)
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [Header("当前角色")]
        public int currentCharacterId = 155; // 默认燐无
        public string currentCharacterName = "燐无";

        [Header("角色头像")]
        public Dictionary<int, Sprite> characterAvatars = new Dictionary<int, Sprite>();

        [Header("角色属性加成")]
        public Dictionary<int, CharacterStats> characterStats = new Dictionary<int, CharacterStats>();

        [System.Serializable]
        public class CharacterStats
        {
            public int hpBonus;       // 血量加成
            public int damageBonus;   // 伤害加成
            public float speedBonus;  // 速度加成
            public string description; // 角色描述
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeCharacters();
        }

        /// <summary>
        /// 初始化角色属性
        /// </summary>
        private void InitializeCharacters()
        {
            // 燐无(155) - 平衡型
            characterStats[155] = new CharacterStats
            {
                hpBonus = 0,
                damageBonus = 0,
                speedBonus = 0f,
                description = "平衡型角色，各项属性均衡，适合新手。"
            };

            // 久流(166) - 高攻型
            characterStats[166] = new CharacterStats
            {
                hpBonus = -20,
                damageBonus = 15,
                speedBonus = 0.5f,
                description = "高攻击高速度，但血量较低，适合进阶玩家。"
            };

            // 安英(194) - 坦克型
            characterStats[194] = new CharacterStats
            {
                hpBonus = 50,
                damageBonus = -5,
                speedBonus = -0.5f,
                description = "高血量防御型，移动较慢，适合团队前排。"
            };

            Debug.Log("[CharacterManager] 角色系统初始化完成: 燐无(155), 久流(166), 安英(194)");
        }

        /// <summary>
        /// 切换角色
        /// </summary>
        public void SwitchCharacter(int characterId)
        {
            if (!GameConfig.CharacterNames.ContainsKey(characterId))
            {
                Debug.LogWarning($"[CharacterManager] 角色不存在: {characterId}");
                return;
            }

            currentCharacterId = characterId;
            currentCharacterName = GameConfig.CharacterNames[characterId];

            // 应用角色属性到玩家
            ApplyCharacterStats();

            Debug.Log($"[CharacterManager] 切换角色: {currentCharacterName}(ID:{characterId})");
        }

        /// <summary>
        /// 应用角色属性加成
        /// </summary>
        private void ApplyCharacterStats()
        {
            if (!characterStats.TryGetValue(currentCharacterId, out CharacterStats stats)) return;

            var player = GameManager.Instance?.playerController;
            if (player != null)
            {
                player.maxHp = GameConfig.PLAYER_DEFAULT_HP + stats.hpBonus;
                player.currentHp = player.maxHp;
                player.currentWeaponDamage += stats.damageBonus;
                player.moveSpeed += stats.speedBonus;
            }
        }

        /// <summary>
        /// 获取所有角色ID
        /// </summary>
        public int[] GetAllCharacterIds()
        {
            return new int[] { 155, 166, 194 };
        }

        /// <summary>
        /// 获取角色名称
        /// </summary>
        public string GetCharacterName(int id)
        {
            return GameConfig.CharacterNames.TryGetValue(id, out string name) ? name : $"未知角色_{id}";
        }

        /// <summary>
        /// 获取角色属性
        /// </summary>
        public CharacterStats GetCharacterStats(int id)
        {
            return characterStats.TryGetValue(id, out CharacterStats stats) ? stats : null;
        }
    }
}
