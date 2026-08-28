using System;
using System.Collections.Generic;
using UnityEngine;
using GongWon.Core;
using GongWon.Monsters;
using GongWon.Maps;

namespace GongWon.Developer
{
    /// <summary>
    /// 开发者工具 — 悬浮窗功能
    /// 功能：绘制怪物、绘制玩家、绘制棺椁、加速、创建新账号
    /// 这是Unity侧的逻辑，悬浮窗UI由Android原生层实现
    /// </summary>
    public class DeveloperTool : MonoBehaviour
    {
        public static DeveloperTool Instance { get; private set; }

        [Header("工具状态")]
        public bool isToolEnabled = false;
        public bool drawMonsters = false;
        public bool drawPlayers = false;
        public bool drawCoffins = false;
        public bool speedHack = false;

        [Header("加速倍率")]
        [Range(1f, 10f)]
        public float speedMultiplier = 2f;
        private float originalTimeScale = 1f;

        [Header("绘制设置")]
        public Color monsterColor = Color.red;
        public Color playerColor = Color.green;
        public Color coffinColor = Color.yellow;
        public float drawDistance = 50f;

        [Header("账号管理")]
        public string newAccountName = "";

        public event Action<bool> OnToolToggled;
        public event Action<bool, string> OnDrawOptionChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 开启/关闭开发者工具
        /// </summary>
        public void ToggleTool(bool enable)
        {
            isToolEnabled = enable;
            OnToolToggled?.Invoke(enable);
            Debug.Log($"[DeveloperTool] 开发者工具{(enable ? "开启" : "关闭")}");

            if (!enable)
            {
                // 关闭时重置所有功能
                drawMonsters = false;
                drawPlayers = false;
                drawCoffins = false;
                SetSpeedHack(false);
            }
        }

        /// <summary>
        /// 切换绘制怪物
        /// </summary>
        public void ToggleDrawMonsters(bool enable)
        {
            drawMonsters = enable;
            OnDrawOptionChanged?.Invoke(enable, "monsters");
            Debug.Log($"[DeveloperTool] 绘制怪物: {enable}");
        }

        /// <summary>
        /// 切换绘制玩家
        /// </summary>
        public void ToggleDrawPlayers(bool enable)
        {
            drawPlayers = enable;
            OnDrawOptionChanged?.Invoke(enable, "players");
            Debug.Log($"[DeveloperTool] 绘制玩家: {enable}");
        }

        /// <summary>
        /// 切换绘制棺椁
        /// </summary>
        public void ToggleDrawCoffins(bool enable)
        {
            drawCoffins = enable;
            OnDrawOptionChanged?.Invoke(enable, "coffins");
            Debug.Log($"[DeveloperTool] 绘制棺椁: {enable}");
        }

        /// <summary>
        /// 设置加速
        /// </summary>
        public void SetSpeedHack(bool enable)
        {
            if (enable && !speedHack)
            {
                originalTimeScale = Time.timeScale;
                Time.timeScale = speedMultiplier;
                speedHack = true;
                Debug.Log($"[DeveloperTool] 加速开启，倍率: {speedMultiplier}x");
            }
            else if (!enable && speedHack)
            {
                Time.timeScale = originalTimeScale;
                speedHack = false;
                Debug.Log("[DeveloperTool] 加速关闭");
            }
        }

        /// <summary>
        /// 设置加速倍率
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Clamp(multiplier, 1f, 10f);
            if (speedHack) Time.timeScale = speedMultiplier;
            Debug.Log($"[DeveloperTool] 加速倍率设置为: {speedMultiplier}x");
        }

        /// <summary>
        /// 创建新账号
        /// </summary>
        public bool CreateNewAccount(string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                Debug.LogWarning("[DeveloperTool] 账号名称不能为空");
                return false;
            }

            // 在本地存储创建账号文件夹
            string playerDir = System.IO.Path.Combine(Application.persistentDataPath, "players", accountName);
            if (System.IO.Directory.Exists(playerDir))
            {
                Debug.LogWarning($"[DeveloperTool] 账号已存在: {accountName}");
                return false;
            }

            System.IO.Directory.CreateDirectory(playerDir);
            string credential = System.IO.Path.Combine(playerDir, "credential.dat");
            System.IO.File.WriteAllText(credential, $"{accountName}|{DateTime.Now:yyyyMMddHHmmss}|{GameConfig.APP_VERSION}|dev");

            // 创建资源文件夹
            string resourceDir = System.IO.Path.Combine(playerDir, "resources");
            System.IO.Directory.CreateDirectory(resourceDir);

            Debug.Log($"[DeveloperTool] 新账号创建成功: {accountName}, 路径: {playerDir}");
            return true;
        }

        /// <summary>
        /// 获取所有本地账号
        /// </summary>
        public string[] GetAllAccounts()
        {
            string playersDir = System.IO.Path.Combine(Application.persistentDataPath, "players");
            if (!System.IO.Directory.Exists(playersDir)) return new string[0];
            return System.IO.Directory.GetDirectories(playersDir);
        }

        /// <summary>
        /// 绘制功能 — 在OnGUI中绘制方框和连线
        /// </summary>
        private void OnGUI()
        {
            if (!isToolEnabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // 绘制怪物
            if (drawMonsters)
            {
                foreach (Monster monster in FindObjectsOfType<Monster>())
                {
                    if (monster == null || monster.currentState == Monster.MonsterState.Dead) continue;
                    DrawESPBox(cam, monster.transform, monsterColor, $"{monster.monsterName}\nHP:{monster.currentHp}/{monster.maxHp}");
                }
            }

            // 绘制玩家
            if (drawPlayers)
            {
                foreach (GongWon.Player.PlayerController player in FindObjectsOfType<GongWon.Player.PlayerController>())
                {
                    if (player == null || player.isDead) continue;
                    DrawESPBox(cam, player.transform, playerColor, $"Player\nHP:{player.currentHp}/{player.maxHp}");
                }
            }

            // 绘制棺椁
            if (drawCoffins && CoffinManager.Instance != null)
            {
                foreach (Coffin coffin in FindObjectsOfType<Coffin>())
                {
                    if (coffin == null) continue;
                    Color c = coffin.isGold ? new Color(1f, 0.84f, 0f) : coffinColor;
                    DrawESPBox(cam, coffin.transform, c, $"{(coffin.isGold ? "金棺" : "普通棺")}\n{(coffin.isOpened ? "已开启" : "未开启")}");
                }
            }
        }

        /// <summary>
        /// 绘制ESP方框和标签
        /// </summary>
        private void DrawESPBox(Camera cam, Transform target, Color color, string label)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            if (screenPos.z < 0) return; // 在相机背后

            float x = Screen.width - screenPos.x; // Unity GUI坐标翻转
            float y = Screen.height - screenPos.y;

            // 估算目标高度（基于距离）
            float distance = Vector3.Distance(cam.transform.position, target.position);
            if (distance > drawDistance) return;
            float boxHeight = Mathf.Clamp(200f / distance * 10f, 20f, 150f);
            float boxWidth = boxHeight * 0.5f;

            // 绘制方框
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();

            // 边框
            GUI.DrawTexture(new Rect(x - boxWidth / 2, y - boxHeight, boxWidth, 2), tex);
            GUI.DrawTexture(new Rect(x - boxWidth / 2, y, boxWidth, 2), tex);
            GUI.DrawTexture(new Rect(x - boxWidth / 2, y - boxHeight, 2, boxHeight), tex);
            GUI.DrawTexture(new Rect(x + boxWidth / 2 - 2, y - boxHeight, 2, boxHeight), tex);

            // 标签
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 12;
            style.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(x - 50, y - boxHeight - 30, 100, 30), label, style);

            Destroy(tex);
        }

        private void OnDestroy()
        {
            // 确保关闭加速
            if (speedHack) Time.timeScale = originalTimeScale;
        }
    }
}
