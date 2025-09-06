using Assets.Script;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossLV3 : MonoBehaviour
{
    public GameObject GameManagerGO;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private bool facingRight = true;

    [SerializeField] private Vector2 patrolDirection = Vector2.right;
    [SerializeField] private float patrolDistance = 10f;
    private float patrolStartPosX;
    private Vector3 initialScale;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private int attack1Damage = 1;
    [SerializeField] private int attack2Damage = 1;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attack1Probability = 0.5f;

    [Header("Attack Hitboxes")]
    [SerializeField] private Transform attack1HitboxPoint;
    [SerializeField] private Transform attack2HitboxPoint;
    [SerializeField] private Vector2 attack1HitboxSize = new Vector2(2f, 2f);
    [SerializeField] private Vector2 attack2HitboxSize = new Vector2(3f, 2f);

    [Header("Health")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    private GameObject healthBarObject;
    private Slider healthSlider;
    private Image fillImage;

    [Header("Hurt")]
    [SerializeField] private float hurtDuration = 0.5f;
    [SerializeField] private float invulnerabilityDuration = 1f; // Time boss cannot be hurt again
    [SerializeField] private float knockbackForce = 3f;         // Reduced from 5f
    [SerializeField] private float knockbackResistance = 0.5f;  // How much knockback is reduced
    private bool isHurt = false;
    private bool isInvulnerable = false;
    private float hurtEndTime;
    private float invulnerabilityEndTime;
    private SpriteRenderer spriteRenderer;

    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isGrounded;
    private bool isAttacking = false;

    private static readonly string IS_WALKING = "IsWalking";
    private static readonly string ATTACK_1 = "Attack1";
    private static readonly string ATTACK_2 = "Attack2";
    private static readonly string DIE = "Die";
    private static readonly string HURT = "Hurt";

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        patrolStartPosX = transform.position.x;
        initialScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        InitializeHealthBar();

    }

    private void InitializeHealthBar()
    {
        // Ensure we have a canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Create health bar
        if (healthBarPrefab != null)
        {
            healthBarObject = Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity);
            healthBarObject.transform.SetParent(canvas.transform, false);

            // Get components
            healthSlider = healthBarObject.GetComponent<Slider>();
            Transform fillArea = healthBarObject.transform.Find("Fill Area/Fill");
            if (fillArea != null)
            {
                fillImage = fillArea.GetComponent<Image>();
                fillImage.color = Color.red;
            }

            // Configure slider
            if (healthSlider != null)
            {
                healthSlider.minValue = 0;
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }
        else
        {
            Debug.LogError("Health Bar Prefab is not assigned!");
        }
    }


    private void LateUpdate()
    {
        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarObject != null && Camera.main != null)
        {
            // Convert world position to screen position
            Vector3 worldPosition = transform.position + healthBarOffset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            // Only update if the object is in front of the camera
            if (screenPosition.z > 0)
            {
                healthBarObject.transform.position = new Vector3(screenPosition.x, screenPosition.y, 0);
            }
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isDead) return;

        // Check if hurt state should end
        if (isHurt && Time.time >= hurtEndTime)
        {
            isHurt = false;
        }

        if (Time.time >= invulnerabilityEndTime)
        {
            isInvulnerable = false;
        }

        if (!isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            animator.SetBool(IS_WALKING, false);
            return;
        }

        // Don't move if hurt or attacking
        if (isHurt || isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StopMovement();
                if (Random.value < attack1Probability)
                {
                    Attack1();
                }
                else
                {
                    Attack2();
                }
            }
            else
            {
                MoveTowardsPlayer();
            }
            UpdateFacingTowardsPlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetBool(IS_WALKING, false);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Patrol()
    {
        if (!isGrounded) return;

        animator.SetBool(IS_WALKING, true);

        if (Mathf.Abs(transform.position.x - patrolStartPosX) >= patrolDistance)
        {
            patrolDirection = -patrolDirection;
            patrolStartPosX = transform.position.x;
            transform.localScale = new Vector3(
                initialScale.x * (patrolDirection.x > 0 ? 1 : -1),
                initialScale.y,
                initialScale.z
            );
        }

        // Apply movement only when grounded
        float targetSpeed = patrolDirection.x * moveSpeed;
        rb.velocity = new Vector2(targetSpeed, rb.velocity.y);
    }

    private void MoveTowardsPlayer()
    {
        if (!isGrounded) return;

        float direction = player.position.x > transform.position.x ? 1 : -1;

        // Smooth movement towards player
        float targetSpeed = direction * moveSpeed;
        float currentSpeed = rb.velocity.x;
        float smoothedSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.3f);

        rb.velocity = new Vector2(smoothedSpeed, rb.velocity.y);
        animator.SetBool(IS_WALKING, true);
    }

    private void UpdateFacingTowardsPlayer()
    {
        transform.localScale = new Vector3(
            initialScale.x * (player.position.x > transform.position.x ? 1 : -1),
            initialScale.y,
            initialScale.z
        );
    }

    private void Attack1()
    {
        isAttacking = true;
        StopMovement();
        animator.SetTrigger(ATTACK_1);
        lastAttackTime = Time.time;
    }

    private void Attack2()
    {
        isAttacking = true;
        StopMovement();
        animator.SetTrigger(ATTACK_2);
        lastAttackTime = Time.time;
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    public void ApplyAttack1Damage()
    {
        Debug.Log($"ApplyAttack1Damage called, hitbox position: {attack1HitboxPoint.position}");

        // Draw debug visualization of the hitbox
        Debug.DrawLine(
            attack1HitboxPoint.position - new Vector3(attack1HitboxSize.x / 2, attack1HitboxSize.y / 2),
            attack1HitboxPoint.position + new Vector3(attack1HitboxSize.x / 2, attack1HitboxSize.y / 2),
            Color.red,
            1f
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attack1HitboxPoint.position,
            attack1HitboxSize,
            0f
        );

        Debug.Log($"Found {hits.Length} colliders in hitbox");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"Checking collider on: {hit.gameObject.name} with tag: {hit.tag}");
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Found player!");
                PlayerLV3 playerController = hit.GetComponent<PlayerLV3>();
                if (playerController != null)
                {
                    Debug.Log($"Applying {attack1Damage} damage to player");
                    playerController.TakeDamage(attack1Damage);
                }
            }
        }
    }

    public void ApplyAttack2Damage()
    {
        Debug.Log($"ApplyAttack2Damage called, hitbox position: {attack2HitboxPoint.position}");

        // Draw debug visualization of the hitbox
        Debug.DrawLine(
            attack2HitboxPoint.position - new Vector3(attack2HitboxSize.x / 2, attack2HitboxSize.y / 2),
            attack2HitboxPoint.position + new Vector3(attack2HitboxSize.x / 2, attack2HitboxSize.y / 2),
            Color.blue,
            1f
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attack2HitboxPoint.position,
            attack2HitboxSize,
            0f
        );

        Debug.Log($"Found {hits.Length} colliders in hitbox");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"Checking collider on: {hit.gameObject.name} with tag: {hit.tag}");
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Found player!");
                PlayerLV3 playerController = hit.GetComponent<PlayerLV3>();
                if (playerController != null)
                {
                    Debug.Log($"Applying {attack2Damage} damage to player");
                    playerController.TakeDamage(attack2Damage);
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Update health bar
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;

            // Change color based on health percentage
            if (fillImage != null)
            {
                float healthPercent = currentHealth / maxHealth;
                fillImage.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Rest of your damage handling code...
        if (!isInvulnerable)
        {
            isHurt = true;
            isInvulnerable = true;
            hurtEndTime = Time.time + hurtDuration;
            invulnerabilityEndTime = Time.time + invulnerabilityDuration;
            animator.SetTrigger(HURT);

            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            float adjustedKnockback = knockbackForce * knockbackResistance;
            rb.velocity = new Vector2(knockbackDirection.x * adjustedKnockback, rb.velocity.y);

            StartCoroutine(FlashEffect());
        }
    }

    private void Die()
    {
        isDead = true;
        isInvulnerable = false;
        animator.SetTrigger(DIE);
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.gravityScale = 0;

        // Destroy health bar
        if (healthBarObject != null)
        {
            Destroy(healthBarObject);
        }

        // Notify the GameControllerLV3 that the boss has been defeated
        GameManagerGO.GetComponent<GameControllerLV3>().BossDefeated(Time.time);

        StartCoroutine(DisableColliderAfterAnimation());
        Destroy(gameObject, 2f);
    }

    private IEnumerator FlashEffect()
    {
        // Flash the boss while invulnerable
        Color originalColor = spriteRenderer.color;
        float flashInterval = 0.1f;

        while (isInvulnerable)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.7f); // Reddish flash
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }

        spriteRenderer.color = originalColor; // Ensure color is reset
    }

    private IEnumerator DisableColliderAfterAnimation()
    {
        // Wait for death animation to complete
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnDrawGizmos()
    {
        if (attack1HitboxPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attack1HitboxPoint.position, attack1HitboxSize);
        }

        if (attack2HitboxPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(attack2HitboxPoint.position, attack2HitboxSize);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}