using UnityEngine;


namespace BallTravel_Remake.Scripts.Managers
{
    public interface IStageManager
    {
        public void Initialize();
        public void StageUpdate();
        public void End();
    }
}
