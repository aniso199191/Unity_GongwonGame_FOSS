using UnityEngine;
using GongWon.Core;

namespace GongWon.Monsters
{
    /// <summary>
    /// 怪物基类 — 所有怪物通过ID加载，不同材质/画质有不同外观
    /// </summary>
    public class Monster : MonoBehaviour
    {
        [Header("怪物配置")]
        public int monsterId;
        public string monsterName;
        public int maxHp = 100;
        public int currentHp;
        public int attackDamage = 10;
        public float attackRange = 2f;
        public float moveSpeed = 2f;
        public float attackCooldown = 1.5f;

        [Header("外观")]
        public Renderer monsterRenderer;
        public Material normalMaterial;
        public Material highQualityMaterial;

        [Header("AI状态")]
        public MonsterState currentState = MonsterState.Idle;
        private Transform targetPlayer;
        private float lastAttackTime;
        private bool isNerfed = false; // 新手教程削弱

        public enum MonsterState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }

        /// <summary>
        /// 通过ID初始化怪物
        /// </summary>
        public void Initialize(int id, bool nerfed = false)
        {
            monsterId = id;
            isNerfed = nerfed;

            if (GameConfig.MonsterNames.TryGetValue(id, out string name))
            {
                monsterName = name;
            }
            else
            {
                monsterName = $"未知怪物_{id}";
            }

            // 根据ID设置不同属性
            SetupMonsterStats(id);

            // 新手教程削弱
            if (isNerfed)
            {
                maxHp = Mathf.FloorToInt(maxHp * 0.5f);
                attackDamage = Mathf.FloorToInt(attackDamage * 0.3f);
                moveSpeed *= 0.6f;
                Debug.Log($"[Monster] {monsterName} 已削弱（新手教程）");
            }

            currentHp = maxHp;
            gameObject.name = $"Monster_{id}_{monsterName}";
            Debug.Log($"[Monster] 初始化: {monsterName}(ID:{id}), HP:{maxHp}, 攻击:{attackDamage}");
        }

        /// <summary>
        /// 根据怪物ID配置属性和外观
        /// </summary>
        private void SetupMonsterStats(int id)
        {
            switch (id)
            {
                case 30024: // 공악
                    maxHp = 200; attackDamage = 15; moveSpeed = 2.5f; attackRange = 2.5f;
                    break;
                case 32111: // 두려워하다
                    maxHp = 150; attackDamage = 20; moveSpeed = 3f; attackRange = 2f;
                    break;
                case 45273: // 사영
                    maxHp = 180; attackDamage = 18; moveSpeed = 2.8f; attackRange = 3f;
                    break;
                case 15766: // 저항 영
                    maxHp = 300; attackDamage = 12; moveSpeed = 1.5f; attackRange = 2f;
                    break;
                case 5764649: // 죽을 무서워하는 녀
                    maxHp = 500; attackDamage = 30; moveSpeed = 2f; attackRange = 3.5f;
                    break;
                case 578864: // 장무성
                    maxHp = 250; attackDamage = 22; moveSpeed = 2.2f; attackRange = 2.5f;
                    break;
                case 576: // 오금찬
                    maxHp = 120; attackDamage = 25; moveSpeed = 3.5f; attackRange = 1.8f;
                    break;
                case 46945: // 넌 제일 무서워하는 것
                    maxHp = 800; attackDamage = 40; moveSpeed = 1.8f; attackRange = 4f;
                    break;
                case 1: // 너 — 特殊：伤害等于玩家当前武器伤害
                    maxHp = 999; attackDamage = 0; moveSpeed = 2f; attackRange = 2.5f;
                    break;
                case 24675: // 가족
                    maxHp = 350; attackDamage = 25; moveSpeed = 2.3f; attackRange = 3f;
                    break;
                case 454679: // 사람의 가장 음침한 면
                    maxHp = 600; attackDamage = 35; moveSpeed = 2f; attackRange = 3.5f;
                    break;
                default:
                    maxHp = 100; attackDamage = 10; moveSpeed = 2f; attackRange = 2f;
                    break;
            }
        }

        private void Update()
        {
            if (currentState == MonsterState.Dead) return;

            // "너"怪物的特殊机制：攻击伤害等于玩家当前武器伤害
            if (monsterId == 1 && targetPlayer != null)
            {
                var player = targetPlayer.GetComponent<GongWon.Player.PlayerController>();
                if (player != null)
                {
                    attackDamage = player.GetCurrentWeaponDamage();
                }
            }

            UpdateAI();
        }

        /// <summary>
        /// 怪物AI逻辑
        /// </summary>
        private void UpdateAI()
        {
            if (targetPlayer == null)
            {
                FindNearestPlayer();
                currentState = MonsterState.Idle;
                return;
            }

            float distance = Vector3.Distance(transform.position, targetPlayer.position);

            if (distance <= attackRange)
            {
                currentState = MonsterState.Attack;
                TryAttack();
            }
            else if (distance <= 15f)
            {
                currentState = MonsterState.Chase;
                ChasePlayer();
            }
            else
            {
                currentState = MonsterState.Patrol;
                Patrol();
            }
        }

        private void FindNearestPlayer()
        {
            // 查找最近的玩家
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float minDist = float.MaxValue;
            foreach (GameObject p in players)
            {
                float dist = Vector3.Distance(transform.position, p.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetPlayer = p.transform;
                }
            }
        }

        private void ChasePlayer()
        {
            if (targetPlayer == null) return;
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(targetPlayer);
        }

        private void TryAttack()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                // 对玩家造成伤害
                var player = targetPlayer.GetComponent<GongWon.Player.PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                    Debug.Log($"[Monster] {monsterName} 攻击玩家，造成 {attackDamage} 伤害");
                }
            }
        }

        private void Patrol()
        {
            // 简单巡逻逻辑
            transform.Rotate(0, Mathf.Sin(Time.time) * 0.5f, 0);
            transform.Translate(Vector3.forward * moveSpeed * 0.3f * Time.deltaTime);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHp -= damage;
            Debug.Log($"[Monster] {monsterName} 受到 {damage} 伤害，剩余HP: {currentHp}");

            if (currentHp <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            currentState = MonsterState.Dead;
            Debug.Log($"[Monster] {monsterName} 已死亡");
            // 播放死亡动画、掉落物品等
            Destroy(gameObject, 2f);
        }
    }
}
