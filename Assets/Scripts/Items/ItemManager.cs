using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;

namespace GongWon.Items
{
    /// <summary>
    /// 道具管理器 — 商城、背包、道具使用
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        [Header("玩家背包")]
        public Dictionary<int, int> inventory = new Dictionary<int, int>(); // itemId -> count

        [Header("商城")]
        public Dictionary<int, int> shopPrices = new Dictionary<int, int>(); // itemId -> price

        [Header("玩家金币")]
        public int playerCoins = 1000;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeShop();
        }

        /// <summary>
        /// 初始化商城价格
        /// </summary>
        private void InitializeShop()
        {
            shopPrices[3764649] = 50;    // 汉堡
            shopPrices[469998] = 200;    // 小刀
            shopPrices[7674] = 150;      // 防身符咒
            shopPrices[124545] = 300;    // 小人
            shopPrices[54679] = 250;     // 纸扎人
            shopPrices[17] = 800;        // 猎枪
            shopPrices[15] = 500;        // 左轮短枪
            Debug.Log("[ItemManager] 商城初始化完成");
        }

        /// <summary>
        /// 购买道具
        /// </summary>
        public bool BuyItem(int itemId)
        {
            if (!shopPrices.TryGetValue(itemId, out int price))
            {
                Debug.LogWarning($"[ItemManager] 商品不存在: {itemId}");
                return false;
            }

            if (playerCoins < price)
            {
                Debug.LogWarning($"[ItemManager] 金币不足，需要{price}，当前{playerCoins}");
                return false;
            }

            playerCoins -= price;
            AddItem(itemId, 1);
            Debug.Log($"[ItemManager] 购买成功: {GameConfig.ItemNames[itemId]}, 花费{price}金币");
            return true;
        }

        /// <summary>
        /// 添加道具到背包
        /// </summary>
        public void AddItem(int itemId, int count = 1)
        {
            if (inventory.ContainsKey(itemId))
                inventory[itemId] += count;
            else
                inventory[itemId] = count;
            Debug.Log($"[ItemManager] 获得道具: {GameConfig.ItemNames[itemId]} x{count}");
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        public bool UseItem(int itemId)
        {
            if (!inventory.ContainsKey(itemId) || inventory[itemId] <= 0)
            {
                Debug.LogWarning($"[ItemManager] 道具不足: {itemId}");
                return false;
            }

            inventory[itemId]--;
            ApplyItemEffect(itemId);
            Debug.Log($"[ItemManager] 使用道具: {GameConfig.ItemNames[itemId]}");
            return true;
        }

        /// <summary>
        /// 应用道具效果
        /// </summary>
        private void ApplyItemEffect(int itemId)
        {
            var player = GongWon.Core.GameManager.Instance?.playerController;
            switch (itemId)
            {
                case 3764649: // 汉堡 - 恢复血量
                    player?.Heal(30);
                    break;
                case 469998: // 小刀 - 装备武器
                    player?.EquipWeapon(itemId);
                    break;
                case 7674: // 防身符咒 - 临时无敌
                    if (player != null)
                    {
                        player.isInvincible = true;
                        player.Invoke(nameof(player.ResetInvincible), 10f);
                    }
                    break;
                case 17: // 猎枪
                case 15: // 左轮短枪
                    player?.EquipWeapon(itemId);
                    break;
                case 124545: // 小人 - 召唤替身吸引怪物
                    // TODO: 召唤替身逻辑
                    break;
                case 54679: // 纸扎人 - 范围攻击
                    // TODO: 纸扎人爆炸逻辑
                    break;
            }
        }

        /// <summary>
        /// 获取所有商城道具
        /// </summary>
        public int[] GetShopItems()
        {
            int[] items = new int[shopPrices.Count];
            shopPrices.Keys.CopyTo(items, 0);
            return items;
        }

        /// <summary>
        /// 获取道具名称
        /// </summary>
        public string GetItemName(int itemId)
        {
            return GameConfig.ItemNames.TryGetValue(itemId, out string name) ? name : $"未知道具_{itemId}";
        }

        /// <summary>
        /// 获取道具价格
        /// </summary>
        public int GetItemPrice(int itemId)
        {
            return shopPrices.TryGetValue(itemId, out int price) ? price : 0;
        }
    }
}
