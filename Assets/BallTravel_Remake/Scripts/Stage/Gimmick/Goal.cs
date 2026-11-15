using UnityEngine;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class Goal : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.ClearGame();
            }
        }
    }
}
