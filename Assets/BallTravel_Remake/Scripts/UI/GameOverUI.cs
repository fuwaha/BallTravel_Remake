using UnityEngine;
using UnityEngine.UI;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Tooltip("ゲームオーバー時に表示するパネル(このスクリプトを付けたオブジェクトとは別のオブジェクトにすること)")]
        [SerializeField] private GameObject _gameOverPanel;
        [Tooltip("直前のチェックポイントからやり直すボタン")]
        [SerializeField] private Button _retryButton;
        [Tooltip("タイトルに戻るボタン")]
        [SerializeField] private Button _titleButton;

        void Awake()
        {
            //ボタンはこのUIと一緒に破棄されるため、解除の処理は不要
            _retryButton.onClick.AddListener(OnClickRetry);
            _titleButton.onClick.AddListener(OnClickReturnTitle);

            _gameOverPanel.SetActive(false);
        }

        void Start()
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnRespawn += HideGameOver;
        }

        void OnDestroy()
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnRespawn -= HideGameOver;
        }

        //ゲームオーバーになったときにパネルを表示する
        //リトライ後にもう一度ゲームオーバーになることがあるため、ボタンを押せる状態に戻す
        private void ShowGameOver()
        {
            SetButtonsInteractable(true);
            _gameOverPanel.SetActive(true);
        }

        //リトライしたときにパネルを非表示にする
        private void HideGameOver()
        {
            _gameOverPanel.SetActive(false);
        }

        //リトライボタン
        private void OnClickRetry()
        {
            //連打で何度も呼ばれないようにする
            SetButtonsInteractable(false);

            GameManager.Instance.Retry();
        }

        //タイトルに戻るボタン
        private void OnClickReturnTitle()
        {
            //連打で何度も呼ばれないようにする
            SetButtonsInteractable(false);

            GameManager.Instance.LoadToTitle();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            _retryButton.interactable = interactable;
            _titleButton.interactable = interactable;
        }
    }
}
