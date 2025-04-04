using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WyzalUtilities.Audio
{
    [CreateAssetMenu(fileName = "NewAudioContainer", menuName = "WyzalUtilities/Audio/AudioContainer")]
    public class AudioContainer : ScriptableObject
    {
        [SerializeField] private List<Sound> musicSounds = new List<Sound>();
        [SerializeField] private List<Sound> sfxSounds = new List<Sound>();

        public bool TryFindSound(out Sound sound, string soundName, SoundType type)
        {
            sound = null;
            if (soundName == null) return false;

            switch (type)
            {
                case SoundType.Music:
                {
                    if (musicSounds.Count == 0) return false;
                    sound = FindSound(musicSounds, soundName);
                    break;
                }

                case SoundType.Sfx:
                    if (sfxSounds.Count == 0) return false;
                    sound = FindSound(sfxSounds, soundName);
                    break;

                default:
                    return false;
            }

            return sound != null;
        }

        private static Sound FindSound(List<Sound> sounds, string name)
        {
            return sounds.First(sound => sound.name == name);
        }
    }

    public enum SoundType
    {
        Music,
        Sfx
    }
}