using UnityEngine;
using DG.Tweening;
using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;
using System.Collections.Generic;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveGround : MonoBehaviour
    {
        private enum PathMode
        {
            [Tooltip("最後の地点から最初の地点に戻ってぐるっと回る")]
            Loop,
            [Tooltip("最後の地点まで行ったら来た道を引き返す")]
            PingPong,
        }

        [Tooltip("通る地点(空オブジェクト)。上から順に通る。一番上がスタート地点")]
        [SerializeField] private Transform[] _points;
        [Tooltip("Loop: ぐるっと周回 / PingPong: 往復")]
        [SerializeField] private PathMode _pathMode = PathMode.Loop;
        [Tooltip("地点から次の地点まで何秒で動くか")]
        [SerializeField] private float _moveDuration = 3f;
        [Tooltip("各地点で止まる時間。0なら止まらない")]
        [SerializeField] private float _waitDuration = 0f;
        [Tooltip("ボールが一瞬浮いても「乗っている」扱いを続ける秒数")]
        [SerializeField] private float _rideGraceTime = 0.1f;

        private Rigidbody _rb;
        private Vector3[] _pointPositions;
        private Vector3 _pathPosition;
        private Sequence _moveAnim;

        //今乗っているボール。乗っていなければnull
        private Rigidbody _ridingRb;
        //「乗っている」扱いの残り時間
        private float _rideTimer;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            //位置を直接書き換える(ワープ扱い)ので補間はしない
            _rb.interpolation = RigidbodyInterpolation.None;

            if (_points == null || _points.Length < 2)
            {
                Debug.LogWarning($"{name}: 地点が2つ以上ないので動かないよ");
                return;
            }

            //地点を足場の子にしても一緒に動かないように、最初に位置だけ覚えておく
            _pointPositions = new Vector3[_points.Length];
            for (int i = 0; i < _points.Length; i++)
            {
                _pointPositions[i] = _points[i].position;
            }

            //スタート地点に置く
            _pathPosition = _pointPositions[0];
            transform.position = _pathPosition;

            _moveAnim = CreateMoveAnim();
        }

        private void Start()
        {
            GameManager.Instance.OnPausing += MoveStop;
            GameManager.Instance.OnExitPause += MoveRestart;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= MoveStop;
            GameManager.Instance.OnExitPause -= MoveRestart;
            _moveAnim?.Kill();
        }

        private void MoveRestart()
        {
            _moveAnim?.Play();
        }

        private void MoveStop()
        {
            _moveAnim?.Pause();
        }

        private Sequence CreateMoveAnim()
        {
            //通る順番を作る(どっちのモードも最後は0番に戻るので、そのままループできる)
            //Loop:     0 → 1 → 2 → 0
            //PingPong: 0 → 1 → 2 → 1 → 0
            var order = new List<int>();
            for (int i = 1; i < _pointPositions.Length; i++)
            {
                order.Add(i);
            }

            if (_pathMode == PathMode.Loop)
            {
                order.Add(0);
            }
            else
            {
                for (int i = _pointPositions.Length - 2; i >= 0; i--)
                {
                    order.Add(i);
                }
            }

            var sequence = DOTween.Sequence();
            foreach (int index in order)
            {
                //足場は物理で動かしたいので、位置の値だけDOTweenで動かしてFixedUpdateで反映する
                sequence.Append(
                    DOTween.To(GetPathPosition, SetPathPosition, _pointPositions[index], _moveDuration)
                        .SetEase(Ease.InOutSine));

                if (_waitDuration > 0f)
                    sequence.AppendInterval(_waitDuration);
            }

            return sequence
                .SetUpdate(UpdateType.Fixed)
                .SetLoops(-1)
                .SetLink(gameObject);
        }

        private Vector3 GetPathPosition()
        {
            return _pathPosition;
        }

        private void SetPathPosition(Vector3 position)
        {
            _pathPosition = position;
        }

        private void FixedUpdate()
        {
            if (_moveAnim == null) return;

            Vector3 delta = _pathPosition - _rb.position;

            //MovePositionだと物理的に「動いている床」になって摩擦でボールを引っ張ってしまうので、
            //位置を直接書き換えて床の速度を0のままにする
            _rb.position = _pathPosition;

            if (_ridingRb == null) return;

            //乗っているボールも同じだけ動かして、置いていかれないようにする
            //ゲームオーバー中などでボールが止まっているときは動かさない
            if (!_ridingRb.isKinematic)
            {
                _ridingRb.MovePosition(_ridingRb.position + delta);
            }

            //しばらく触れていなければ降りたことにする
            _rideTimer -= Time.fixedDeltaTime;
            if (_rideTimer <= 0f)
                _ridingRb = null;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerControl>(out var player)) return;

            //横からぶつかったときは運ばない。ボールの中心より下で触れているときだけ「乗っている」
            bool isOnTop = collision.GetContact(0).point.y < collision.transform.position.y;
            if (!isOnTop) return;

            _ridingRb = collision.rigidbody;
            _rideTimer = _rideGraceTime;
        }
    }
}
