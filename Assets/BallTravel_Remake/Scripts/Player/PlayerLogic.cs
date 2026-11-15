using UnityEngine;
namespace BallTravel_Remake.Scripts.Player
{
    public class PlayerLogic : MonoBehaviour
    {
        [SerializeField] private CameraControl _cameraControl;
        private PlayerControl _player;
        private PlayerInput _playerInput;

        private void Awake()
        {
            _player = GetComponent<PlayerControl>();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            _cameraControl.RotateCamera(_playerInput.LookRotation);
            _player.Jump(_playerInput.IsJumpPressed, _playerInput.IsHangInPressed);
            _player.SwitchPause(_playerInput.IsPausePressed);
        }

        private void FixedUpdate()
        {
            _player.Move
            (
                _playerInput.MoveDirection,
                _playerInput.IsMoving
            );
        }
    }
}
