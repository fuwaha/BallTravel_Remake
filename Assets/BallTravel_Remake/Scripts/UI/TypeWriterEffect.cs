using TMPro;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BallTravel_Remake.Scripts.UI
{
    public class TypeWriterEffect : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tutorialUI;
        [SerializeField] private string _uiText;
        [SerializeField] private float _showSpeed = 0.5f;

        void Start()
        {
            ShowText(this.GetCancellationTokenOnDestroy()).Forget();
        }

        //テキストをタイプライターのように一文字ずつ表示する
        private async UniTask ShowText(CancellationToken token)
        {
            _tutorialUI.text = "";

            for (int i = 0; i < _uiText.Length; i++)
            {
                // \n を検出したら改行する
                if (_uiText[i] == '\\' && i + 1 < _uiText.Length
                    && _uiText[i + 1] == 'n')
                {
                    _tutorialUI.text += "\n";
                    i++; // 次の n をスキップ
                }
                else
                {
                    _tutorialUI.text += _uiText[i];
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_showSpeed), cancellationToken: token);
            }
        }
    }
}
