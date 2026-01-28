using System;
using Audio;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Game.UI {
    public class TypewriterAudio : IDisposable{
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
            if (char.IsWhiteSpace(character) || !data.info.isRendered || !data.isVisible) return;
            
        
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
    }
}
