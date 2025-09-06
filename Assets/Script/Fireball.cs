using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float hitForce;
    [SerializeField] float speed;
    [SerializeField] float lifeTime = 1;
    [SerializeField] public AudioClip hitSound;   // Sound when hitting an enemy
    private AudioSource hitAudioSource; // Thêm AudioSource cho shoot
    void Start()
    {
        Destroy(gameObject, lifeTime);
        hitAudioSource = gameObject.AddComponent<AudioSource>();
        hitAudioSource.clip = hitSound;
    }

    private void FixedUpdate()
    {
        transform.position += speed * transform.right;
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        Enemy enemy = _other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Vector2 hitDirection = (_other.transform.position - transform.position).normalized;
            enemy.EnemyHit(damage, hitDirection, -hitForce);
            if (!hitAudioSource.isPlaying) 
            {
                hitAudioSource.volume = 1.0f; 
                hitAudioSource.PlayOneShot(hitSound);
                Debug.Log("hit");
            }
            Destroy(gameObject, 0.1f); // Hủy đạn sau khi va chạm với enemy
        }
        else if (_other.tag != "Player") 
        {
            Destroy(gameObject);  
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
