using UnityEngine;
using UnityEngine.UI;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Tooltip("ポーズ中に表示するパネル(このスクリプトを付けたオブジェクトとは別のオブジェクトにすること)")]
        [SerializeField] private GameObject _pausePanel;
        [Tooltip("ポーズを解除してゲームに戻るボタン")]
        [SerializeField] private Button _resumeButton;
        [Tooltip("タイトルに戻るボタン")]
        [SerializeField] private Button _titleButton;

        void Awake()
        {
            //ボタンはこのUIと一緒に破棄されるため、解除の処理は不要
            _resumeButton.onClick.AddListener(OnClickResume);
            _titleButton.onClick.AddListener(OnClickReturnTitle);

            _pausePanel.SetActive(false);
        }

        void Start()
        {
            GameManager.Instance.OnPausing += ShowPause;
            GameManager.Instance.OnExitPause += HidePause;
        }

        void OnDestroy()
        {
            GameManager.Instance.OnPausing -= ShowPause;
            GameManager.Instance.OnExitPause -= HidePause;
        }

        //ポーズに入ったときにパネルを表示する(キー、UIどちらからのポーズでも呼ばれる)
        private void ShowPause()
        {
            _pausePanel.SetActive(true);
        }

        //ポーズを解除したときにパネルを非表示にする
        private void HidePause()
        {
            _pausePanel.SetActive(false);
        }

        //ゲームに戻るボタン
        private void OnClickResume()
        {
            GameManager.Instance.SwitchPausingFromUI();
        }

        //タイトルに戻るボタン
        private void OnClickReturnTitle()
        {
            //連打で何度も呼ばれないようにする
            _resumeButton.interactable = false;
            _titleButton.interactable = false;

            GameManager.Instance.LoadToTitle();
        }
    }
}
