using UnityEngine;
using TMPro;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.UI
{
    public class ClearUI : MonoBehaviour
    {
        [Tooltip("クリア時に表示するパネル(このスクリプトを付けたオブジェクトとは別のオブジェクトにすること)")]
        [SerializeField] private GameObject _clearPanel;
        [SerializeField] private TMP_Text _clearTimeText;
        [SerializeField] private TMP_Text _rankText;

        [Header("ランクの基準タイム(秒)")]
        [Tooltip("この時間以内ならSランク")]
        [SerializeField] private float _sRankTime = 60f;
        [Tooltip("この時間以内ならAランク")]
        [SerializeField] private float _aRankTime = 90f;
        [Tooltip("この時間以内ならBランク。これより遅ければCランク")]
        [SerializeField] private float _bRankTime = 120f;

        void Awake()
        {
            _clearPanel.SetActive(false);
        }

        void Start()
        {
            GameManager.Instance.OnGameClear += ShowClear;
        }

        void OnDestroy()
        {
            GameManager.Instance.OnGameClear -= ShowClear;
        }

        //クリア時にクリアタイムとランクを表示する
        private void ShowClear()
        {
            float clearTime = GameManager.Instance.Timer;

            _clearTimeText.text = ParseTime(clearTime);
            _rankText.text = EvaluateRank(clearTime);
            _clearPanel.SetActive(true);
        }

        //クリアタイムからランクを判定する
        private string EvaluateRank(float clearTime)
        {
            if (clearTime <= _sRankTime) return "Rank:S";
            if (clearTime <= _aRankTime) return "Rank:A";
            if (clearTime <= _bRankTime) return "Rank:B";
            return "C";
        }

        private string ParseTime(float time)
        {
            int hh = (int)(time / 3600f);
            int mm = (int)(time / 60f) % 60;
            float ss = time % 60;

            return $"{hh:D2}:{mm:D2}:{ss:00.00}";
        }
    }
}
