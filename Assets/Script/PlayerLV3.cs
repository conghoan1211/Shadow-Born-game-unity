using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class PlayerLV3 : MonoBehaviour
    {
        public GameObject GameManagerGO;
        private GameObject scoreTextGO;
        public AudioClip ItemSound;

        #region Move settings
        [Header("Horizontal Movement Settings")]
        [SerializeField] private float walkSpeed = 1;
        private AudioSource moveAudioSource; // Thêm AudioSource cho shoot
        [SerializeField] private AudioClip moveSound; // Âm thanh khi bắn
        [Space(5)]

        [Header("Vertical Movement Settings")]
        [SerializeField] private float jumpForce = 45f;
        private int jumpBufferCounter = 0;
        [SerializeField] private int jumpBufferFrames;
        private float coyoteTimeCounter = 0;
        [SerializeField] private float coyoteTime;
        private int airJumpCounter = 0;
        [SerializeField] private int maxAirJump = 1;
        [Space(5)]
        #endregion

        #region Ground Check Settings
        [Header("Ground Check Settings")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckY = 0.2f;
        [SerializeField] private float groundCheckX = 0.5f;
        [SerializeField] private LayerMask whatIsGround;
        [Space(5)]
        #endregion

        #region Strike Settings
        [Header("Strike Settings")]
        [SerializeField] private float strikeDistance = 5f;
        [SerializeField] private float strikeSpeed = 40f;
        [SerializeField] private float strikeTime = 0.4f;
        [SerializeField] private GameObject strikeDustEffect;
        private bool isStriking = false;
        [SerializeField] private float manaStrikeTake = 0.05f;
        [Space(5)]
        #endregion

        #region Attack Settings
        [Header("Attack Settings")]
        private bool attack = false;
        float timeBetweenAttack, timeSinceAttack;
        [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
        [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
        [SerializeField] LayerMask attackableLayer;
        [SerializeField] float damage;
        [SerializeField] GameObject slashEffect;
        [SerializeField] public GameObject hitEffectPrefab; // Prefab cho hiệu ứng va chạm
        private AudioSource hitAudioSource; // AudioSource cho âm thanh đánh trúng
        [SerializeField] private AudioClip[] hitSounds; // Mảng âm thanh khi đánh trúng enemy
        [Space(5)]
        #endregion

        #region Recoil
        [Header("Recoil")]
        [SerializeField] int recoilXSteps = 5;
        [SerializeField] int recoilYSteps = 5;
        [SerializeField] float recoilXSpeed = 100;
        int stepsXRecoiled, stepsYRecoiled;
        [SerializeField] float recoilYSpeed = 100;
        [Space(5)]
        #endregion

        #region Health Settings
        [Header("Health settings")]
        [SerializeField] public int currentHealth = 5;
        [SerializeField] public int maxHealth = 5;
        [SerializeField] GameObject bloodParticle;
        private HeartController healthDisplay;
        [Space(5)]
        #endregion

        #region Shoot Settings
        [Header("Shoot settings")]
        [SerializeField] float manaShootTake = 0.1f;
        private float healthTimer;
        [SerializeField] private float timeToHealth;
        private AudioSource audioSource;
        [SerializeField] private AudioClip healingSound;
        private GameObject activeHealingEffect; // Tham chiếu đến instance đang hoạt động
        [SerializeField] private GameObject healingEffectPrefab;
        [Space(5)]
        #endregion

        #region Mana Settings
        [Header("Mana settings")]
        [SerializeField] Image manaStorage;
        [SerializeField] float mana;
        [SerializeField] float manaDrainSpeed;
        [SerializeField] float manaGain; // Increase mana by attack normal
        [SerializeField] float timeBetweenShoot = 0.5f;
        [SerializeField] float fireballSpeed = 0.2f; // Tốc độ của viên đạn
        private float timeSinceShoot;
        [SerializeField] private float fireballLifetime = 5f; // Thời gian sống của đạn
        [SerializeField] private float shootDamage;
        [SerializeField] private GameObject sideShootFireBall;
        private AudioSource shootAudioSource; // Thêm AudioSource cho shoot
        [SerializeField] private AudioClip shootSound; // Âm thanh khi bắn
        [Space(5)]
        #endregion

        private Rigidbody2D rb;
        private float xAxis, yAxis;
        private float gravity;
        Animator anim;
        private bool canDash;
        private bool dashed;

        [HideInInspector] public PlayerStateList pState;


        public static PlayerLV3 Instance;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else Instance = this;
            currentHealth = maxHealth;
        }

        private void Start()
        {
            pState = GetComponent<PlayerStateList>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            gravity = rb.gravityScale;
            canDash = true;

            healthDisplay = Object.FindAnyObjectByType<HeartController>();
            healthDisplay.UpdateHealth(currentHealth);
            Mana = mana;
            manaStorage.fillAmount = Mana;

            audioSource = gameObject.AddComponent<AudioSource>();
            shootAudioSource = gameObject.AddComponent<AudioSource>();

            moveAudioSource = gameObject.AddComponent<AudioSource>();
            moveAudioSource.clip = moveSound;

            hitAudioSource = gameObject.AddComponent<AudioSource>();
            hitAudioSource.playOnAwake = false;
            hitAudioSource.volume = 0.5f;
            scoreTextGO = GameObject.FindGameObjectWithTag("ScoreTextTag");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
            Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
            Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        }

        private void Update()
        {
            GetInputs();
            UpdateJumpVariables();
            Move();
            Jump();
            Flip();
            StartStrike();
            Attack();
            Recoil();
            Shoot();
            Heal();
            Die();
        }

        void GetInputs()
        {

            xAxis = Input.GetAxisRaw("Horizontal");
            yAxis = Input.GetAxisRaw("Vertical");

            attack = Input.GetKeyDown(KeyCode.X);
        }

        void Flip()
        {
            if (xAxis < 0)
            {
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
                pState.lookingRight = false;
            }
            else if (xAxis > 0)
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
                pState.lookingRight = true;
            }
        }

        private void Move()
        {
            rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
            anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
            if (Grounded() && rb.velocity.x != 0)  // Kiểm tra di chuyển và trên mặt đất
            {
                if (!moveAudioSource.isPlaying) // Kiểm tra âm thanh có đang phát không
                {
                    moveAudioSource.volume = 0.5f;
                    moveAudioSource.Play(); // Phát âm thanh khi di chuyển
                }
            }
            else
            {
                moveAudioSource.Stop(); // Dừng âm thanh khi không di chuyển
            }
        }
        void StartStrike()
        {
            if (Input.GetKeyDown(KeyCode.Z) && canDash && Mana >= manaStrikeTake && !dashed && !isStriking)
            {
                StartCoroutine(Strike());
            }
        }

        IEnumerator Strike()
        {
            isStriking = true;
            pState.dashing = true;
            anim.SetBool("Strike", true);
            Vector2 startPosition = transform.position;
            Vector2 targetPosition = startPosition + new Vector2(transform.localScale.x * strikeDistance, 0);

            float elapsedTime = 0f;
            float duration = strikeDistance / strikeSpeed; // Thời gian để di chuyển hết khoảng cách

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration; // Tính toán tỷ lệ thời gian
                if (Physics2D.Raycast(transform.position, transform.localScale.x > 0 ? Vector2.right : Vector2.left, strikeDistance, whatIsGround))
                {
                    break; // Dừng nếu va chạm với tường
                }

                transform.position = Vector2.Lerp(startPosition, targetPosition, t); // Di chuyển nhân vật
                elapsedTime += Time.deltaTime;

                yield return null;
            }
            if (Grounded()) Instantiate(strikeDustEffect, transform);

            Mana -= manaStrikeTake;
            yield return new WaitForSeconds(strikeTime); // Thời gian giữ animation nếu cần
            anim.SetBool("Strike", false);
            pState.dashing = false;
            isStriking = false;
        }

        void Attack()
        {
            timeSinceAttack += Time.deltaTime;
            if (attack && timeSinceAttack >= timeBetweenAttack)
            {
                timeSinceAttack = 0;

                if (yAxis == 0 || yAxis < 0 && Grounded())
                {
                    Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                    Instantiate(slashEffect, SideAttackTransform);
                    anim.SetTrigger("Attacking");
                }
                else if (yAxis > 0)
                {
                    Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                    SlashEffectAtAngle(slashEffect, 90, UpAttackTransform);
                    anim.SetTrigger("JumpAttack");

                }
                else if (yAxis < 0 && !Grounded())
                {
                    Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSteps);
                    SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
                    anim.SetTrigger("JumpAttack");

                }
            }
        }

        private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
        {
            Collider2D[] objectsHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

            for (int i = 0; i < objectsHit.Length; i++)
            {
                // Check for both enemy types
                EnemyLV1 enemy = objectsHit[i].GetComponent<EnemyLV1>();
                BossLV3 boss = objectsHit[i].GetComponent<BossLV3>();

                if (enemy != null)
                {
                    enemy.EnemyHit(damage, (transform.position - objectsHit[i].transform.position).normalized, _recoilStrength);
                }
                else if (boss != null)
                {
                    boss.TakeDamage(damage);  // Using boss's existing TakeDamage method
                }

                if (objectsHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                    if (hitSounds.Length > 0)
                    {
                        int randomIndex = Random.Range(0, hitSounds.Length);
                        hitAudioSource.clip = hitSounds[randomIndex];
                        hitAudioSource.PlayOneShot(hitAudioSource.clip);
                    }
                    GameObject _hitEffect = Instantiate(hitEffectPrefab, objectsHit[i].transform.position, Quaternion.identity);
                    Destroy(_hitEffect, 0.5f);
                }
            }
        }

        void SlashEffectAtAngle(GameObject _slashEfect, int _effectAngle, Transform _attackTranform)
        {
            _slashEfect = Instantiate(_slashEfect, _attackTranform);
            _slashEfect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
            _slashEfect.transform.localPosition = new Vector2(transform.localScale.x, transform.localScale.y);

        }

        void Recoil()
        {
            if (pState.recoilingX)
            {
                if (pState.lookingRight)
                {
                    rb.velocity = new Vector2(-recoilXSteps, 0);
                }
                else
                {
                    rb.velocity = new Vector2(recoilXSteps, 0);
                }
            }
            if (pState.recoilingY)
            {
                rb.gravityScale = 0;

                if (yAxis < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
                }
                airJumpCounter = 0;
            }
            else
            {
                rb.gravityScale = gravity;
            }
            //stop recoin
            if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
            {
                stepsXRecoiled++;
            }
            else
            {
                StopRecoilX();
            }
            if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
            {
                stepsYRecoiled++;
            }
            else
            {
                StopRecoilY();
            }
            if (Grounded())
            {
                StopRecoilY();
            }
        }

        void StopRecoilX()
        {
            stepsXRecoiled = 0;
            pState.recoilingX = false;
        }

        void StopRecoilY()
        {
            stepsYRecoiled = 0;
            pState.recoilingY = false;
        }

        public bool Grounded()
        {
            if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
            {
                return true;
            }
            else return false;
        }

        void Jump()
        {
            if (jumpBufferCounter > 0 && (Grounded() || coyoteTimeCounter > 0))
            {
                // Nhảy lần đầu hoặc nhảy trong thời gian coyote
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
                anim.SetBool("Jump", true);

                // Reset các biến khi nhảy
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
                airJumpCounter = 0; // Reset số lần nhảy trên không khi chạm đất hoặc nhảy coyote
            }
            else if (!Grounded() && airJumpCounter < maxAirJump && Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
                anim.SetBool("Jump", true);

                airJumpCounter++; // Tăng số lần nhảy trên không
            }
            // Đặt lại animation nếu tiếp đất
            anim.SetBool("Jump", !Grounded());
        }

        void UpdateJumpVariables()
        {
            if (Grounded())
            {
                pState.jumping = false;
                coyoteTimeCounter = coyoteTime;
                airJumpCounter = 0;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferFrames;
            }
            else
            {
                jumpBufferCounter--;
            }
        }


        /**Take damage*/
        public void TakeDamage(int _damage)
        {
            currentHealth -= _damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, healthDisplay.maxHealth);
            healthDisplay.UpdateHealth(currentHealth);
            StartCoroutine(StopTakingDamage());
        }

        IEnumerator StopTakingDamage()
        {
            pState.invincible = true;
            anim.SetTrigger("TakeDamage");
            GameObject _bloodParticles = Instantiate(bloodParticle, transform.position, Quaternion.identity);
            Destroy(_bloodParticles, 1.5f);
            ClampHealth();
            yield return new WaitForSeconds(1f);
            pState.invincible = false;
        }

        void ClampHealth()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.GetComponent<EnemyLV1>() != null && pState.shoot)
            {
                _other.GetComponent<EnemyLV1>().EnemyHit(shootDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
            }

            if (_other.CompareTag("BonusItem"))
            {
                scoreTextGO.GetComponent<GameScore>().Score += 1;
                
                AudioSource.PlayClipAtPoint(ItemSound, transform.position);
                Destroy(_other.gameObject);

            }

            if (_other.CompareTag("Finish"))
            {

                GameManagerGO.GetComponent<GameController>().SetGameManagerState(GameController.GameManagerState.Victory);
                
                gameObject.SetActive(false);
            }
        }

        void Shoot()
        {
            if (Input.GetKeyDown(KeyCode.C) && timeSinceShoot >= timeBetweenShoot && Mana >= manaShootTake)
            {
                pState.shoot = true;
                timeSinceShoot = 0;
                StartCoroutine(ShootCoroutine());
            }
            else
            {
                timeSinceShoot += Time.deltaTime;
            }
        }
        IEnumerator ShootCoroutine()
        {
            anim.SetBool("Shoot", true);
            yield return new WaitForSeconds(0.25f); // Đợi animation

            GameObject fireBall = Instantiate(sideShootFireBall, SideAttackTransform.position, Quaternion.identity);
            pState.shoot = true;
            if (shootSound != null)
            {
                shootAudioSource.PlayOneShot(shootSound);
            }
            if (rb != null)
            {
                if (pState.lookingRight)
                {
                    fireBall.transform.eulerAngles = Vector3.zero;
                }
                else
                {
                    fireBall.transform.eulerAngles = new Vector2(fireBall.transform.eulerAngles.x, 180);
                }
                pState.recoilingX = true;
                Destroy(fireBall, fireballLifetime);
            }
            Mana -= manaShootTake;
            yield return new WaitForSeconds(0.35f);
            anim.SetBool("Shoot", false);
        }

        IEnumerator ShootCoroutine1() //delete
        {
            // Bắt đầu animation bắn
            anim.SetBool("Shoot", true);

            yield return new WaitForSeconds(0.45f); // Đợi animation

            // Kiểm tra điều kiện bắn (đứng trên mặt đất hoặc không nhảy cao)
            if (yAxis == 0 || (yAxis <= 0 && Grounded()))
            {
                // Tạo đạn
                GameObject fireBall = Instantiate(sideShootFireBall, SideAttackTransform.position, Quaternion.identity);
                Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
                pState.shoot = true;

                if (rb != null)
                {
                    if (pState.lookingRight)
                    {
                        fireBall.transform.rotation = Quaternion.identity;
                        rb.velocity = Vector2.right * fireballSpeed;
                    }
                    else
                    {
                        fireBall.transform.rotation = Quaternion.Euler(0, 180, 0);
                        rb.velocity = Vector2.left * fireballSpeed;
                    }
                    pState.recoilingX = true;
                    Destroy(fireBall, fireballLifetime);
                }
                // Xử lý tiêu hao mana ở đây
                Mana -= manaShootTake;
            }

            // Kết thúc animation và reset trạng thái
            yield return new WaitForSeconds(0.5f);
            anim.SetBool("Shoot", false);
        }

        void Heal()
        {
            if (Input.GetKey(KeyCode.F) && currentHealth < maxHealth && Mana > 0 && !pState.jumping && !pState.dashing)
            {
                pState.healing = true;
                healthTimer += Time.deltaTime;
                anim.SetBool("Healing", true);
                if (activeHealingEffect == null)
                {
                    activeHealingEffect = Instantiate(healingEffectPrefab, transform.position, Quaternion.identity);
                    activeHealingEffect.transform.parent = transform; // Đặt prefab làm con của nhân vật để theo dõi vị trí
                }
                if (!audioSource.isPlaying) // Phát âm thanh nếu chưa đang phát
                {
                    audioSource.clip = healingSound;
                    audioSource.Play();
                }
                if (healthTimer >= timeToHealth)
                {
                    currentHealth++;
                    healthDisplay.UpdateHealth(currentHealth);
                    healthTimer = 0;
                }
                Mana -= Time.deltaTime * manaDrainSpeed;
            }
            else
            {
                pState.healing = false;
                anim.SetBool("Healing", false);
                healthTimer = 0;
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                if (activeHealingEffect != null)
                {
                    Destroy(activeHealingEffect);
                    activeHealingEffect = null;
                }
            }
        }

        float Mana
        {
            get { return mana; }
            set
            {
                if (mana != value)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                    manaStorage.fillAmount = Mana;
                }
            }
        }

        void Die()
        {
            bool hasDied = false;
            if (currentHealth <= 0 && !hasDied)
            {
                hasDied = true;
                pState.alive = false;
                pState.dashing = false;
                pState.jumping = false;
                anim.SetTrigger("Die");
                walkSpeed = 0;
                rb.velocity = Vector2.zero;
                StartCoroutine(WaitForDeathAnimation());
                GameManagerGO.GetComponent<GameControllerLV3>().SetGameManagerState(GameControllerLV3.GameManagerState.GameOver);
            }

            IEnumerator WaitForDeathAnimation()
            {
                yield return new WaitForSeconds(2f);
                Destroy(gameObject);
            }

        }

    }
}