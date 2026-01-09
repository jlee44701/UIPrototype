using System.Reflection;
using Audio;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    [UxmlElement]
    public partial class AnimatedTextFieldElement : VisualElement
    {
        const string AnimatedLabelElementName = "animated-label";
        const string DefaultTimingsResourcesPath = "AnimatedText/Delays_By_Character";

        AnimatedLabel m_AnimatedLabel;
        AudioClip m_TypeWriterSound;
        TypingsTimingsScriptableBase m_DefaultTimings;
        TypingsTimingsScriptableBase m_TimingsOverride;
        string m_Text;

        bool m_IsAttachedToPanel;

        public float TypingVolume { get; set; }

        public AudioParams.Distortion Distortion { get; set; }
        AudioParams.Pitch m_Pitch = new AudioParams.Pitch(1);
        public AudioParams.Randomization Randomization { get; set; }
        public AudioParams.Repetition Repetition { get; set; }
        public AudioParams.Pitch.Variation PitchVariation { get; set; }

        [UxmlAttribute("text")]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                if (m_IsAttachedToPanel)
                    TryPlayTypewriter();
            }
        }

        [UxmlAttribute("typewriter-sound")]
        public AudioClip TypewriterSound
        {
            get => m_TypeWriterSound;
            set => m_TypeWriterSound = value;
        }

        public AnimatedLabel AnimatedLabel => m_AnimatedLabel;

        public void SetAudioParams(
            AudioParams.Pitch.Variation pitchVariation,
            AudioParams.Randomization randomization,
            AudioParams.Repetition repetition,
            AudioParams.Distortion distortion,
            float typingVolume)
        {
            PitchVariation = pitchVariation;
            Randomization = randomization;
            Repetition = repetition;
            Distortion = distortion;
            TypingVolume = typingVolume;
        }

        // MUST be public for UxmlElement instantiation.
        public AnimatedTextFieldElement()
        {
            m_IsAttachedToPanel = true;

            EnsureAnimatedLabelExists();
            EnsureTypewriterCallbackHooked();
            EnsureTimingsAssigned();
            TryPlayTypewriter();
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnDetachFromPanel(DetachFromPanelEvent detachFromPanelEvent)
        {
            m_IsAttachedToPanel = false;



            var typewriter = m_AnimatedLabel.Typewriter;


            typewriter.OnCharacterVisible -= CharacterVisible;
        }

        void EnsureAnimatedLabelExists()
        {
            if (m_AnimatedLabel != null)
                return;

            // Prefer a named lookup, then fallback to first match.
            m_AnimatedLabel = this.Q<AnimatedLabel>(AnimatedLabelElementName) ?? this.Q<AnimatedLabel>();

            if (m_AnimatedLabel != null)
                return;

            m_AnimatedLabel = new AnimatedLabel
            {
                name = AnimatedLabelElementName
            };

            hierarchy.Add(m_AnimatedLabel);
        }

        void EnsureTypewriterCallbackHooked()
        {
            var typewriter = m_AnimatedLabel.Typewriter;

            typewriter.OnCharacterVisible -= CharacterVisible;
            typewriter.OnCharacterVisible += CharacterVisible;
        }

        public void SetTimings(TypingsTimingsScriptableBase timingsTimings, bool restartIfTextAlreadySet = true)
        {
            m_TimingsOverride = timingsTimings;

            if (restartIfTextAlreadySet && m_IsAttachedToPanel)
                TryPlayTypewriter();
        }

        void EnsureTimingsAssigned() {
            if (m_AnimatedLabel.TimingSettings) return;
            var timingsToUse = m_TimingsOverride;

            if (!timingsToUse)
            {
                if (!m_DefaultTimings)
                    m_DefaultTimings = Resources.Load<TypingsTimingsScriptableBase>(DefaultTimingsResourcesPath);

                timingsToUse = m_DefaultTimings;
            }

            if (!timingsToUse) return;

            TryAssignFirstTimingsSlot(m_AnimatedLabel, timingsToUse);
            var typewriter = m_AnimatedLabel.Typewriter;
            TryAssignFirstTimingsSlot(typewriter, timingsToUse);
        }

        void CharacterVisible(CharacterData data)
        {
            if (!m_TypeWriterSound)
                return;

            var character = data.info.character;
            if (char.IsWhiteSpace(character) || !data.info.isRendered) return;

            var audioManager = AudioManager.Instance;
            if (!audioManager) return;

            audioManager.PlaySfx2D(
                m_TypeWriterSound,
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

        static void TryAssignFirstTimingsSlot(object targetObject, TypingsTimingsScriptableBase timingsAsset)
        {
            if (targetObject == null || !timingsAsset)
                return;

            var timingsBaseType = typeof(TypingsTimingsScriptableBase);
            var targetType = targetObject.GetType();

            foreach (var propertyInfo in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!propertyInfo.CanWrite)
                    continue;

                if (!timingsBaseType.IsAssignableFrom(propertyInfo.PropertyType))
                    continue;

                if (propertyInfo.GetValue(targetObject) is UnityEngine.Object existing && existing)
                    return;

                propertyInfo.SetValue(targetObject, timingsAsset);
                return;
            }

            foreach (var fieldInfo in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!timingsBaseType.IsAssignableFrom(fieldInfo.FieldType))
                    continue;

                if (fieldInfo.GetValue(targetObject) is UnityEngine.Object existing && existing)
                    return;

                fieldInfo.SetValue(targetObject, timingsAsset);
                return;
            }
        }
        void TryPlayTypewriter()
        {
            if (m_AnimatedLabel == null) return;

            var textToShow = m_Text ?? string.Empty;
            
            var typewriter = m_AnimatedLabel.Typewriter;
            
            typewriter.ShowText(textToShow);
            //typewriter.StartShowingText(true);
        }

    }
}
