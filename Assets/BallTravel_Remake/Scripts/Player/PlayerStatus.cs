using System;
using Unity.Mathematics;
using UnityEngine;

namespace BallTravel_Remake.Scripts.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        [SerializeField] private int _hp;
        [SerializeField] private int _maxHp;

        [SerializeField] private float _maxStrength;
        [SerializeField] private float _drainStrength;
        [SerializeField] private float _recoverStrength;
        [SerializeField] private float _strength;
        public bool IsExhausted => _strength <= 0;
        public event Action<float> OnStrengthChanged;

        void Start()
        {
            _strength = _maxStrength;
        }

        public void Running()
        {
            _strength = math.clamp(_strength, 0, _maxStrength);
            _strength -= _drainStrength * Time.deltaTime;
            OnStrengthChanged?.Invoke(_strength);
        }

        public void Recover()
        {
            if (_strength < _maxStrength)
            {
                _strength = math.clamp(_strength, 0, _maxStrength);
                _strength += _recoverStrength * Time.deltaTime;
                OnStrengthChanged?.Invoke(_strength);
                return;
            }

            _strength = (int)_strength;
        }
    }
}
