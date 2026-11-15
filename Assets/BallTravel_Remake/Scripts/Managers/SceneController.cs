using UnityEngine.SceneManagement;

namespace BallTravel_Remake.Scripts.Managers
{
    public class SceneController
    {
        public bool IsSceneLoading { get; private set; } = false;

        //シーン指定用のenum
        public enum SceneName
        {
            Title,

            Tutorial,
            Stage1,
            Stage2,
            Stage3,

            Load,//使わないかもだけど一応
        }

        //シーン遷移(nameでシーンを指定)
        public void LoadScene(SceneName name)
        {
            if (IsSceneLoading) return;
            SceneManager.sceneLoaded += CompleteSceneLoad;

            IsSceneLoading = true;

            switch (name)
            {
                case SceneName.Title:
                    SceneManager.LoadScene("Title");
                    break;

                case SceneName.Tutorial:
                    SceneManager.LoadScene("InGame_Tutorial");
                    break;

                case SceneName.Stage1:
                    SceneManager.LoadScene("InGame_Stage1");
                    break;

                case SceneName.Stage2:
                    SceneManager.LoadScene("InGame_Stage2");
                    break;

                case SceneName.Stage3:
                    SceneManager.LoadScene("InGame_Stage3");
                    break;
            }
        }

        //シーン遷移完了時の処理
        private void CompleteSceneLoad(Scene scene, LoadSceneMode mode)
        {
            IsSceneLoading = false;
            SceneManager.sceneLoaded -= CompleteSceneLoad;
        }
    }
}
