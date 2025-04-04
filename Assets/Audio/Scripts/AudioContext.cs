using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using WyzalUtilities.Data;
using NullReferenceException = System.NullReferenceException;

namespace WyzalUtilities.Audio
{
    public class AudioContext : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private string defaultContainer;
        [SerializeField] private SerializableDictionary<string, AudioContainer> audioContainers;
        [SerializeField] private AudioSource globalMusicAudioSource;
        [SerializeField] private AudioSource globalSfxAudioSource;

        #endregion

        #region Properties

        public static AudioContext Instance { get; private set; }

        public string DefaultContainerName => defaultContainer;

        #endregion

        #region Initialization

        private void Awake()
        {
            Init(this);
        }

        private static void Init(AudioContext context)
        {
            if (Instance == null)
            {
                Instance = context;
                DontDestroyOnLoad(context);
            }
            else
            {
                Destroy(context.gameObject);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play music
        /// </summary>
        /// <param name="musicName">Music name</param>
        /// <param name="fadeSettings">Class that handles fade parameters. if null - no fade applied</param>
        /// <param name="containerName">Container name where music stores. if null - default container will be used</param>
        /// <param name="audioSource">Audio Source to play music. if null - global audio source will be used.</param>
        public static void PlayGlobalMusic(string musicName, FadeSettings fadeSettings = null,
            string containerName = null, AudioSource audioSource = null)
        {
            if (fadeSettings == null)
            {
                Instance.PlayMusic(musicName, containerName, audioSource);
                return;
            }

            Instance.PlayMusicWithFade(musicName, fadeSettings, containerName, audioSource);
        }

        /// <summary>
        /// Play sfx
        /// </summary>
        /// <param name="sfxName">Sfx name</param>
        /// <param name="containerName">Container name where sfx stores. if null - default container will be used</param>
        /// <param name="audioSource">Audio Source to play sfx. if null - global audio source will be used.</param>
        public static void PlayGlobalSfx(string sfxName, string containerName = null, AudioSource audioSource = null)
        {
            Instance.PlayOneShot(sfxName, containerName, audioSource);
        }

        #endregion

        #region Private Methods

        private void PlayMusicWithFade(string musicName, FadeSettings fadeSettings, string containerName = null,
            AudioSource audioSource = null)
        {
            if (audioSource == null)
                audioSource = globalMusicAudioSource;
            var endVolume = FindVolumeOfSound(musicName, SoundType.Music, containerName);

            var sequence = DOTween.Sequence();
            sequence.Append(
                audioSource.DOFade(0, fadeSettings.durationIn).SetEase(fadeSettings.easeIn).SetUpdate(true)
                    .OnComplete(() => PlayMusic(musicName, containerName, audioSource)));
            sequence.Append(
                audioSource.DOFade(endVolume, fadeSettings.durationOut).SetEase(fadeSettings.easeOut).SetUpdate(true));
        }

        private void PlayMusic(string musicName, string containerName = null,
            AudioSource audioSource = null)
        {
            //check for globals audioSource
            if (audioSource == null)
                audioSource = globalMusicAudioSource;

            containerName ??= defaultContainer;

            var audioContainer = audioContainers[containerName];

            if (audioContainer == null)
            {
                Debug.LogException(new Exception($"Can't find audio container {containerName} in audio context."));
                return;
            }

            //play sound
            if (audioContainer.TryFindSound(out var musicSound, musicName, SoundType.Music))
            {
                audioSource.clip = musicSound.clip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                Debug.LogException(new Exception($"Can't find Sound in audio container {containerName}."));
            }
        }

        private float FindVolumeOfSound(string soundName, SoundType soundType, string containerName = null)
        {
            containerName ??= defaultContainer;
            var audioContainer = audioContainers[containerName];

            if (audioContainer.TryFindSound(out var sound, soundName, soundType))
            {
                return sound.volume;
            }
            else
            {
                Debug.LogException(new Exception($"Can't find Sound in audio container {containerName}."));
                return 0;
            }
        }

        private void PlayOneShot(string sfxName, string containerName = null,
            AudioSource audioSource = null)
        {
            //check for globals audioSource
            if (audioSource == null)
                audioSource = globalSfxAudioSource;

            containerName ??= defaultContainer;

            var audioContainer = audioContainers[containerName];

            if (audioContainer == null)
            {
                Debug.LogException(new Exception($"Can't find audio container {containerName} in audio context."));
                return;
            }

            //play sound
            if (audioContainer.TryFindSound(out var sfxSound, sfxName, SoundType.Sfx))
            {
                audioSource.PlayOneShot(sfxSound.clip, sfxSound.volume);
            }
            else
            {
                Debug.LogException(new Exception($"Can't find Sound in audio container {containerName}."));
            }
        }

        #endregion
    }

    public class FadeSettings
    {
        public float durationIn = 0.5f;
        public Ease easeIn;
        public float durationOut = 0.5f;
        public Ease easeOut;
    }
}