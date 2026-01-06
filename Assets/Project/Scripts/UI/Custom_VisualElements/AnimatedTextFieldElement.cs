using System.Reflection;
using Audio;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI
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
        
        public float TypingVolume { get; set; }
        
        public AudioParams.Distortion Distortion { get; set; }
        public AudioParams.Pitch Pitch { get; set; }
        public AudioParams.Randomization Randomization { get; set; }
        public AudioParams.Repetition Repetition { get; set; }

        [UxmlAttribute("text")]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
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

        public AnimatedTextFieldElement()
        {
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                EnsureInitialized();
                TryPlayTypewriter();

            });


            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (m_AnimatedLabel == null)
                    return;

                m_AnimatedLabel.Typewriter.OnCharacterVisible -= CharacterVisible;
            });
        }

        public AnimatedTextFieldElement(TypingsTimingsScriptableBase timingsTimings) : this()
        {
            SetTimings(timingsTimings, restartIfTextAlreadySet: false);
        }

        void EnsureInitialized()
        {
            EnsureAnimatedLabelExists();
            if (m_AnimatedLabel == null)
                return;

            m_AnimatedLabel.Typewriter.OnCharacterVisible -= CharacterVisible;
            m_AnimatedLabel.Typewriter.OnCharacterVisible += CharacterVisible;
   
            EnsureTimingsAssigned();
        }

        void EnsureAnimatedLabelExists()
        {
            if (m_AnimatedLabel != null)
                return;

            m_AnimatedLabel = this.Q<AnimatedLabel>();

            if (m_AnimatedLabel != null)
                return;

            m_AnimatedLabel = new AnimatedLabel { name = AnimatedLabelElementName };
            hierarchy.Add(m_AnimatedLabel);
        }

        public void SetTimings(TypingsTimingsScriptableBase timingsTimings, bool restartIfTextAlreadySet = true)
        {
            m_TimingsOverride = timingsTimings;

            EnsureInitialized();

            if (restartIfTextAlreadySet)
                TryPlayTypewriter();
        }

        void EnsureTimingsAssigned()
        {
            var timingsToUse = m_TimingsOverride;

            if (!timingsToUse)
            {
                if (!m_DefaultTimings)
                    m_DefaultTimings = Resources.Load<TypingsTimingsScriptableBase>(DefaultTimingsResourcesPath);

                timingsToUse = m_DefaultTimings;
            }

            if (!timingsToUse || m_AnimatedLabel == null)
                return;

            TryAssignFirstTimingsSlot(m_AnimatedLabel, timingsToUse);
            TryAssignFirstTimingsSlot(m_AnimatedLabel.Typewriter, timingsToUse);
        }

        void CharacterVisible(CharacterData data)
        {
            if (!Application.isPlaying)
                return;

            if (!m_TypeWriterSound)
                return;

            var character = data.info.character;
            if (char.IsWhiteSpace(character) || !data.info.isRendered)
                return;

            var audioManager = AudioManager.Instance;
            if (!audioManager) return;

            audioManager.PlaySfx2D(
                m_TypeWriterSound, 
                Vector3.zero, 
                Pitch, 
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
            if (!Application.isPlaying)
                return;

            if (m_AnimatedLabel == null)
                return;

            if (string.IsNullOrEmpty(m_Text))
                return;

            m_AnimatedLabel.Typewriter.ShowText(m_Text);
            m_AnimatedLabel.Typewriter.StartShowingText(true);
        }
    }
}
