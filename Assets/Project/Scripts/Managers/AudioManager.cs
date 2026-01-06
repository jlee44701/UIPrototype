    using System.Collections;
    using System.Collections.Generic;
    using Audio;
    using PixelEngine;
    using RuntimeUI;
    using Settings;
    using UnityEngine;
    using SettingsEvents = RuntimeUI.SettingsEvents;

    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance => SingletonBehaviourHelper<AudioManager>.Instance;
        [SerializeField] SfxChannel m_SfxChannelPrefab;
        [SerializeField] int m_InitialChannelCount = 2;
        readonly List<SfxChannel> m_AllChannels = new List<SfxChannel>();
        
        // AudioMixer group names
        const string k_SFXVolume = "SFXVolume";
        const string k_MusicVolume = "MusicVolume";
        const string k_MasterVolume = "MasterVolume";

        [Tooltip("AudioSettings ScriptableObject storing volume settings and AudioMixer")]
        [SerializeField] AudioSettingsSO m_AudioSettings;

        [Tooltip("AudioSource dedicated to playing sound effects")]
        [SerializeField] AudioSource m_SFXAudioSource;

        [Tooltip("AudioSource dedicated to playing music")]
        [SerializeField] AudioSource m_MusicAudioSource;
        [SerializeField] AudioLowPassFilter m_SfxLowPassFilter;
        public AudioSettingsSO AudioSettings => m_AudioSettings;
        
        
        private Dictionary<AudioClip, float> limitedFrequencySounds = new Dictionary<AudioClip, float>();
        
        Camera m_Camera;
        Camera Camera {
            get {
                if (!m_Camera)
                    m_Camera = Camera.main;
                return m_Camera;
            } 
        }
        void Awake()
        {
            for (var channelIndex = 0; channelIndex < m_InitialChannelCount; channelIndex++)
            {
                CreateNewChannel();
            }
        }
        public void PlaySfx2D(
            AudioClip clip, 
            Vector3 position, 
            AudioParams.Pitch pitch, 
            AudioParams.Repetition repetition,
            AudioParams.Randomization randomization,
            AudioParams.Distortion distortion,
            bool looping = false,
            float muffledCutoffFreq = 300,
            float volume = 1, 
            float skipToTime = 0) 
        {
            if (repetition != null)
            {
                if (RepetitionIsTooFrequent(clip, repetition.minRepetitionFrequency)) {
                    return;
                }
            }

            // if (randomization != null)
            // {
            //     randomization.
            //     randomVariationId = GetRandomVariationOfSound(soundId, randomization.noRepeating);
            // }
            
            Play(clip, position, volume, 0, pitch.pitch, false, distortion.muffled, muffledCutoffFreq);
        }
        void Play(
            AudioClip clip,
            Vector3 position,
            float volume,
            float normalizedSkipTime,
            float pitch,
            bool isLooping,
            bool isMuffled,
            float muffledCutoffFrequencyHz)
        {
            if (!clip)
                return;

            var channel = GetOrCreateAvailableChannel();
            channel.Play(
                clip,
                position,
                volume,
                normalizedSkipTime,
                pitch,
                isLooping,
                isMuffled,
                muffledCutoffFrequencyHz,
                OnChannelFinished);
        }

        SfxChannel GetOrCreateAvailableChannel()
        {
            for (var channelIndex = 0; channelIndex < m_AllChannels.Count; channelIndex++)
            {
                var channel = m_AllChannels[channelIndex];
                if (channel && channel.IsAvailable)
                    return channel;
            }

            return CreateNewChannel();
        }

        SfxChannel CreateNewChannel()
        {
            if (!m_SfxChannelPrefab)
                return null;

            var channel = Instantiate(m_SfxChannelPrefab, transform);
            m_AllChannels.Add(channel);
            return channel;
        }

        void OnChannelFinished(SfxChannel channel)
        {
            // We could disable the GameObject here if we want.
            // We’re already “free” via IsAvailable, so this can stay empty.
            if (!channel)
                return;
        }
        // Event subscriptions
        private void OnEnable()
        {
            m_Camera = Camera.main;
            SettingsEvents.SFXVolumeChanged += SettingsEvents_OnSFXVolumeChanged;
            SettingsEvents.MusicVolumeChanged += SettingsEvents_OnMusicVolumeChanged;
            SettingsEvents.MasterVolumeChanged += SettingsEvents_OnMasterVolumeChanged;

            Initialize();
        }

        // Event unsubscriptions
        private void OnDisable()
        {
            SettingsEvents.SFXVolumeChanged -= SettingsEvents_OnSFXVolumeChanged;
            SettingsEvents.MusicVolumeChanged -= SettingsEvents_OnMusicVolumeChanged;
            SettingsEvents.MasterVolumeChanged -= SettingsEvents_OnMasterVolumeChanged;
        }

        // Sets the initial values from the AudioSettings
        private void Initialize()
        {
            // Verifies required fields in the Inspector
            NullRefChecker.Validate(this);

            SettingsEvents_OnSFXVolumeChanged(m_AudioSettings.SoundEffectsVolume);
            SettingsEvents_OnMusicVolumeChanged(m_AudioSettings.MusicVolume);
            SettingsEvents_OnMasterVolumeChanged(m_AudioSettings.MasterVolume);
        }


        #region Event-handling methods (responds to volume change events, raised by the SettingsManager)
        private void SettingsEvents_OnSFXVolumeChanged(float volume)
        {
            float decibelVolume = ConvertLinearToDecibel(volume);
            m_AudioSettings.AudioMixer.SetFloat(k_SFXVolume, decibelVolume);
        }

        private void SettingsEvents_OnMusicVolumeChanged(float volume)
        {
            float decibelVolume = ConvertLinearToDecibel(volume);
            m_AudioSettings.AudioMixer.SetFloat(k_MusicVolume, decibelVolume);
        }

        private void SettingsEvents_OnMasterVolumeChanged(float volume)
        {
            float decibelVolume = ConvertLinearToDecibel(volume);
            m_AudioSettings.AudioMixer.SetFloat(k_MasterVolume, decibelVolume);
        }
        #endregion

        public void PlaySfx2DAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float skipToTime = 0, AudioParams.Pitch pitch = null, AudioParams.Repetition repetition = null, AudioParams.Randomization randomization = null, AudioParams.Distortion distortion = null, bool looping = false) {
            if (repetition != null)
            {
                if (RepetitionIsTooFrequent(clip, repetition.minRepetitionFrequency)) {
                    return;
                }
            }
            
            // if (randomization != null)
            // {
            //     randomization.
            //     randomVariationId = GetRandomVariationOfSound(soundId, randomization.noRepeating);
            // }

            // var source = CreateAudioSourceForSound(randomVariationId, position, looping);
            if (m_SFXAudioSource != null)
            {
                m_SFXAudioSource.volume = volume;
                m_SFXAudioSource.time = m_SFXAudioSource.clip.length * skipToTime;
                
                if (pitch != null)
                {
                    m_SFXAudioSource.pitch = pitch.pitch;
                }

                if (distortion != null)
                {
                    if (distortion.muffled)
                    {
                        //MuffleSource(source);
                    }
                }
                m_SFXAudioSource.PlayOneShot(clip);
            }
            
        }
        private void MuffleSource(AudioSource source, float cutoff = 300f)
        {
            m_SfxLowPassFilter.cutoffFrequency = cutoff;
        }
        // Play a sound effect at the specified position.
        public void PlaySfx2DAtPoint(AudioClip clip, Vector3 position, float delay = 0f, bool loop = false, float pitch = 1f, float volume = 1f)
        {
            m_SFXAudioSource.Stop();
            StartCoroutine(PlaySfxAtPointDelayed(clip, position, delay, loop, pitch, volume));
        }

        // Coroutine to play a sound effect at the specified position with a delay.
        private IEnumerator PlaySfxAtPointDelayed(AudioClip clip, Vector3 position, float delay, bool loop, float pitch, float volume = 1f)
        {
            yield return new WaitForSeconds(delay);
            m_SFXAudioSource.pitch = pitch;
            m_SFXAudioSource.transform.position = position;
            m_SFXAudioSource.loop = loop;
            m_SFXAudioSource.volume = volume;

            if (clip != null)
                m_SFXAudioSource.PlayOneShot(clip);
            else
                m_SFXAudioSource.Stop();
        }

        // Play music with the specified AudioClip (unused in this demo)
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            m_MusicAudioSource.clip = clip;
            m_MusicAudioSource.loop = loop;
            m_MusicAudioSource.Play();
        }

        // Convert from the logarithmic AudioMixer scale (-80dB to 0dB) to linear UI scale (0 to 1) and vice versa
        public static float ConvertLinearToDecibel(float linearVolume)
        {
            return Mathf.Log10(Mathf.Max(0.0001f, linearVolume)) * 20.0f;
        }

        public static float ConvertDecibelToLinear(float decibelVolume)
        {
            return Mathf.Pow(10, decibelVolume / 20.0f);
        }
        private bool RepetitionIsTooFrequent(AudioClip clip, float frequencyMin, string entrySuffix = "")
        {
            float time = Time.unscaledTime;

            if (limitedFrequencySounds.ContainsKey(clip))
            {
                if (time - frequencyMin > limitedFrequencySounds[clip])
                {
                    limitedFrequencySounds[clip] = time;
                    return false;
                }
            }
            else
            {
                limitedFrequencySounds.Add(clip, time);
                return false;
            }

            return true;
        }

    }
