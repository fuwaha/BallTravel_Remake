using UnityEngine;
using System;
using BallTravel_Remake.Scripts.UI;

namespace BallTravel_Remake.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Tooltip("フェード用のクラス")]
        [SerializeField] private Fader _fader;

        //--------------ゲームイベント----------//
        public event Action OnGameClear;
        public event Action OnGameOver;
        public bool IsClear { get; private set; } = false;
        public bool IsOver { get; private set; } = false;
        public event Action OnRespawn;

        //チェックポイントに触れたときに発行するイベント
        //購読している側は、リトライで戻すための自分の状態を保存する
        public event Action OnCheckPointReached;
        //チェックポイントに触れた時点のタイム(リトライ時にここへ戻す)
        private float _savedTimer = 0f;

        //CheckPointに触れたときに実行
        public void ReachCheckPoint()
        {
            _savedTimer = Timer;
            OnCheckPointReached?.Invoke();
        }
        public void ClearGame()
        {
            if (IsClear || IsOver) return;

            Cursor.visible = true;
            StopTimer();
            IsClear = true;
            OnGameClear?.Invoke();
        }
        public void FailGame()
        {
            if (IsClear || IsOver) return;

            Cursor.visible = true;
            IsOver = true;
            StopTimer();
            OnGameOver?.Invoke();
        }

        //ゲームオーバーから直前のチェックポイントでやり直す
        //GameOverUIのリトライボタンで実行
        //フェードで画面が隠れている間に、OnRespawnを購読している側が自分の状態を戻す
        public async void Retry()
        {
            if (!IsOver) return;

            await _fader.FadeInWithoutLoad();

            Cursor.visible = false;
            IsOver = false;
            OnRespawn?.Invoke();
            //チェックポイントに触れた時点のタイムから再開する
            Timer = _savedTimer;
            StartTimer();

            await _fader.FadeOutWithoutLoad();
        }

        //--------------ポーズ関連の関数、変数---------------//
        public bool IsPause { get; private set; } = false;
        public event Action OnPausing;
        public event Action OnExitPause;
        //キーからのポーズ状態切り替え
        //playerLogicで実行
        public void SwitchPausingFromKey()
        {
            if (IsClear) return;
            if (IsOver) return;

            Cursor.visible = !IsPause;

            if (!IsPause)
            {
                OnPausing?.Invoke();
                IsPause = true;
                return;
            }

            if (IsPause)
            {
                OnExitPause?.Invoke();
                IsPause = false;
            }
        }

        //UIからのポーズ状態切り替え
        //isPressedなどで実行
        public void SwitchPausingFromUI()
        {
            if (IsClear) return;
            if (IsOver) return;

            Cursor.visible = !IsPause;

            if (!IsPause)
            {
                OnPausing?.Invoke();

                IsPause = true;
                return;
            }

            if (IsPause)
            {
                OnExitPause?.Invoke();
                IsPause = false;
            }
        }

        //--------Managers-----------//
        public static GameManager Instance { get; private set; }
        public SceneController SceneController { get; private set; }
        public InputController InputController { get; private set; }
        public AudioManager AudioManager { get; private set; }

        //どのシーンから再生しても、最初のシーンの読み込み前にGameManagerを生成する
        //Resourcesフォルダに「GameManager」という名前のprefabを置いておくこと(_faderもprefabの中に入れる)
        //シーンに置いたGameManagerがある場合は、Awakeの重複チェックで破棄される
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            if (Instance != null) return;

            var prefab = Resources.Load<GameManager>("GameManager");
            if (prefab == null)
            {
                Debug.LogError("Resources/GameManager.prefab が見つかりません");
                return;
            }

            Instantiate(prefab);
        }

        void Awake()
        {
            //GameManagerのシングルトン化、各マネージャーのインスタンス生成
            //Awakeより先に各マネージャーを参照しないように注意
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SceneController = new SceneController();
            InputController = new InputController();
            AudioManager = new AudioManager();
        }

        //-------------タイマー関連の関数、変数-------------//
        public float Timer { get; private set; } = 0f;
        private bool _isTimerRunning = false;


        public void StartTimer()
        {
            _isTimerRunning = true;
        }

        public void StopTimer()
        {
            _isTimerRunning = false;
        }

        public void ResetTimer()
        {
            Timer = 0f;
        }

        void Update()
        {
            if (IsPause) return;
            if (!_isTimerRunning) return;

            Timer += Time.deltaTime;
        }

        //-------------ゲーム状態のリセット-------------//
        //GameManagerはシーンをまたいで残るため、引き継ぎたくない状態はここでまとめて初期化する
        //初期化が必要な変数を追加したときは、ここにも追記すること
        public void ResetGameState()
        {
            IsClear = false;
            IsOver = false;
            IsPause = false;
            StopTimer();
            ResetTimer();
            _savedTimer = 0f;
            Cursor.visible = true;
        }

        //-------------ゲームのシーン遷移用の関数、必要であれば他の処理も追加,、その場合awaitの前に書くこと-------------//

        public async void LoadToTitle()
        {
            ResetGameState();
            await _fader.StartFade();
            SceneController.LoadScene(SceneController.SceneName.Title);
        }

        public async void ChangeSceneToStage1()
        {
            await _fader.StartFade();
            SceneController.LoadScene(SceneController.SceneName.Stage1);
        }

        public async void ChangeSceneToStage2()
        {
            await _fader.StartFade();
            SceneController.LoadScene(SceneController.SceneName.Stage2);
        }

        public async void ChangeSceneToStage3()
        {
            await _fader.StartFade();
            SceneController.LoadScene(SceneController.SceneName.Stage3);
        }

    }
}