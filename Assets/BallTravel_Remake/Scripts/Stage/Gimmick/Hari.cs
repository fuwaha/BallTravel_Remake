using UnityEngine;
using BallTravel_Remake.Scripts.Player;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    public class Hari : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {

            if (other.TryGetComponent<Health>(out Health health))
                health.DotTrigger();
        }
    }
}
