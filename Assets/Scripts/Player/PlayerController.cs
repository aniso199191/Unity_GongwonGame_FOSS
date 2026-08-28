using UnityEngine;
using GongWon.Core;
using GongWon.Items;

namespace GongWon.Player
{
    /// <summary>
    /// 玩家控制器 — 移动、攻击、血量、武器切换
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("玩家属性")]
        public int hpId = GameConfig.PLAYER_HP_ID; // 血量ID=145666
        public int maxHp = GameConfig.PLAYER_DEFAULT_HP; // 默认100
        public int currentHp;
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f;

        [Header("战斗")]
        public int currentWeaponId = -1; // 当前装备武器ID
        public int currentWeaponDamage = 10; // 徒手伤害
        public float attackRange = 2f;
        public float attackCooldown = 0.8f;
        private float lastAttackTime;

        [Header("引用")]
        public CharacterController characterController;
        public Animator animator;
        public Transform cameraTransform;

        [Header("状态")]
        public bool isDead = false;
        public bool isInvincible = false;

        private Vector3 moveDirection;

        private void Start()
        {
            currentHp = maxHp;
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            Debug.Log($"[Player] 玩家初始化, HP_ID:{hpId}, HP:{maxHp}");
        }

        private void Update()
        {
            if (isDead) return;
            HandleMovement();
            HandleAttack();
        }

        /// <summary>
        /// 玩家移动（WASD/虚拟摇杆）
        /// </summary>
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // 相对于摄像机的移动方向
            if (cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();
                moveDirection = forward * vertical + right * horizontal;
            }
            else
            {
                moveDirection = new Vector3(horizontal, 0, vertical);
            }

            if (moveDirection.magnitude > 0.1f)
            {
                // 移动
                characterController?.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
                // 旋转
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                // 动画
                if (animator != null) animator.SetBool("IsMoving", true);
            }
            else
            {
                if (animator != null) animator.SetBool("IsMoving", false);
            }
        }

        /// <summary>
        /// 攻击输入处理
        /// </summary>
        private void HandleAttack()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                TryAttack();
            }
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        public void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            lastAttackTime = Time.time;

            if (animator != null) animator.SetTrigger("Attack");

            // 检测攻击范围内的怪物
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * attackRange * 0.5f, attackRange);
            foreach (Collider col in hitColliders)
            {
                GongWon.Monsters.Monster monster = col.GetComponent<GongWon.Monsters.Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(currentWeaponDamage);
                    Debug.Log($"[Player] 攻击 {monster.monsterName}, 造成 {currentWeaponDamage} 伤害");
                }
            }
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        public void EquipWeapon(int itemId)
        {
            currentWeaponId = itemId;
            if (GameConfig.ItemBaseDamage.TryGetValue(itemId, out int damage))
            {
                currentWeaponDamage = damage;
            }
            else
            {
                currentWeaponDamage = 10; // 默认徒手
            }
            Debug.Log($"[Player] 装备武器 ID:{itemId}, 伤害:{currentWeaponDamage}");
        }

        /// <summary>
        /// 获取当前武器伤害（用于"너"怪物反伤机制）
        /// </summary>
        public int GetCurrentWeaponDamage()
        {
            return currentWeaponDamage;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (isInvincible || isDead) return;

            currentHp -= damage;
            Debug.Log($"[Player] 受到 {damage} 伤害，剩余HP: {currentHp}");

            // 受伤反馈
            if (animator != null) animator.SetTrigger("Hurt");

            if (currentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(int amount)
        {
            currentHp = Mathf.Min(currentHp + amount, maxHp);
            Debug.Log($"[Player] 恢复 {amount} HP，当前HP: {currentHp}");
        }

        private void Die()
        {
            isDead = true;
            currentHp = 0;
            if (animator != null) animator.SetTrigger("Die");
            Debug.Log("[Player] 玩家死亡");
            // 通知游戏管理器
            GongWon.Core.GameManager.Instance?.ChangeState(GongWon.Core.GameManager.GameState.GameOver);
        }

        /// <summary>
        /// 复活
        /// </summary>
        public void Respawn()
        {
            isDead = false;
            currentHp = maxHp;
            if (animator != null) animator.SetTrigger("Respawn");
            Debug.Log("[Player] 玩家复活");
        }
    }
}
