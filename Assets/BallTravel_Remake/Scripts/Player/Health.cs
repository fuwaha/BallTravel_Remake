using UnityEngine;
using System;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Player
{
    public class Health : MonoBehaviour
    {
        private float _currentHealth = 0f;
        private bool IsDamaged = false;
        //チェックポイントに触れた時点の体力(リトライ時にここへ戻す)
        private float _savedHealth = 0f;

        //体力が変化したときに発行するイベント
        public event Action<float> OnHealthChanged;
        //ダメージを受けると発行するイベント
        public event Action OnDamaged;
        //回復したときに発行するイベント
        public event Action OnHealed;

        [SerializeField] private float _maxHealth = 100;
        [Tooltip("一秒ごとに与えるダメージ")]
        [SerializeField] private float _damagePerSecond = 2f;
        public float MaxHealth => _maxHealth;

        public event Action OnDead;
        public bool IsDead { get; private set; } = false;

        void Start()
        {
            _currentHealth = _maxHealth;
            //チェックポイント前に死んだときは満タンで復活する
            _savedHealth = _maxHealth;

            OnDead += GameManager.Instance.FailGame;
            GameManager.Instance.OnRespawn += ResetHealth;
            GameManager.Instance.OnCheckPointReached += SaveHealth;
        }

        void OnDestroy()
        {
            OnDead -= GameManager.Instance.FailGame;
            GameManager.Instance.OnRespawn -= ResetHealth;
            GameManager.Instance.OnCheckPointReached -= SaveHealth;
        }

        //チェックポイントに触れたとき: 継続ダメージを止めて、今の体力を保存する(回復はしない)
        private void SaveHealth()
        {
            StopDotDamage();
            _savedHealth = _currentHealth;
        }

        //リトライ時: チェックポイント時点の体力に戻して、死亡状態と継続ダメージを解除する
        private void ResetHealth()
        {
            IsDamaged = false;
            IsDead = false;
            _currentHealth = _savedHealth;
            OnHealthChanged?.Invoke(_currentHealth);
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth);

            
            //0になった瞬間に一度だけイベントを発行する
            if (_currentHealth <= 0 && !IsDead)
            {
                IsDead = true;
                OnDead?.Invoke();
            }
        }

        public void Heal(float healValue)
        {
            _currentHealth += healValue;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            StopDotDamage();

            OnHealthChanged?.Invoke(_currentHealth);
            OnHealed?.Invoke();
        }
        
        //徐々にダメージを与える
        private void DotDamage() { TakeDamage(_damagePerSecond * Time.deltaTime); }

        public void DotTrigger()
        {
            if (IsDamaged) return;
            
            OnDamaged?.Invoke();
            IsDamaged = true; 
        }

        private void StopDotDamage()
        {
            if (!IsDamaged) return;
            IsDamaged = false;
        }
        void Update()
        {
            if (!IsDamaged) return;
            if (GameManager.Instance.IsPause) return;
            //クリア後は体力を減らさない
            if (GameManager.Instance.IsClear) return;

            DotDamage();
        }
    }
}
