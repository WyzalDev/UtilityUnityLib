using System;
using UnityEngine;

namespace WyzalUtilities.Audio
{
    [Serializable]
    public class Sound
    {
        public string name;
        
        public float volume;

        public AudioClip clip;
    }
}