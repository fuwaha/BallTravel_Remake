using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BallTravel_Remake.Scripts.Player;

namespace BallTravel_Remake.Scripts.UI
{
    public class HealthUI : MonoBehaviour, IPlayerUIPart
    {
        private Health _health;
        private Image _healthBar;
        private TMP_Text _healthText;

        public void Bind(PlayerUI playerUI)
        {
            _health = playerUI.Health;
            _healthBar = GetComponent<Image>();
            _healthText = GetComponentInChildren<TMP_Text>(true);

            _healthBar.fillAmount = 1f;
            _healthText.text = _health.MaxHealth.ToString("F0");
            _health.OnHealthChanged += HealthUIUpdate;
        }

        void OnDestroy()
        {
            if (_health == null) return;

            _health.OnHealthChanged -= HealthUIUpdate;
        }

        private void HealthUIUpdate(float currentHealth)
        {
            _healthBar.fillAmount = currentHealth / _health.MaxHealth;
            _healthText.text = currentHealth.ToString("F0");
        }
    }
}
