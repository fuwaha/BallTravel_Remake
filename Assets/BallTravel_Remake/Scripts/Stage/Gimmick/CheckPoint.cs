using System;
using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;
using UnityEngine;
using DG.Tweening;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _nuno;

        //プレイヤーが触れたときに発行するイベント(RiseGroundなどが購読する)
        public event Action OnPassed;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerControl>(out PlayerControl playerControl))
            {
                playerControl.SetNewRespawnTransform(_spawnPoint);
                _nuno.DOLocalMoveY(0f, 1f);
                OnPassed?.Invoke();
                GameManager.Instance.ReachCheckPoint();
                Destroy(this);
                Destroy(GetComponent<BoxCollider>());
            }
            else
            {
                Debug.Log("PlayerControlが検出できなかった");
            }
        }
    }
}
