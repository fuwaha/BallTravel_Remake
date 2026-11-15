using UnityEngine;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.UI
{
    public class TitleUI : MonoBehaviour
    {
        //ここにボタンUIの処理を記述
        public void OnClickStage1()
        {
            GameManager.Instance.ChangeSceneToStage1();
        }

        public void OnClickStage2()
        {
            GameManager.Instance.ChangeSceneToStage2();
        }

        public void OnClickStage3()
        {
            GameManager.Instance.ChangeSceneToStage3();
        }

        //ここに画面切り替え用の処理を記述
        private void ChangeFlipControlInfo()
        {
        }
        private void ChangeFlipStageSelect()
        {
        }
        private void ChangeFlipOption()
        {
        }
    }
}
