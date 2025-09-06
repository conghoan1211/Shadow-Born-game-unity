using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HeartController : MonoBehaviour
{

    [SerializeField] private GameObject heartPrefab; // Prefab hình trái tim
    [SerializeField] private Transform heartContainer; // Container chứa các hình ảnh trái tim
    [SerializeField] public int maxHealth = 5; // Số lượng tối đa của health

    private List<GameObject> hearts = new List<GameObject>();

    public void UpdateHealth(int currentHealth)
    {
        // Xóa các trái tim cũ
        foreach (GameObject heart in hearts)
        {
            Destroy(heart);
        }
        hearts.Clear();

        // Tạo lại các trái tim theo số lượng health hiện tại
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            hearts.Add(newHeart);
        }
    }



    //PlayerController player;

    //private GameObject[] heartContainers;
    //private Image[] heartFills;
    //public Transform heartsParent;
    //public GameObject heartContainerPrefab;

    //void Start()
    //{
    //    player  =PlayerController.Instance;
    //    heartContainers = new GameObject[PlayerController.Instance.maxHealth];
    //    heartFills = new Image[PlayerController.Instance.maxHealth];
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //void SetheartContainers()
    //{
    //    for (int i = 0; i < heartContainers.Length; i++)
    //    {
    //        if (i <PlayerController.Instance.maxHealth)
    //        {
    //            heartContainers[i].SetActive(true);
    //        }
    //        else
    //        {
    //            heartContainers[i].SetActive(false);
    //        }
    //    }
    //}
    //void SetFilledHeart()
    //{
    //    for (int i = 0; i < heartFills.Length; i++)
    //    {
    //        if (i < PlayerController.Instance.maxHealth)
    //        {
    //            heartFills[i].fillAmount = 1;
    //        }
    //        else
    //        {
    //            heartFills[i].fillAmount = 0;
    //        }
    //    }
    //}
    //void InstantiateContainers()
    //{
    //    for (int i = 0;i < PlayerController.Instance.maxHealth; i++)
    //    {
    //        GameObject temp = Instantiate(heartContainerPrefab);
    //        temp.transform.SetParent(heartsParent, false);
    //        heartContainers[i] = temp;
    //        heartFills[i] = temp.transform.Find("HeartFill")
    //    }
    //}
}
