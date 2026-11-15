using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace BallTravel_Remake.Scripts.UI
{
    public class Fader : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Image _fadeImage;

        public async UniTask StartFade()
        {
            await FadeIn();
            SceneManager.sceneLoaded += FadeOut;
        }

        private async UniTask FadeIn()
        {
            await _fadeImage.DOFade(1, _fadeDuration)
                .AsyncWaitForCompletion();
        }
        private void FadeOut(Scene scene, LoadSceneMode mode)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_fadeImage.DOFade(0, _fadeDuration));

            SceneManager.sceneLoaded -= FadeOut;
        }

        //シーンを読み直さないとき(リトライなど)に使うフェード
        //StartFadeと違い、sceneLoadedでのフェードアウトは予約しない
        public async UniTask FadeInWithoutLoad()
        {
            await FadeIn();
        }

        public async UniTask FadeOutWithoutLoad()
        {
            await _fadeImage.DOFade(0, _fadeDuration)
                .AsyncWaitForCompletion();
        }
    }
}
