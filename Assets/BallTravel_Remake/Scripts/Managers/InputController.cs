using UnityEngine.InputSystem;


namespace BallTravel_Remake.Scripts.Managers
{
    public class InputController
    {
        public InputSystem_Actions Actions { get; private set; }
        private InputSystem_Actions.UIActions _uiAction;

        void Awake()
        {
            Actions = new InputSystem_Actions();
            _uiAction = Actions.UI;
            _uiAction.Enable();
        }

        void OnDestroy()
        {
            _uiAction.Disable();
        }

        public bool GetIsGamePadUsed() => _uiAction.Navigate.activeControl?.device is Gamepad;
        public void UIActionEnable() => _uiAction.Enable();
        public void UIActionDisable() => _uiAction.Disable();
    }
}
