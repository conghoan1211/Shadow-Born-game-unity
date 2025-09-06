using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Script
{
    public class GameScore : MonoBehaviour
    {
        public TextMeshProUGUI scoreTextUI;
        int score = 0;

        public int Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
                UpdateScoreTextUI(score);
            }
        }

        void Start()
        {
            // Nếu scoreTextUI chưa được gán trong Editor, tìm và gán nó từ GameObject hiện tại
            if (scoreTextUI == null)
            {
                scoreTextUI = GetComponent<TextMeshProUGUI>();
            }
        }

        // Hàm để cập nhật giao diện hiển thị điểm số
        void UpdateScoreTextUI(int scores)
        {
            scoreTextUI.text = scores.ToString();
        }
    }
}
