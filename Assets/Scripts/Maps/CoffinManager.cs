using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;

namespace GongWon.Maps
{
    /// <summary>
    /// 棺椁管理器 — 5个棺椁，金棺/普通棺，开棺特效和奖励
    /// 金棺：回收物2万以上，共50个，开时无灰尘
    /// 普通棺：灰色，低于2万，喷出灰色烟雾
    /// </summary>
    public class CoffinManager : MonoBehaviour
    {
        public static CoffinManager Instance { get; private set; }

        [Header("棺椁预制体")]
        public GameObject normalCoffinPrefab;  // 普通灰色棺椁
        public GameObject goldCoffinPrefab;    // 金棺

        [Header("特效")]
        public GameObject graySmokeEffect;      // 灰色烟雾（普通棺）
        public GameObject goldLightEffect;      // 金色光芒（金棺）

        [Header("生成位置")]
        public Transform[] coffinSpawnPoints;   // 5个生成点

        [Header("当前棺椁")]
        public List<Coffin> activeCoffins = new List<Coffin>();
        public bool coffinsRevealed = false;

        [Header("回收物")]
        public int totalRecoveryItems = 0;
        public int goldRecoveryCount = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// 生成5个棺椁（初始隐藏，怪物清除后显现）
        /// </summary>
        public void SpawnCoffins()
        {
            activeCoffins.Clear();
            coffinsRevealed = false;

            int coffinCount = GameConfig.COFFIN_COUNT_PER_MAP; // 5个
            for (int i = 0; i < coffinCount; i++)
            {
                Vector3 pos = coffinSpawnPoints != null && i < coffinSpawnPoints.Length
                    ? coffinSpawnPoints[i].position
                    : new Vector3((i - 2) * 3f, 0, 0);

                // 随机决定是否为金棺（概率20%）
                bool isGold = Random.value < 0.2f;
                GameObject prefab = isGold ? goldCoffinPrefab : normalCoffinPrefab;

                if (prefab == null)
                {
                    Debug.LogWarning("[CoffinManager] 棺椁预制体未设置，使用空物体代替");
                    GameObject emptyCoffin = new GameObject($"Coffin_{i}");
                    emptyCoffin.transform.position = pos;
                    Coffin coffin = emptyCoffin.AddComponent<Coffin>();
                    coffin.Initialize(i, isGold);
                    activeCoffins.Add(coffin);
                    emptyCoffin.SetActive(false); // 初始隐藏
                }
                else
                {
                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
                    Coffin coffin = obj.GetComponent<Coffin>();
                    if (coffin == null) coffin = obj.AddComponent<Coffin>();
                    coffin.Initialize(i, isGold);
                    activeCoffins.Add(coffin);
                    obj.SetActive(false); // 初始隐藏
                }
            }

            Debug.Log($"[CoffinManager] 生成 {coffinCount} 个棺椁（隐藏中）");
        }

        /// <summary>
        /// 怪物清除后显现棺椁
        /// </summary>
        public void RevealCoffins()
        {
            coffinsRevealed = true;
            foreach (Coffin c in activeCoffins)
            {
                if (c != null) c.gameObject.SetActive(true);
            }
            Debug.Log("[CoffinManager] 棺椁已显现！");
        }

        /// <summary>
        /// 开启棺椁
        /// </summary>
        public void OpenCoffin(Coffin coffin)
        {
            if (coffin == null || coffin.isOpened) return;

            coffin.isOpened = true;
            totalRecoveryItems++;

            if (coffin.isGold)
            {
                // 金棺：无灰尘，金色光芒，回收物2万以上
                goldRecoveryCount++;
                if (goldLightEffect != null)
                    Instantiate(goldLightEffect, coffin.transform.position, Quaternion.identity);

                int value = Random.Range(20000, 100000);
                Debug.Log($"[CoffinManager] 金棺开启！回收物价值: {value}（共{goldRecoveryCount}/{GameConfig.COFFIN_TOTAL_RECOVERY}）");
            }
            else
            {
                // 普通棺：灰色烟雾，低于2万
                if (graySmokeEffect != null)
                    Instantiate(graySmokeEffect, coffin.transform.position, Quaternion.identity);

                int value = Random.Range(1000, 19999);
                // 随机出金色（普通棺也有小概率出金）
                if (Random.value < 0.1f)
                {
                    value = Random.Range(20000, 50000);
                    Debug.Log($"[CoffinManager] 普通棺出金色！回收物价值: {value}");
                }
                else
                {
                    Debug.Log($"[CoffinManager] 普通棺开启，回收物价值: {value}");
                }
            }

            // 检查是否所有棺椁都开了
            CheckAllCoffinsOpened();
        }

        private void CheckAllCoffinsOpened()
        {
            int openedCount = 0;
            foreach (Coffin c in activeCoffins)
            {
                if (c != null && c.isOpened) openedCount++;
            }

            if (openedCount >= activeCoffins.Count)
            {
                Debug.Log($"[CoffinManager] 所有棺椁已开启！总回收物: {totalRecoveryItems}, 金棺数: {goldRecoveryCount}");
            }
        }
    }

    /// <summary>
    /// 单个棺椁
    /// </summary>
    public class Coffin : MonoBehaviour
    {
        public int coffinId;
        public bool isGold;
        public bool isOpened;

        public void Initialize(int id, bool gold)
        {
            coffinId = id;
            isGold = gold;
            isOpened = false;
            gameObject.name = $"Coffin_{id}_{(gold ? "Gold" : "Normal")}";
        }

        private void OnMouseDown()
        {
            if (!isOpened)
            {
                CoffinManager.Instance?.OpenCoffin(this);
            }
        }
    }
}
