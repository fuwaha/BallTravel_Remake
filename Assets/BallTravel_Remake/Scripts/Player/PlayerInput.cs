using UnityEngine;
using UnityEngine.InputSystem;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Player
{
    public class PlayerInput : MonoBehaviour
    {
        private InputSystem_Actions _actions;
        private InputSystem_Actions.PlayerActions _playerInputAction;
        private InputSystem_Actions.GlobalActions _globalAction;

        /// <summary>
        ///     マウスを使用しているならtrue
        ///     移動量がマウス由来の際はdeltaTimeを乗算しないようにするために使用
        /// </summary>
        public bool IsUsingMouse => _playerInputAction.Look.activeControl?.device is Mouse;

        public Vector2 LookRotation { get; set; }
        public Vector2 MoveDirection { get; private set; }
        public bool IsMoving => MoveDirection.magnitude > 0;
        public bool IsPausePressed => _globalAction.Pause.triggered; 
        public bool IsJumpPressed => _playerInputAction.Jump.IsPressed(); 
        public bool IsInteractPressed => _playerInputAction.Interact.triggered;
        public bool IsSprintPressed => _playerInputAction.Sprint.IsPressed();
        public bool IsHangInPressed => _playerInputAction.HangIn.IsPressed();

        private void Start()
        {
            _actions = new InputSystem_Actions();
            _playerInputAction = _actions.Player;
            _globalAction = _actions.Global;

            _actions.Enable();

            GameManager.Instance.OnPausing += DisablePlayerInput;
            GameManager.Instance.OnExitPause += EnablePlayerInput;
            GameManager.Instance.OnGameOver += DisablePlayerInput;
            GameManager.Instance.OnGameClear += DisablePlayerInput;
            GameManager.Instance.OnRespawn += EnablePlayerInput;
        }

        private void DisablePlayerInput() { _playerInputAction.Disable(); }
        private void EnablePlayerInput() { _playerInputAction.Enable(); }

        void OnDestroy()
        {
            GameManager.Instance.OnPausing -= DisablePlayerInput;
            GameManager.Instance.OnExitPause -= EnablePlayerInput;
            GameManager.Instance.OnGameOver -= DisablePlayerInput;
            GameManager.Instance.OnGameClear -= DisablePlayerInput;
            GameManager.Instance.OnRespawn -= EnablePlayerInput;

            _actions?.Dispose();
        }

        //InGameが終了したとき(ゲームオーバー、クリア状態)のインプット制御

        private void Update()
        {
            MoveDirection = _playerInputAction.Move.ReadValue<Vector2>();
            LookRotation = _playerInputAction.Look.ReadValue<Vector2>();
        }
    }
}
