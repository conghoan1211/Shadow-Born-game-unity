using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    public class GroundWater : MonoBehaviour
    {
        public GameObject GameManagerGO;
        public GameObject playerGO;
        public AudioClip looseSound;
        public GameObject crashEffect;
        public GameObject gameOver;
        public GameObject playButton;
        public GameObject quitButton;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                Debug.Log("Player has collided with the ground");

                var player = collision.gameObject.transform;
                var crash = Instantiate(crashEffect, player.position, player.rotation);
                crash.GetComponent<ParticleSystem>().Play();
                playerGO.SetActive(false);
                gameOver.SetActive(true);
                playButton.SetActive(true);
                quitButton.SetActive(true);
                AudioSource.PlayClipAtPoint(looseSound, transform.position);

                // Check if GameManagerGO is not null
                if (GameManagerGO != null)
                {
                    Debug.Log("Changing Game State to GameOver");
                    GameManagerGO.GetComponent<GameController>().SetGameManagerState(GameController.GameManagerState.GameOver);
                }
                else
                {
                    Debug.LogError("GameManagerGO is null");
                }
            }
        }
    }
    }
