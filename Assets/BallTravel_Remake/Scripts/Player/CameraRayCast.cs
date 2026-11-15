using UnityEngine;

namespace BallTravel_Remake.Scripts.Player
{
    public class CameraRayCast : MonoBehaviour
    {
        [SerializeField] private LayerMask _itemLayer;
        [SerializeField] private LayerMask _interactObjectLayer;
        [SerializeField] private float _rayDistance;
        private Ray _ray;
        private RaycastHit _hit;
        public bool IsRayHit { get; private set; }
        public GameObject HitObj { get; private set; }

        void Update()
        {
            _ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            IsRayHit = Physics.Raycast(_ray, out _hit, _rayDistance, _itemLayer)
                    || Physics.Raycast(_ray, out _hit, _rayDistance, _interactObjectLayer);

            HitObj = IsRayHit ? _hit.collider.gameObject : null;
        }
    }
}
