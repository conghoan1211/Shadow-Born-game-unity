using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script
{
    public class Player : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 8f;
        [SerializeField] float leftBoundary = -40f;  // Giới hạn bên trái
        [SerializeField] float rightBoundary = 40f;  // Giới hạn bên phải
        [SerializeField] float bottomBoundary = -19f; // Giới hạn dưới
        [SerializeField] float topBoundary = 19f;     // Giới hạn trên


        Rigidbody2D myRigidbody;
        public Animator animator;

        public Vector2 moveInput;

        private bool isAttack = false;
        private bool isAttacking = false;  // Để theo dõi trạng thái tấn công


        [SerializeField] private Transform groundCheck;  // Transform của GroundCheck
        [SerializeField] private LayerMask groundLayer;  // Lớp mặt đất
        [SerializeField] float JumpForce;
        [SerializeField] int maxJumpCount = 2;  // Cho phép nhảy 2 lần (double jump)
        private int jumpCount = 0;  // Đếm số lần nhảy hiện tại
        private bool isGround = false;


        void Start()
        {
            myRigidbody = GetComponent<Rigidbody2D>();
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            animator = GetComponent<Animator>();

        }

        void Update()
        {
            Move();
            UpdateDirection();
            Jump();

        }
        void Move()
        {
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
            // Di chuyển nhân vật bằng cách thay đổi vận tốc của Rigidbody2D
            myRigidbody.velocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.velocity.y);

            animator.SetBool("Walking", moveInput.sqrMagnitude != 0);
        }

        void LimitMove()
        {
            Vector3 clampedPosition = transform.position;

            clampedPosition.x = Mathf.Clamp(clampedPosition.x, leftBoundary, rightBoundary);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, bottomBoundary, topBoundary);

            transform.position = clampedPosition;
        }

        void Jump()
        {
            if (Input.GetButtonDown("Jump") && Mathf.Abs(myRigidbody.velocity.y) < 0.001f)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, JumpForce);
            }

            bool isJump = Input.GetKeyDown(KeyCode.Space);
            if (isJump && jumpCount < maxJumpCount)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 0f);  // Reset vận tốc theo trục Y
                myRigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);  // Nhảy với lực
                animator.SetBool("Jump", true);
                jumpCount++;  // Tăng số lần nhảy
                isGround = false;
            }

            if (myRigidbody.velocity.y < 0.01f && isGround)  // Đang ở trên đất
            {
                animator.SetBool("Jump", false);
                jumpCount = 0;
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGround = true;
                jumpCount = 0;  // Reset số lần nhảy
                //GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 1f), ForceMode2D.Impulse);  // Thêm một lực nhỏ hướng lên
            }
        }
        public void Replay()
        {
            SceneManager.LoadScene("GamePlay");
        }

        private void UpdateDirection()
        {
            // Chỉ thay đổi hướng nếu moveInput.x khác với hướng hiện tại của nhân vật
            if (moveInput.x > 0 && transform.localScale.x < 0 || moveInput.x < 0 && transform.localScale.x > 0)
            {
                // Thay đổi hướng bằng cách nhân với Mathf.Sign để điều chỉnh scale theo hướng di chuyển
                transform.localScale = new Vector3(1f * Mathf.Sign(moveInput.x), 1f, 1f);
            }
        }
    }
}