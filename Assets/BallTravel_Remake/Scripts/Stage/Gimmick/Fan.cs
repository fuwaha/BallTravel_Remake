using UnityEngine;
using BallTravel_Remake.Scripts.Player;
using BallTravel_Remake.Scripts.Managers;

namespace BallTravel_Remake.Scripts.Stage.Gimmick
{
    //扇風機。このオブジェクトのBoxCollider(Trigger)が風の範囲で、中のボールを青い矢印(Z+)の向きに押す
    [RequireComponent(typeof(BoxCollider))]
    public class Fan : MonoBehaviour
    {
        [Header("風")]
        [Tooltip("風の強さ(重さに関係なく、この加速度で押す)")]
        [SerializeField] private float _windForce = 15f;
        [Tooltip("扇風機から遠いほど風を弱くするか")]
        [SerializeField] private bool _isFalloff = true;
        [Tooltip("風の範囲の一番奥での強さの割合(0.3なら30%)")]
        [Range(0f, 1f)]
        [SerializeField] private float _farForceRate = 0.3f;

        [Header("オン/オフ")]
        [Tooltip("オン/オフを繰り返すか。オフなら回りっぱなし")]
        [SerializeField] private bool _isCycle = true;
        [Tooltip("風が吹いている秒数")]
        [SerializeField] private float _onDuration = 3f;
        [Tooltip("風が止まっている秒数")]
        [SerializeField] private float _offDuration = 2f;
        [Tooltip("回り始め・止まり始めに、最大まで変わるのにかかる秒数")]
        [SerializeField] private float _powerChangeDuration = 0.5f;

        [Header("羽根")]
        [Tooltip("回す羽根。空なら回さない")]
        [SerializeField] private Transform _blade;
        [Tooltip("羽根を回す軸(羽根のローカル座標)")]
        [SerializeField] private Vector3 _bladeAxis = Vector3.forward;
        [Tooltip("全力のときの回転速度(度/秒)")]
        [SerializeField] private float _bladeSpeed = 720f;

        [Header("風のエフェクト")]
        [Tooltip("風の線を出すParticleSystem(子に置く)。空ならエフェクトなし。マテリアルだけ自分でセットしてね")]
        [SerializeField] private ParticleSystem _windEffect;
        [Tooltip("線が流れる速さ")]
        [SerializeField] private float _effectSpeed = 12f;
        [Tooltip("1秒に出す線の数")]
        [SerializeField] private float _effectRate = 30f;
        [Tooltip("線の色(透明度もここで)")]
        [SerializeField] private Color _effectColor = new Color(1f, 1f, 1f, 0.35f);
        [Tooltip("この強さ(0〜1)より弱いときは線を出さない")]
        [Range(0f, 1f)]
        [SerializeField] private float _effectThreshold = 0.5f;

        private BoxCollider _windArea;
        //風の範囲の一番奥(ローカルZ)
        private float _farZ;

        //今の強さ(0〜1)。風・羽根・エフェクトは全部これに合わせる
        private float _power;
        private bool _isWindOn = true;
        private float _cycleTimer;
        private bool _isPaused;

        //今風の範囲にいるボール。いなければnull
        private Rigidbody _targetRb;

        void Awake()
        {
            _windArea = GetComponent<BoxCollider>();
            _windArea.isTrigger = true;
            _farZ = _windArea.center.z + _windArea.size.z * 0.5f;

            //最初はいきなり全力で吹いている
            _power = 1f;

            SetupWindEffect();
        }

        private void Start()
        {
            GameManager.Instance.OnPausing += FanStop;
            GameManager.Instance.OnExitPause += FanRestart;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPausing -= FanStop;
            GameManager.Instance.OnExitPause -= FanRestart;
        }

        private void FanStop()
        {
            _isPaused = true;
            _windEffect?.Pause();
        }

        private void FanRestart()
        {
            _isPaused = false;
            _windEffect?.Play();
        }

        void Update()
        {
            if (_isPaused) return;

            UpdateCycle();
            UpdatePower();
            RotateBlade();
            UpdateEffectEmission();
        }

        void FixedUpdate()
        {
            if (_isPaused) return;
            if (_targetRb == null || _power <= 0f) return;

            _targetRb.AddForce(transform.forward * (_windForce * GetFalloffRate() * _power), ForceMode.Acceleration);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null) return;
            if (!other.attachedRigidbody.TryGetComponent<PlayerControl>(out _)) return;

            _targetRb = other.attachedRigidbody;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == _targetRb)
                _targetRb = null;
        }

        private void UpdateCycle()
        {
            if (!_isCycle)
            {
                _isWindOn = true;
                return;
            }

            _cycleTimer += Time.deltaTime;
            float duration = _isWindOn ? _onDuration : _offDuration;
            if (_cycleTimer < duration) return;

            _cycleTimer -= duration;
            _isWindOn = !_isWindOn;
        }

        private void UpdatePower()
        {
            float target = _isWindOn ? 1f : 0f;
            if (_powerChangeDuration <= 0f)
            {
                _power = target;
                return;
            }

            _power = Mathf.MoveTowards(_power, target, Time.deltaTime / _powerChangeDuration);
        }

        private void RotateBlade()
        {
            if (_blade == null) return;

            _blade.Rotate(_bladeAxis, _bladeSpeed * _power * Time.deltaTime, Space.Self);
        }

        //扇風機に近いほど1、一番奥で_farForceRate
        private float GetFalloffRate()
        {
            if (!_isFalloff || _farZ <= 0f) return 1f;

            float localZ = transform.InverseTransformPoint(_targetRb.position).z;
            float t = Mathf.Clamp01(localZ / _farZ);
            return Mathf.Lerp(1f, _farForceRate, t);
        }

        private void UpdateEffectEmission()
        {
            if (_windEffect == null) return;

            var emission = _windEffect.emission;
            emission.enabled = _power >= _effectThreshold;
        }

        //風の範囲に合わせてParticleSystemの設定を自動で作る
        private void SetupWindEffect()
        {
            if (_windEffect == null) return;

            //扇風機と同じ位置・向きにそろえる(風の範囲の計算をそのまま使うため)
            Transform effectTransform = _windEffect.transform;
            effectTransform.localPosition = Vector3.zero;
            effectTransform.localRotation = Quaternion.identity;
            effectTransform.localScale = Vector3.one;

            Vector3 center = _windArea.center;
            Vector3 size = _windArea.size;
            float length = Mathf.Max(size.z, 0.01f);
            float speed = Mathf.Max(_effectSpeed, 0.01f);

            var main = _windEffect.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startLifetime = length / speed;
            main.startSpeed = speed;
            main.startSize = 0.05f;
            main.startColor = _effectColor;
            main.gravityModifier = 0f;
            main.maxParticles = 1000;

            var emission = _windEffect.emission;
            emission.rateOverTime = _effectRate;

            //扇風機側の面から奥(Z+)に向かって出す
            var shape = _windEffect.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.position = new Vector3(center.x, center.y, center.z - size.z * 0.5f);
            shape.rotation = Vector3.zero;
            shape.scale = new Vector3(size.x, size.y, 0f);
            shape.randomDirectionAmount = 0f;

            //出始めと消える前はうっすら
            var colorOverLifetime = _windEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            //進む向きに伸ばして線に見せる
            var particleRenderer = _windEffect.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            particleRenderer.lengthScale = 2f;
            particleRenderer.velocityScale = 0.1f;
            if (particleRenderer.sharedMaterial == null)
                Debug.LogWarning($"{name}: 風のエフェクトにマテリアルが入ってないよ(ParticlesUnlitを入れてね)");

            _windEffect.Clear();
            _windEffect.Play();
        }
    }
}
