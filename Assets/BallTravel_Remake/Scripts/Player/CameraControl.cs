using UnityEngine;

namespace BallTravel_Remake.Scripts.Player
{
    public class CameraControl : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, -6f);
        [SerializeField] private Vector2 _cameraSpeed = new Vector2(3, 3);
        [Tooltip("カメラの下回転の制限")]
        [SerializeField] private float _minPitch = 30f;
        [Tooltip("カメラの上回転の制限")]
        [SerializeField] private float _maxPitch = 90f;
        [Tooltip("障害物のレイヤーを指定")]
        [SerializeField] private LayerMask _obstacleLayer;
        [Tooltip("カメラが障害物との間に設ける間隔")]
        [SerializeField] private float _cameraRadius = 0.2f;

        private float _yaw = 0f;
        private float _pitch = 0f;

        private Quaternion CameraRotation =>
        Quaternion.Euler(_pitch,_yaw, 0f);

        void Start()
        {
            transform.position =
            _target.position + transform.rotation * _offset;
        }
        public void RotateCamera(Vector2 rotation)
        {
            _yaw += rotation.x * _cameraSpeed.x * Time.deltaTime;
            _pitch -= rotation.y * _cameraSpeed.y * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        //カメラとターゲットの間の障害物を検知し、カメラを移動させる関数
        private void CameraTransformToRayHitPos()
        {
            //カメラの所定位置
            Vector3 desiredPos =
            _target.position + transform.rotation * _offset;

            //カメラからターゲットへの距離を計算
            Vector3 rayDirection =
            desiredPos - _target.transform.position;

            float distance = rayDirection.magnitude;

            //カメラとターゲットの間にRayを発射する
            Ray ray = new Ray(_target.position, rayDirection.normalized);
            if (Physics.Raycast(ray,out RaycastHit hit,distance,_obstacleLayer))
            {
                //Rayに障害物が当たればそこにカメラを移動
                transform.position =
                hit.point - ray.direction * _cameraRadius;
            }
            else
            {
                //何もなければ所定位置へ
                transform.position = desiredPos;
            }
        }
        private void LateUpdate()
        {
            transform.rotation = CameraRotation;
            CameraTransformToRayHitPos();
        }


    }
}
