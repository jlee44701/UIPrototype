using System;
using System.Globalization;
using Audio;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Game.UI {
    public class TypewriterAudio : IDisposable{
        const bool EnableTypewriterDebugLogs = false;
        public AudioParams.Distortion Distortion { get; set; }
        AudioParams.Pitch m_Pitch = new AudioParams.Pitch(1);
        public AudioParams.Randomization Randomization { get; set; }
        public AudioParams.Repetition Repetition { get; set; }
        public AudioParams.Pitch.Variation PitchVariation { get; set; }
        public float TypingVolume { get; set; }
        
        public AudioClip TypeWriterSound { get; set; }
        TypewriterCore m_TypewriterCore;

        public TypewriterAudio(
            AudioParams.Pitch.Variation pitchVariation,
            AudioParams.Randomization randomization,
            AudioParams.Repetition repetition,
            AudioParams.Distortion distortion,
            float typingVolume,
            TypewriterCore typeWriterCore
            )
        {
            PitchVariation = pitchVariation;
            Randomization = randomization;
            Repetition = repetition;
            Distortion = distortion;
            TypingVolume = typingVolume;
            
            m_TypewriterCore =   typeWriterCore ?? throw new ArgumentNullException(nameof (typeWriterCore));
            m_TypewriterCore.OnCharacterVisible -= CharacterVisible;
            m_TypewriterCore.OnCharacterVisible += CharacterVisible;
            
        }
        public void Dispose() {
            if (m_TypewriterCore != null) {
                m_TypewriterCore.OnCharacterVisible -= CharacterVisible;    
            }
        }

        void CharacterVisible(CharacterData data)
        {
            var character = data.info.character;
            if (ShouldSkipCharacter(data, character))
                return;
            
        
            //if (!m_TypewriterCore.IsShowingText) return;
            
            var audioManager = AudioManager.Instance;
            if (!audioManager) return;
            audioManager.PlaySfx2D(
                TypeWriterSound,
                Vector3.zero,
                m_Pitch.Vary(PitchVariation),
                Repetition,
                Randomization,
                Distortion,
                false,
                300,
                TypingVolume,
                0);
        }

        static bool ShouldSkipCharacter(CharacterData data, char character)
        {
            var category = char.GetUnicodeCategory(character);
            var shouldSkip = !data.info.isRendered
                || !data.isVisible
                || char.IsWhiteSpace(character)
                || char.IsControl(character)
                || char.IsSurrogate(character)
                || category == UnicodeCategory.Format
                || category == UnicodeCategory.LineSeparator
                || category == UnicodeCategory.ParagraphSeparator
                || category == UnicodeCategory.SpaceSeparator;

            if (EnableTypewriterDebugLogs)
            {
                Debug.Log($"[TypewriterAudio] Character '{character}' (U+{(int)character:X4}) category={category} rendered={data.info.isRendered} visible={data.isVisible} skip={shouldSkip}");
            }

            return shouldSkip;
        }
    }
}
