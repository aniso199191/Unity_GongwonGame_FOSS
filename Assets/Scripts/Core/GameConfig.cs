using System.Collections.Generic;
using UnityEngine;

namespace GongWon.Core
{
    /// <summary>
    /// 游戏全局配置 — 所有ID定义、怪物/道具/角色数据表
    /// </summary>
    public static class GameConfig
    {
        // ==================== 玩家属性ID ====================
        public const int PLAYER_HP_ID = 145666;          // 玩家血量ID
        public const int PLAYER_DEFAULT_HP = 100;         // 默认血量

        // ==================== 怪物ID定义 ====================
        public static readonly Dictionary<int, string> MonsterNames = new Dictionary<int, string>
        {
            { 30024, "공악" },
            { 32111, "두려워하다" },
            { 45273, "사영" },
            { 15766, "저항 영" },
            { 5764649, "죽을 무서워하는 녀" },
            { 578864, "장무성" },
            { 576, "오금찬" },
            { 46945, "넌 제일 무서워하는 것" },
            { 1, "너" },
            { 24675, "가족" },
            { 454679, "사람의 가장 음침한 면" }
        };

        // ==================== 道具ID定义（商城） ====================
        public static readonly Dictionary<int, string> ItemNames = new Dictionary<int, string>
        {
            { 3764649, "汉堡" },
            { 469998, "小刀" },
            { 7674, "防身符咒" },
            { 124545, "小人" },
            { 54679, "纸扎人" },
            { 17, "猎枪" },
            { 15, "左轮短枪" }
        };

        // 道具基础伤害（用于"너"怪物反伤机制）
        public static readonly Dictionary<int, int> ItemBaseDamage = new Dictionary<int, int>
        {
            { 469998, 50 },   // 小刀伤害50
            { 17, 120 },      // 猎枪
            { 15, 80 },       // 左轮短枪
            { 7674, 30 },     // 防身符咒
            { 54679, 60 }     // 纸扎人
        };

        // ==================== 角色ID定义 ====================
        public static readonly Dictionary<int, string> CharacterNames = new Dictionary<int, string>
        {
            { 155, "燐无" },
            { 166, "久流" },
            { 194, "安英" }
        };

        // ==================== 地图定义 ====================
        public static readonly string[] MapNames = { "鬼灵之谷", "江河地", "江河地四队" };

        // ==================== 棺椁配置 ====================
        public const int COFFIN_TOTAL_RECOVERY = 50;      // 金棺回收物总数
        public const int COFFIN_GOLD_VALUE_THRESHOLD = 20000; // 金色回收物价值阈值
        public const int COFFIN_COUNT_PER_MAP = 5;         // 每地图棺椁数量

        // ==================== 多人联机配置 ====================
        public const int TEAM_COUNT = 4;                    // 队伍数量
        public const int PLAYERS_PER_TEAM = 4;              // 每队人数

        // ==================== 网络配置 ====================
        public const string ANNOUNCEMENT_URL = "https://sharechain.qq.com/cc7943c4e737c08bf4f0ec281c1a803d?qq_aio_chat_type=3";
        public const string UPDATE_URL = "https://sharechain.qq.com/b8dcfd993313ac7f6ad5f80d470f0d4d?qq_aio_chat_type=3";

        // ==================== 版本 ====================
        public const string APP_VERSION = "1.0.0";
        public const string APP_NAME = "공원가족";
    }
}
