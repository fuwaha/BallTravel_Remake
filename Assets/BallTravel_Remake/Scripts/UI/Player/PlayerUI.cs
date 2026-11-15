using UnityEngine;
using BallTravel_Remake.Scripts.Player;

namespace BallTravel_Remake.Scripts.UI
{
    public class PlayerUI : MonoBehaviour
    {
        public Health Health { get; private set; }
        public PlayerStatus PlayerStatus { get; private set; }
        public PlayerControl PlayerControl { get; private set; }

        void Awake()
        {
            Health = transform.parent.GetComponent<Health>();
            PlayerStatus = transform.parent.GetComponent<PlayerStatus>();
            PlayerControl = transform.parent.GetComponent<PlayerControl>();

            // 配下のUI部品を自動で集めて紐づける(非アクティブなものも含む)
            foreach (var part in GetComponentsInChildren<IPlayerUIPart>(true))
            {
                part.Bind(this);
            }
        }
    }
}
