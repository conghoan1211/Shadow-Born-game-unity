using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Script
{
    public class _GameController : MonoBehaviour
    {
        public GameObject MainMenuPanel; // add
        public GameObject ItemCoin;
        public GameObject MenuFinishPanel;
        public GameObject GameOverTitle;
        public GameObject GameWinTitle;
        public GameObject ShowMissionPanel;
        public GameObject ShowDialogNPCPanel;

        public GameObject playButton;
        public GameObject quitButton;
        [Space(5)]
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
            LevelSelection,
            ShowMenu,
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
                    MenuFinishPanel.SetActive(false);
                    MainMenuPanel.SetActive(false);
                    playButton.SetActive(true);
                    quitButton.SetActive(true);
                    break;

                case GameManagerState.GameOver:
                    UI.instance.SetScoreFinish();
                    MenuFinishPanel.SetActive(true);
                    ItemCoin.SetActive(false);
                    GameOverTitle.SetActive(true);
                    GameWinTitle.SetActive(false);
                    nextLevelButton.gameObject.SetActive(false);
                    break;

                case GameManagerState.Victory:
                    UI.instance.SetScoreFinish();
                    MenuFinishPanel.SetActive(true);
                    ItemCoin.SetActive(false);
                    GameOverTitle.SetActive(false);
                    GameWinTitle.SetActive(true);
                    nextLevelButton.gameObject.SetActive(true);
                    break;

                case GameManagerState.ShowMenu:
                    MainMenuPanel.SetActive(true);
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

        public void ShowMenuGame()
        {
            state = GameManagerState.ShowMenu;
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

        public void IsShowMissionPanel(bool show)
        {
            ShowMissionPanel.SetActive(show);
        }
        public void IsShowDialogNPCPanel(bool show)
        {
           ShowDialogNPCPanel.SetActive(show);
        }
    }
}