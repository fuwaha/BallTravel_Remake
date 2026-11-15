using UnityEngine;
using DG.Tweening;
using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class RotateGround : MonoBehaviour
    {
        [SerializeField] private float _rotateSpeed = 10f;
        [Tooltip("縦方向に回転するかどうか、しないなら横回転")]
        [SerializeField] private bool _isRotateDirY = false;

        private Tween _rotateAnim;
        void Awake()
        {

            //回転の補正
            var rotate = _isRotateDirY ? new Vector3(360, 0, 0) : new Vector3(0, 360, 0);
            transform.rotation = Quaternion.identity;

            //DOTweenで回転
            _rotateAnim =
            transform.DOLocalRotate(rotate, _rotateSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void Start()
        {
            GameManager.Instance.OnPausing += RotateStop;
            GameManager.Instance.OnExitPause += RotateRestart;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= RotateStop;
            GameManager.Instance.OnExitPause -= RotateRestart;
        }

        private void RotateRestart()
        {
            _rotateAnim.Play();
        }

        private void RotateStop()
        {
            _rotateAnim.Pause();
        }

        private void OnCollisionStay(Collision collision)
        {
            //足場と等速でプレイヤーを回すことで、滑り落ちないようにする
            //縦回転の場合は処理しない

            if (_isRotateDirY) return;

            if (collision.gameObject.TryGetComponent<PlayerControl>(out var player))
            {
                //中心を設定してオブジェクトを回すメソッド
                collision.transform.RotateAround
                (
                    transform.position,
                    Vector3.up,
                    360 / _rotateSpeed * Time.deltaTime
                );
            }
        }
    }
}
