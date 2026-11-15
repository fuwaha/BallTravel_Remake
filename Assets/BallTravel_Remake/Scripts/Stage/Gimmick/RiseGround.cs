using UnityEngine;
using DG.Tweening;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    //チェックポイントに触れたら、地面の下からせり上がってくる足場
    //足場は「せり上がり終わった位置」に置いておくこと。開始時に_riseHeightぶん下に沈めて隠す
    //せり上がっている間はコライダーを切っておき、終わったら有効にする
    //一度せり上がったら、リトライしてもそのまま残る
    public class RiseGround : MonoBehaviour
    {
        [Tooltip("触れたら足場がせり上がるチェックポイント")]
        [SerializeField] private CheckPoint _checkPoint;
        [Tooltip("どれだけ下からせり上がるか。足場が地面に完全に隠れる高さにする")]
        [SerializeField] private float _riseHeight = 5f;
        [Tooltip("せり上がるのにかかる秒数")]
        [SerializeField] private float _riseDuration = 1.5f;
        [Tooltip("せり上がりの動き方")]
        [SerializeField] private Ease _riseEase = Ease.OutCubic;

        private Collider[] _colliders;
        private Vector3 _risenPosition;
        private Tween _riseAnim;

        void Awake()
        {
            //置いた位置をせり上がり後の位置として覚えて、下に沈めておく
            _risenPosition = transform.position;
            transform.position = _risenPosition + Vector3.down * _riseHeight;

            //子オブジェクトのコライダーも含めて、せり上がるまでは切っておく
            _colliders = GetComponentsInChildren<Collider>();
            SetCollidersEnabled(false);
        }

        private void Start()
        {
            GameManager.Instance.OnPausing += RiseStop;
            GameManager.Instance.OnExitPause += RiseRestart;

            if (_checkPoint == null)
            {
                Debug.LogWarning($"{name}: チェックポイントが設定されてないから、せり上がらないよ");
                return;
            }
            _checkPoint.OnPassed += Rise;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= RiseStop;
            GameManager.Instance.OnExitPause -= RiseRestart;

            //CheckPointは触れたときに自分を消すので、残っているときだけ解除する
            if (_checkPoint != null) _checkPoint.OnPassed -= Rise;

            _riseAnim?.Kill();
        }

        //チェックポイントに触れたときに呼ばれる
        private void Rise()
        {
            //二回目以降はせり上がらない
            if (_riseAnim != null) return;

            _riseAnim = transform.DOMove(_risenPosition, _riseDuration)
                .SetEase(_riseEase)
                .OnComplete(FinishRise);
        }

        //せり上がり終わったらコライダーを有効にして、乗れるようにする
        private void FinishRise()
        {
            SetCollidersEnabled(true);
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (var col in _colliders)
            {
                col.enabled = isEnabled;
            }
        }

        private void RiseStop()
        {
            //せり上がり中だけ止める(終わったTweenは消えているので触らない)
            if (_riseAnim == null || !_riseAnim.IsActive()) return;
            _riseAnim.Pause();
        }

        private void RiseRestart()
        {
            if (_riseAnim == null || !_riseAnim.IsActive()) return;
            _riseAnim.Play();
        }
    }
}
