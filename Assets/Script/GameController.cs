using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Script
{
    public class GameController : MonoBehaviour
    {
        public GameObject MainMenuPanel;
        public GameObject playButton;
        public GameObject quitButton;
        public GameObject player;
        public GameObject GameOverGO;
        public GameObject scoreUITextGO;
        public GameObject FinalScoreGO;

        public GameObject FinalScoreText;
        public GameObject VictoryGO;
        public static int totalScore = 0;
        public Button nextLevelButton;
        public GameObject levelSelectionPanel;


        public enum GameManagerState
        {
            GamePlay,
            GameOver,
            Victory,
            Opening,
            LevelSelection
        }
        GameManagerState state;
        // Start is called before the first frame update
        void Start()
        {
            state = GameManagerState.Opening;
            StartGameplay();

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                nextLevelButton.gameObject.SetActive(false);
            }
        }

        void UpdateGameManagerState()
        {
            switch (state)
            {
                case GameManagerState.GamePlay:
                    playButton.SetActive(false);
                    quitButton.SetActive(false);
                    levelSelectionPanel.SetActive(false);
                    GameOverGO.SetActive(false);
                    FinalScoreText.SetActive(false);

                    // Reset điểm số và số lượng đồng xu khi bắt đầu chơi
                    scoreUITextGO.GetComponent<GameScore>().Score = 0;
                    VictoryGO.SetActive(false);
                    break;

                case GameManagerState.GameOver:
                    playButton.SetActive(true);
                    quitButton.SetActive(true);
                    GameOverGO.SetActive(true);
                    Debug.Log("GameOverGO Active State: " + GameOverGO.activeSelf);
                    break;

                case GameManagerState.Victory:
                    if (!VictoryGO.activeSelf)
                    {
                        // Cập nhật tổng điểm và số lượng đồng xu
                        totalScore += scoreUITextGO.GetComponent<GameScore>().Score;
                    }

                    VictoryGO.SetActive(true);
                    FinalScoreText.SetActive(true);
                    FinalScoreGO.GetComponent<TextMeshProUGUI>().text = totalScore.ToString();
                    break;

                case GameManagerState.Opening:
                    scoreUITextGO.GetComponent<GameScore>().Score = 0;


                    GameOverGO.SetActive(false);
                    playButton.SetActive(true);
                    quitButton.SetActive(true);
                    break;

                case GameManagerState.LevelSelection:
                    levelSelectionPanel.SetActive(true);
                    break;
            }
        }


        public void SetGameManagerState(GameManagerState states)
        {
            state = states;
            UpdateGameManagerState();
        }

        public void StartGameplay()
        {
            state = GameManagerState.GamePlay;

            UpdateGameManagerState();
        }


        public void ShowLevelSelection()
        {
            state = GameManagerState.LevelSelection;
            UpdateGameManagerState();
        }

        public void LoadLevel(int levelIndex)
        {
            SceneManager.LoadScene(levelIndex);
        }

        public void StartGame()
        {
            ShowLevelSelection();
        }

        public void StartGameReplay()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }

        public void ExitGame()
        {
            SceneManager.LoadSceneAsync(0);
            Debug.Log("Quit success");
        }

        public void GoToNextScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more levels. Victory!");
            }
        }

        public void BackMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void IsShowMainMenu(bool show)
        {
            MainMenuPanel.SetActive(show);
        }
    }
}