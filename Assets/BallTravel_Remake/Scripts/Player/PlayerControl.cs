using System;
using UnityEngine;
using BallTravel_Remake.Scripts.Managers;
using BallTravel_Remake.Scripts.Common;

namespace BallTravel_Remake.Scripts.Player
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] private Camera _playerCamera;
        private Rigidbody _rb;

        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMoveSpeed = 3f;
        [Tooltip("踏ん張り開始してからジャンプ力が最大になるまでの時間")]
        [SerializeField] private float _maxJumpForceSeconds = 1f;
        [Tooltip("最小ジャンプ力")]
        [SerializeField] private float _minJumpForce = 5f;
        [Tooltip("最大ジャンプ力")]
        [SerializeField] private float _maxJumpForce = 8f;
        [Tooltip("入力がない場合、徐々に速度を減衰させる力の大きさ")]
        [SerializeField] private float _delayForce = 10f;
        [SerializeField] private String _groundTag;
        //リスポーンするときの位置と向きを保存しておく変数
        //Transformのまま持つと、初期値がプレイヤー自身を指して一緒に動いてしまうため、値で保存する
        private Vector3 _respawnPosition;
        private Quaternion _respawnRotation;

        private float _addJumpForceFromSeconds = 0f;

        //ポーズに入る直前の速度(解除時に復元する)
        private Vector3 _savedLinearVelocity;
        private Vector3 _savedAngularVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            //最初のチェックポイントを通るまでは、開始位置に戻す
            _respawnPosition = transform.position;
            _respawnRotation = transform.rotation;
            _addJumpForceFromSeconds = (_maxJumpForce - _minJumpForce) / _maxJumpForceSeconds;
        }

        //GameManagerのAwakeより後に参照する必要があるため、購読はStartで行う
        private void Start()
        {
            GameManager.Instance.OnPausing += OnPause;
            GameManager.Instance.OnExitPause += OnResume;
            GameManager.Instance.OnGameOver += OnGameEndStop;
            GameManager.Instance.OnGameClear += OnGameEndStop;
            GameManager.Instance.OnRespawn += Respawn;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= OnPause;
            GameManager.Instance.OnExitPause -= OnResume;
            GameManager.Instance.OnGameOver -= OnGameEndStop;
            GameManager.Instance.OnGameClear -= OnGameEndStop;
            GameManager.Instance.OnRespawn -= Respawn;
        }

        //ゲームオーバー、クリアになったとき: ジャンプのためを捨てて、Rigidbodyを止める
        //リトライで位置ごと戻すため、速度は保存しない(クリア時は再開しない)
        private void OnGameEndStop()
        {
            _currentJumpForce = _minJumpForce;
            _isNormalJumping = true;
            OnJumpForceReset?.Invoke();

            _rb.isKinematic = true;
        }

        //ポーズに入ったとき: ジャンプのためを捨てて、Rigidbodyを止める
        private void OnPause()
        {
            if (GameManager.Instance.IsPause) return;

            //ジャンプのため状態を捨てて、ゲージUIもリセットする
            _currentJumpForce = _minJumpForce;
            _isNormalJumping = true;
            OnJumpForceReset?.Invoke();

            //isKinematicにすると速度が0になるため、先に保存してから止める
            _savedLinearVelocity = _rb.linearVelocity;
            _savedAngularVelocity = _rb.angularVelocity;
            _rb.isKinematic = true;
        }

        //ポーズを解除したとき: 止まる直前の動きを再開する
        private void OnResume()
        {
            if (!GameManager.Instance.IsPause) return;

            //isKinematicを戻してから、保存した速度を入れ直す
            _rb.isKinematic = false;
            _rb.linearVelocity = _savedLinearVelocity;
            _rb.angularVelocity = _savedAngularVelocity;
        }

        /// <summary>
        ///     入力に基づく移動処理
        /// </summary>
        /// <param name="vectorXZ"></param>
        /// 
        public void Move(Vector2 vectorXZ, bool isMoving)
        {
            if (GameManager.Instance.IsPause) return;
            if (GameManager.Instance.IsOver) return;
            if (GameManager.Instance.IsClear) return;

            //入力がない場合は徐々に減速させる
            if (isMoving == false)
            {
                Vector3 velocity = _rb.linearVelocity;

                // XZだけ減速
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    _delayForce * Time.fixedDeltaTime);

                // Yはそのまま
                _rb.linearVelocity = new Vector3(
                    horizontalVelocity.x,
                    velocity.y,
                    horizontalVelocity.z);

                // 回転速度も減衰させる
                _rb.angularVelocity = Vector3.MoveTowards(
                    _rb.angularVelocity,
                    Vector3.zero,
                    _delayForce * Time.fixedDeltaTime);
                return;
            }

            // カメラ方向の移動ベクトル計算
            var moveDirection =
                _playerCamera.transform.right * vectorXZ.x +
                _playerCamera.transform.forward * vectorXZ.y;
            moveDirection.y = 0;
            moveDirection.Normalize();

            _rb.AddForce(moveDirection * _moveSpeed);
        }

        private float _currentJumpForce = 0f;
        private bool _isNormalJumping = true;
        public event Action<float> OnJumpForceChanged;
        public event Action OnJumpForceReset;
        public void Jump(bool isJumpPressed, bool isHangInPressed)
        {
            if (GameManager.Instance.IsPause) return;
            if (GameManager.Instance.IsOver) return;
            if (GameManager.Instance.IsClear) return;

            Physics.Raycast
            (
                transform.position,
                Vector3.down,
                out RaycastHit hitInfo,
                0.2f
            );
            bool isGrounded = false;

            if (hitInfo.collider != null)
                isGrounded = hitInfo.collider.CompareTag(_groundTag);

            if (!isGrounded)
            {
                OnJumpForceReset?.Invoke();
                return;
            }
            if (!isJumpPressed && !isHangInPressed)
            {
                _currentJumpForce = _minJumpForce;
                _isNormalJumping = true;
                return;
            }

            //踏ん張り + ジャンプボタンを押している場合は力をためる
            if (isJumpPressed && isHangInPressed)
            {
                _isNormalJumping = false;
                _currentJumpForce += _addJumpForceFromSeconds * Time.deltaTime;
                _currentJumpForce = Mathf.Clamp(_currentJumpForce, _minJumpForce, _maxJumpForce);
                OnJumpForceChanged?.Invoke(CommonMethod.ParsePercent(_currentJumpForce, _minJumpForce, _maxJumpForce));
                return;
            }
            else if (!isJumpPressed && !_isNormalJumping)
            {
                //ジャンプボタンを離したらジャンプ力を適用してジャンプ
                _rb.AddForce(Vector3.up * _currentJumpForce, ForceMode.Impulse);
                _currentJumpForce = 0f;
                OnJumpForceReset?.Invoke();
                return;
            }
            else if (isJumpPressed && !isHangInPressed && !_isNormalJumping)
            {
                //途中でHangInを離した場合はジャンプ力をリセット
                _currentJumpForce = _minJumpForce;
                OnJumpForceReset?.Invoke();
            }

            //ジャンプボタンのみ押した場合はそのままジャンプ
            if (isJumpPressed && _isNormalJumping)
            {
                _currentJumpForce = _minJumpForce;
                _rb.AddForce(Vector3.up * _currentJumpForce, ForceMode.Impulse);
                _isNormalJumping = false;
            }
        }

        public void SwitchPause(bool pause)
        {
            if (!pause) return;

            GameManager.Instance.SwitchPausingFromKey();
        }

        public void SetNewRespawnTransform(Transform newTransform)
        {
            _respawnPosition = newTransform.position;
            _respawnRotation = newTransform.rotation;
        }
        
        //リトライ時(OnRespawn)に呼ばれる: 止めていたRigidbodyを戻して、保存した位置に移動する
        public void Respawn()
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            transform.position = _respawnPosition;
            transform.rotation = _respawnRotation;
        }
    }
}
