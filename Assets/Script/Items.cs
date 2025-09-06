using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Items : MonoBehaviour
{
    public string itemName;      // Tên vật phẩm
    public int quantity = 1;     // Số lượng vật phẩm (nếu cần)

    private AudioSource collectAudioSource; // Thêm AudioSource cho shoot
    [SerializeField] private AudioClip collectSound; // Âm thanh khi bắn

    private void Start()
    {
        collectAudioSource = gameObject.AddComponent<AudioSource>();
        collectAudioSource.clip = collectSound;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory.instance.AddItem(this);
            if (!collectAudioSource.isPlaying) // Kiểm tra âm thanh có đang phát không
            {
                collectAudioSource.volume = 1.0f; // Đặt âm lượng tối đa
                collectAudioSource.PlayOneShot(collectSound);
                Debug.Log("collect");
            }
            Destroy(gameObject, 0.2f);
        }
    }
}
