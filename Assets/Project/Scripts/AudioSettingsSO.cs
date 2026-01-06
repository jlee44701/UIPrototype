using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Settings
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Settings/AudioSettings", order = 1)]
    public class AudioSettingsSO : ScriptableObject
    {
        // Default ScriptableObject data
        const float k_DefaultMasterVolume = 1f;
        const float k_DefaultSFXVolume = 1f;
        const float k_DefaultMusicVolume = 0f;

        [Header("Mixer")]
        [Tooltip("The AudioMixer that controls the audio levels for the game")]
        [SerializeField] private AudioMixer m_AudioMixer;

        [Header("Volume Settings")]
        [Tooltip("The master volume level (0 to 1)")]
        [SerializeField] private float m_MasterVolume = k_DefaultMasterVolume;

        [Tooltip("The sound effects volume level (0 to 1)")]
        [SerializeField] private float m_SoundEffectsVolume = k_DefaultSFXVolume;

        [Tooltip("The music volume level (0 to 1)")]
        [SerializeField] private float m_MusicVolume = k_DefaultMusicVolume;

        // Convert bool to 1 and 0 and multiply by MixerGroup (unused in this project, here for demo purposes)
        [Header("Mute Settings")]
        [Tooltip("Mute or unmute the master volume")]
        [SerializeField] private bool m_IsMasterMuted = false;

        [Tooltip("Mute or unmute the sound effects volume")]
        [SerializeField] private bool m_IsSoundEffectsMuted = false;

        [Tooltip("Mute or unmute the music volume")]
        [SerializeField] private bool m_IsMusicMuted = false;

        [Header("Sound Effects")]
        [Tooltip("AudioClip for correctly answering a question")]
        [SerializeField] private AudioClip m_SuccessSound;
        [Tooltip("AudioClip for incorrectly answering a question")]
        [SerializeField] private AudioClip m_FailSound;
        [Tooltip("AudioClip for passing the quiz")]
        [SerializeField] private AudioClip m_GameWonSound;
        [Tooltip("AudioClip for failing the quiz")]
        [SerializeField] private AudioClip m_GameLostSound;

        [Tooltip("AudioClip for simple button tap")]
        [SerializeField] private AudioClip m_TapClickSound;

        // Properties
        public AudioMixer AudioMixer => m_AudioMixer;
        public AudioClip SuccessSound => m_SuccessSound;
        public AudioClip FailSound => m_FailSound;
        public AudioClip GameWonSound => m_GameWonSound;
        public AudioClip GameLostSound => m_GameLostSound;
        public AudioClip TapClickSound => m_TapClickSound;


        public float SoundEffectsVolume { get => m_SoundEffectsVolume; set => m_SoundEffectsVolume = value; }
        public float MusicVolume { get => m_MusicVolume; set => m_MusicVolume = value; }

        public float MasterVolume 
        { 
            get => m_MasterVolume; 
            set 
            { 
                m_MasterVolume = value; 
                ApplyMasterVolume(); 
            } 
        }

        public bool IsMasterMuted 
        { 
            get => m_IsMasterMuted; 
            set 
            { 
                m_IsMasterMuted = value; 
                ToggleMasterMute(); 
            } 
        }
        
        
        public bool IsSoundEffectsMuted { get => m_IsSoundEffectsMuted; set => m_IsSoundEffectsMuted = value; }
        public bool IsMusicMuted { get => m_IsMusicMuted; set => m_IsMusicMuted = value; }

        float m_PreviousMasterVolume;
        
        // Event subscriptions
        private void OnEnable()
        {
            SettingsEvents.MasterVolumeChanged += SettingsEvents_MasterVolumeChanged;
            SettingsEvents.MusicVolumeChanged += SettingsEvents_MusicVolumeChanged;
            SettingsEvents.SFXVolumeChanged += SettingsEvents_SFXVolumeChanged;

            // Note: we disable validating the Sound Effects AudioClips here, instead we
            // validate it from the GameplaySounds class

            // NullRefChecker.Validate(this);
        }

        // Event unsubscriptions
        private void OnDisable()
        {
            SettingsEvents.MasterVolumeChanged -= SettingsEvents_MasterVolumeChanged;
            SettingsEvents.MusicVolumeChanged -= SettingsEvents_MusicVolumeChanged;
            SettingsEvents.SFXVolumeChanged -= SettingsEvents_SFXVolumeChanged;
        }


        void ToggleMasterMute()
        {
            if (m_IsMasterMuted)
            {
                m_PreviousMasterVolume = m_MasterVolume;
                m_AudioMixer.SetFloat("MasterVolume", -80f); // Mute (in decibels)
            }
            else
            {
                m_AudioMixer.SetFloat("MasterVolume", Mathf.Log10(m_PreviousMasterVolume) * 20);
            }
        }
        
        void ApplyMasterVolume()
        {
            if (!m_IsMasterMuted)
            {
                m_AudioMixer.SetFloat("MasterVolume", Mathf.Log10(m_MasterVolume) * 20);
            }
        }

        // Update the master volume when the event is triggered.
        private void SettingsEvents_MasterVolumeChanged(float volume)
        {
            MasterVolume = volume;
        }
        
        // Update the sound effects volume when the event is triggered.
        private void SettingsEvents_SFXVolumeChanged(float volume)
        {
            SoundEffectsVolume = volume;
        }

        // Update the music volume when the event is triggered.
        private void SettingsEvents_MusicVolumeChanged(float volume)
        {
            MusicVolume = volume;
        }
    }
}
