using System.Collections.Generic;
using UnityEngine;

namespace BallTravel_Remake.Scripts.Managers
{
    public class AudioManager
    {
        public float Volume { get; private set; } = 0.5f; // デフォルトの音量を50に設定
        public void SetVolume(float newVolume) => Volume = newVolume;

        public enum SE
        {
            Button,
            ButtonLong,
        }
        [SerializeField] private List<AudioClip> _seClips;

    }
}
