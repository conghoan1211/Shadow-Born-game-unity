using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace Assets.Script
{
    public class GameControllerLV3 : MonoBehaviour
    {
        public GameObject playButton;
        public GameObject quitButton;
        public GameObject player;
        public GameObject GameOverGO;
        public GameObject scoreUIText;
        public TextMeshProUGUI FinalScoreText;
        public GameObject VictoryGO;
        private TextMeshProUGUI TimeDisplayText; 
        public static int totalScore = 0;
        public Button nextLevelButton;

        private float startTime;
        private float completionTime;
        private bool isGameActive = false;
        private const int MAX_SCORE = 80;
        private const int MIN_SCORE = 50;
        private const int SCORE_PENALTY = 10;
        private const float PENALTY_INTERVAL = 10f;
        private float bossDefeatedTime = 0f;

        public enum GameManagerState
        {
            GamePlay,
            GameOver,
            Victory,
            Opening,
            LevelSelection
        }
        GameManagerState state;

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

        void Update()
        {
            if (isGameActive && TimeDisplayText != null)  // Changed from TimeDisplayGO
            {
                float currentTime = Time.time - startTime;
                TimeDisplayText.GetComponent<TextMeshProUGUI>().text = $"Time: {currentTime:F2}s";
            }
        }

        public void BossDefeated(float defeatedTime)
        {
            bossDefeatedTime = defeatedTime;
            SetGameManagerState(GameManagerState.Victory);
        }

        void UpdateGameManagerState()
        {
            switch (state)
            {
                case GameManagerState.GamePlay:
                    playButton.SetActive(false);
                    quitButton.SetActive(false);
                    GameOverGO.SetActive(false);
                    scoreUIText.GetComponent<GameScore>().Score = 0;
                    VictoryGO.SetActive(false);

                    // Start timing when gameplay begins
                    startTime = Time.time;
                    isGameActive = true;
                    break;

                case GameManagerState.GameOver:
                    playButton.SetActive(true);
                    quitButton.SetActive(true);
                    GameOverGO.SetActive(true);
                    isGameActive = false;
                    Debug.Log("GameOverGO Active State: " + GameOverGO.activeSelf);
                    break;

                case GameManagerState.Victory:
                    if (!VictoryGO.activeSelf)
                    {
                        isGameActive = false;

                        // Calculate score based on time
                        int timeBasedScore = CalculateTimeBasedScore(bossDefeatedTime);
                        totalScore += timeBasedScore;

                        // Update the final score and time display
                        if (FinalScoreText != null)
                        {
                            TextMeshProUGUI finalScoreTextComponent = FinalScoreText.GetComponent<TextMeshProUGUI>();
                            if (finalScoreTextComponent != null)
                            {
                                string completionTimeText = $"Completion Time: {bossDefeatedTime:F2}s\n";
                                string scoreText = $"Level Score: {timeBasedScore}\nTotal Score: {totalScore}";
                                finalScoreTextComponent.text = completionTimeText + scoreText;
                            }
                        }
                    }

                    VictoryGO.SetActive(true);
                    break;

                case GameManagerState.Opening:
                    scoreUIText.GetComponent<GameScore>().Score = 0;
                    GameOverGO.SetActive(false);
                    playButton.SetActive(true);
                    quitButton.SetActive(true);
                    isGameActive = false;
                    break;

            }
        }

        private int CalculateTimeBasedScore(float time)
        {
            // Calculate how many PENALTY_INTERVAL periods have passed
            int penaltyPeriods = Mathf.FloorToInt(time / PENALTY_INTERVAL);

            // Calculate score reduction (10 points per 10 seconds)
            int scoreReduction = penaltyPeriods * SCORE_PENALTY;

            // Calculate final score (starting from MAX_SCORE)
            int finalScore = MAX_SCORE - scoreReduction;

            // Ensure score doesn't go below MIN_SCORE
            return Mathf.Max(MIN_SCORE, finalScore);
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
    }
}