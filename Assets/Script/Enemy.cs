using Assets.Script;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Default settings")]
    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected int damage;
    [SerializeField] private float attackCooldown = 1f; // Tăng cooldown lên 2 giây
    private float lastAttackTime = -2f; // Khởi tạo âm để có thể tấn công ngay lập tức khi bắt đầu
    [Space(5)]

    [Header("Area Movement")]
    [SerializeField] private float leftBoundary;  // Giới hạn bên trái
    [SerializeField] private float rightBoundary;  // Giới hạn bên phải
    [Space(5)]

    [Header("Recoil Movement")]
    [SerializeField] protected float recoilLengt;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [Header("Item Drop")]
    [SerializeField] private GameObject itemPrefab; // Prefab của vật phẩm sẽ spawn khi enemy chết

    private bool canAttack = true;

    [SerializeField] protected PlayerController player;
    protected float recoilTimer;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // player = PlayerController.Instance;
        FindPlayer();
    }

    public virtual void Start()
    {

    }
    protected void FindPlayer()
    {
        // Tìm player theo nhiều cách khác nhau
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance;
        }
        else
        {
            // Backup plan nếu singleton không hoạt động
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        }
    }


    protected virtual void Update()
    {
        Move();
        HandleAttackCooldown();
        Die();

        if (player == null)
        {
            FindPlayer();
            return;
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
        rb.velocity = new Vector2(speed, 0f);

        if (transform.position.x < leftBoundary)
        {
            speed = Math.Abs(speed);
            FlipEnemyFacing();
            transform.position = new Vector2(leftBoundary + 0.1f, transform.position.y);
        }
        else if (transform.position.x > rightBoundary)
        {
            speed = -Math.Abs(speed);
            FlipEnemyFacing();
            transform.position = new Vector2(rightBoundary - 0.1f, transform.position.y);
        }
    }
    void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(-(Mathf.Sign(speed)), 1f);
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
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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

    public virtual void Die()
    {
        if (health <= 0)
        {
            SpawnItem(); 
            Destroy(gameObject);
        }
    }
    private void SpawnItem()
    {
        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }
}

