using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;
using UnityEngine;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class HealAirPump : MonoBehaviour
    {
        //チェックポイントに触れた時点で使用済みなら、リトライしても復活させない
        private bool _isUsedConfirmed = false;

        //使って非アクティブになっても購読は残るので、リトライ時に自分で復活できる
        private void Start()
        {
            GameManager.Instance.OnCheckPointReached += ConfirmUsed;
            GameManager.Instance.OnRespawn += Revive;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnCheckPointReached -= ConfirmUsed;
            GameManager.Instance.OnRespawn -= Revive;
        }

        //チェックポイントに触れたとき: もう使っていたら使用済みで確定する
        private void ConfirmUsed()
        {
            if (gameObject.activeSelf) return;
            _isUsedConfirmed = true;
        }

        //リトライ時: チェックポイント以降に使ったポンプだけ復活させる
        private void Revive()
        {
            if (_isUsedConfirmed) return;
            gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Health>(out Health health))
            {
                health.Heal(30f);
                gameObject.SetActive(false);
            }
        }
    }
}
