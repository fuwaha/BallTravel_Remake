using UnityEngine;
using TMPro;
using System;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.UI
{
    public class TimeText : MonoBehaviour
    {
        private TMP_Text _text;
        private float _timer = 0f;

        void Start()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = "Time: 0.00s";

            GameManager.Instance.ResetTimer();
            GameManager.Instance.StartTimer();

        }

        private string ParseTime(float timer)
        {
            int hh = (int)(timer / 3600f);
            int mm = (int)(timer / 60f) % 60;
            float ss = timer % 60;

            return $"{hh:D2}:{mm:D2}:{ss:00.00}";
        }

        void Update()
        {
            _text.text =
            ParseTime(GameManager.Instance.Timer);
        }
    }
}
