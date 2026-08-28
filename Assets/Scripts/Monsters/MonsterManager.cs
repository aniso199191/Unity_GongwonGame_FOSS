using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;

namespace GongWon.Monsters
{
    /// <summary>
    /// 怪物管理器 — 负责生成、管理所有怪物实例
    /// </summary>
    public class MonsterManager : MonoBehaviour
    {
        public static MonsterManager Instance { get; private set; }

        [Header("怪物预制体")]
        public GameObject monsterPrefab; // 通用怪物预制体，通过ID切换外观

        [Header("生成点")]
        public Transform[] spawnPoints;

        [Header("当前活跃怪物")]
        public List<Monster> activeMonsters = new List<Monster>();

        [Header("图鉴")]
        public Dictionary<int, Sprite> monsterGallery = new Dictionary<int, Sprite>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// 在指定位置生成怪物
        /// </summary>
        public Monster SpawnMonster(int monsterId, Vector3 position, bool nerfed = false)
        {
            if (monsterPrefab == null)
            {
                Debug.LogError("[MonsterManager] 怪物预制体未设置！");
                return null;
            }

            GameObject obj = Instantiate(monsterPrefab, position, Quaternion.identity);
            Monster monster = obj.GetComponent<Monster>();
            if (monster == null) monster = obj.AddComponent<Monster>();

            monster.Initialize(monsterId, nerfed);
            activeMonsters.Add(monster);

            Debug.Log($"[MonsterManager] 生成怪物: {monster.monsterName}(ID:{monsterId}) at {position}");
            return monster;
        }

        /// <summary>
        /// 批量生成怪物（用于地图加载）
        /// </summary>
        public void SpawnMonsterWave(int[] monsterIds, bool nerfed = false)
        {
            for (int i = 0; i < monsterIds.Length; i++)
            {
                Vector3 pos = spawnPoints != null && i < spawnPoints.Length
                    ? spawnPoints[i].position
                    : new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                SpawnMonster(monsterIds[i], pos, nerfed);
            }
        }

        /// <summary>
        /// 新手教程生成（全人机+削弱怪物）
        /// </summary>
        public void SpawnTutorialMonsters()
        {
            int[] tutorialIds = { 30024, 32111, 576 }; // 基础怪物
            SpawnMonsterWave(tutorialIds, nerfed: true);
            Debug.Log("[MonsterManager] 新手教程怪物已生成（全部削弱）");
        }

        /// <summary>
        /// 获取所有怪物ID列表（用于图鉴）
        /// </summary>
        public int[] GetAllMonsterIds()
        {
            int[] ids = new int[GameConfig.MonsterNames.Count];
            GameConfig.MonsterNames.Keys.CopyTo(ids, 0);
            return ids;
        }

        /// <summary>
        /// 清除所有活跃怪物
        /// </summary>
        public void ClearAllMonsters()
        {
            foreach (Monster m in activeMonsters)
            {
                if (m != null) Destroy(m.gameObject);
            }
            activeMonsters.Clear();
            Debug.Log("[MonsterManager] 所有怪物已清除");
        }

        /// <summary>
        /// 获取当前存活怪物数量
        /// </summary>
        public int GetAliveMonsterCount()
        {
            activeMonsters.RemoveAll(m => m == null);
            return activeMonsters.Count;
        }
    }
}
