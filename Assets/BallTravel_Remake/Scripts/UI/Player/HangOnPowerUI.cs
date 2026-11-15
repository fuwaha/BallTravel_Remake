using BallTravel_Remake.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

namespace BallTravel_Remake.Scripts.UI
{
    public class HangOnPowerUI : MonoBehaviour,IPlayerUIPart
    {
        private PlayerControl _playerControl;
        private Image _image;

        public void Bind(PlayerUI playerUI)
        {
            _image = GetComponent<Image>();
            _image.fillAmount = 0;

            _playerControl = playerUI.PlayerControl;
            _playerControl.OnJumpForceChanged += UpdateUI;
            _playerControl.OnJumpForceReset += ResetUI;
        }

        void OnDestroy()
        {
            if (_playerControl == null) return;

            _playerControl.OnJumpForceChanged -= UpdateUI;
            _playerControl.OnJumpForceReset -= ResetUI;
        }

        public void UpdateUI(float percent)
        {
            _image.fillAmount = percent;
        }

        public void ResetUI()
        {
            _image.fillAmount = 0f;
        }
    }
}
