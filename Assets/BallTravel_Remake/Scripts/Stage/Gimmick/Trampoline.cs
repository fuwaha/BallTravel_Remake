using UnityEngine;
using DG.Tweening;
using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class Trampoline : MonoBehaviour
    {
        [Tooltip("跳ねる強さ(質量に関係なく、この速度で上に跳ぶ)")]
        [SerializeField] private float _bounceForce = 12f;

        [Header("縮むアニメーション")]
        [Tooltip("縮ませるオブジェクト。空ならこのオブジェクト自身")]
        [SerializeField] private Transform _squashTarget;
        [Tooltip("どれくらい縮むか(0.3なら縦が30%縮んで、横が30%広がる)")]
        [SerializeField] private float _squashAmount = 0.3f;
        [Tooltip("縮むまでの時間")]
        [SerializeField] private float _squashDuration = 0.08f;
        [Tooltip("元の大きさに戻るまでの時間")]
        [SerializeField] private float _returnDuration = 0.4f;

        private Vector3 _defaultScale;
        private Sequence _squashAnim;

        void Awake()
        {
            if (_squashTarget == null)
                _squashTarget = transform;

            _defaultScale = _squashTarget.localScale;
        }

        private void Start()
        {
            GameManager.Instance.OnPausing += SquashStop;
            GameManager.Instance.OnExitPause += SquashRestart;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= SquashStop;
            GameManager.Instance.OnExitPause -= SquashRestart;
            _squashAnim?.Kill();
        }

        private void SquashRestart()
        {
            _squashAnim?.Play();
        }

        private void SquashStop()
        {
            _squashAnim?.Pause();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerControl>(out var player)) return;

            Bounce(collision.rigidbody);
            PlaySquash();
        }

        private void Bounce(Rigidbody rb)
        {
            //落ちてきた速度に関係なく毎回同じ高さになるように、縦の速度を0にしてから跳ばす
            Vector3 velocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);

            rb.AddForce(Vector3.up * _bounceForce, ForceMode.VelocityChange);
        }

        private void PlaySquash()
        {
            //アニメ中にもう一度踏まれたら、元の大きさから最初からやり直す
            _squashAnim?.Kill();
            _squashTarget.localScale = _defaultScale;

            Vector3 squashScale = new Vector3(
                _defaultScale.x * (1f + _squashAmount),
                _defaultScale.y * (1f - _squashAmount),
                _defaultScale.z * (1f + _squashAmount));

            //縦に縮む → ぽよんと戻る
            _squashAnim = DOTween.Sequence()
                .Append(_squashTarget.DOScale(squashScale, _squashDuration).SetEase(Ease.OutQuad))
                .Append(_squashTarget.DOScale(_defaultScale, _returnDuration).SetEase(Ease.OutElastic))
                .SetLink(gameObject);
        }
    }
}
