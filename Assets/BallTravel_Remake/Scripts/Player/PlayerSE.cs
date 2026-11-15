using System.Collections.Generic;
using UnityEngine;
namespace BallTravel_Remake.Scripts.Player
{
    public class PlayerSE : MonoBehaviour
    {
        //鳴らせる効果音の種類。増やすときはここに足して、Inspectorの_seListにクリップを登録する
        public enum SEType
        {
            Walk,
            Jump,
            Damage,
            Heal,
        }

        //Inspectorで効果音の種類とクリップをセットで登録するためのデータ
        [System.Serializable]
        private class SEData
        {
            [SerializeField] private SEType _type;
            [SerializeField] private AudioClip _clip;
            public SEType Type => _type;
            public AudioClip Clip => _clip;
        }

        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private Health _health;
        [Tooltip("歩きなどのループ音用")]
        [SerializeField] private AudioSource _loopAudioSource;
        [Tooltip("ジャンプなどの一回だけ鳴らす音用")]
        [SerializeField] private AudioSource _oneShotAudioSource;
        [SerializeField] private List<SEData> _seList;

        private Dictionary<SEType, AudioClip> _seTable;
        private bool _isMoving;
        private bool _isWalkSEPlaying = false;

        void Awake()
        {
            _seTable = new Dictionary<SEType, AudioClip>();
            foreach (SEData se in _seList)
                _seTable[se.Type] = se.Clip;
        }

        void Start()
        {
            _health.OnDamaged += PlayDamageSE;
            _health.OnHealed += PlayHealSE;
        }

        void OnDestroy()
        {
            _health.OnDamaged -= PlayDamageSE;
            _health.OnHealed -= PlayHealSE;
        }
        private void PlayDamageSE() { PlaySE(SEType.Damage); }
        private void PlayHealSE() { PlaySE(SEType.Heal); }

        void Update()
        {
            _isMoving = _playerInput.IsMoving;

            if (_isMoving)
            {
                WalkSEPlay();
                return;
            }

            if (_isWalkSEPlaying)
            {
                _loopAudioSource.Stop();
                _isWalkSEPlaying = false;
            }
        }

        //指定した効果音を一回だけ鳴らす(他のスクリプトから呼ぶ)
        public void PlaySE(SEType type)
        {
            if (!TryGetClip(type, out AudioClip clip)) return;
            _oneShotAudioSource.PlayOneShot(clip);
        }

        private bool TryGetClip(SEType type, out AudioClip clip)
        {
            if (_seTable.TryGetValue(type, out clip) && clip != null) return true;

            Debug.LogWarning($"PlayerSE: {type} のクリップが設定されていない");
            return false;
        }

        private void WalkSEPlay()
        {
            if (_isWalkSEPlaying) return;
            if (!TryGetClip(SEType.Walk, out AudioClip clip)) return;

            _loopAudioSource.clip = clip;
            _loopAudioSource.Play();
            _isWalkSEPlaying = true;
        }
    }
}
