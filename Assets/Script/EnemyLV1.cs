using System;
using UnityEngine;

namespace Assets.Script
{
    public class EnemyLV1 : MonoBehaviour
    {
        public GameObject GameManagerGO;
        public GameObject playerGO;
        public GameObject crashEffect;


        [Header("Default settings")]
        [SerializeField] protected float health;
        [SerializeField] protected float speed;
        [SerializeField] protected int damage;
        [SerializeField] private float attackCooldown = 1f;
        private float lastAttackTime = -2f;
        [Space(5)]

        [Header("Area Movement")]
        [SerializeField] private float movementRange = 5f;
        private float initialPositionX;
        [Space(5)]

        [Header("Player Detection")]
        [SerializeField] private float detectionRange = 5f;

        [Header("Recoil Movement")]
        [SerializeField] protected float recoilLengt;
        [SerializeField] protected float recoilFactor;
        [SerializeField] protected bool isRecoiling = false;

        [Header("Movement Settings")]
        [SerializeField] private float acceleration = 2f;
        [SerializeField] private float maxSpeed = 3f;

        private bool canAttack = true;

        [SerializeField] protected PlayerLV1 player;
        protected float recoilTimer;
        protected Rigidbody2D rb;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            FindPlayer();
        }

        public virtual void Start()
        {
            initialPositionX = transform.position.x;
        }

        protected void FindPlayer()
        {
            if (PlayerLV1.Instance != null)
            {
                player = PlayerLV1.Instance;
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerLV1>();
            }
        }

        protected virtual void Update()
        {
            if (player == null)
            {
                FindPlayer();
                return;
            }

            Move();
            HandleAttackCooldown();

            if (health <= 0)
            {
                Destroy(gameObject);
            }

            #region Recoiling
            if (isRecoiling)
            {
                if (recoilTimer < recoilLengt)
                {
                    recoilTimer += Time.deltaTime;
                }
                else
                {
                    isRecoiling = false;
                    recoilTimer = 0;
                }
            }
            #endregion
        }

        void Move()
        {
            // Tính khoảng cách tới Player
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            // Kiểm tra nếu Player nằm trong phạm vi phát hiện
            if (distanceToPlayer <= detectionRange)
            {
                // Di chuyển về phía Player với gia tốc nhưng chỉ trên trục x
                Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(directionToPlayer.x * speed, rb.velocity.y), Time.deltaTime * acceleration);

                // Giới hạn vận tốc tối đa trên trục x
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);

                FlipEnemyFacing(directionToPlayer.x);
            }
            else
            {
                // Di chuyển qua lại trong phạm vi ban đầu, chỉ thay đổi trục x
                float targetPositionX = initialPositionX + Mathf.PingPong(Time.time * speed, movementRange * 2) - movementRange;
                float moveDirection = targetPositionX - transform.position.x;
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(moveDirection * speed, rb.velocity.y), Time.deltaTime * acceleration);

                // Giới hạn vận tốc tối đa trên trục x
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);

                FlipEnemyFacing(rb.velocity.x);
            }
        }

        void FlipEnemyFacing(float direction)
        {
            transform.localScale = new Vector2(-Mathf.Sign(direction), 1f);
        }

        private void HandleAttackCooldown()
        {
            if (!canAttack && Time.time >= lastAttackTime + attackCooldown)
            {
                canAttack = true;
            }
        }

        public virtual void EnemyHit(float _dame, Vector2 _hitDirection, float _hitForce)
        {
            health -= _dame;
            if (health <= 0)
            {
                Destroy(gameObject); // Tiêu diệt Enemy khi máu <= 0
            }

            if (!isRecoiling)
            {
                rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
                isRecoiling = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject.transform;
                var crash = Instantiate(crashEffect, player.position, player.rotation);
                crash.GetComponent<ParticleSystem>().Play();

                Attack();
            }
        }

        protected virtual void Attack()
        {
            if (canAttack && !player.pState.invincible)
            {
                player.TakeDamage(damage);
                lastAttackTime = Time.time;
                canAttack = false;
            }
        }
    }
}
