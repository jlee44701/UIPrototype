using Febucci.TextAnimatorCore.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI
{
    [UxmlElement]
    public partial class DialogueElement : VisualElement
    {
        AnimatedTextFieldElement m_AnimatedTextFieldElement;
        AudioSource m_TypewriterAudioSource;
        string m_Text;

        [UxmlAttribute("text")]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;

                if (m_AnimatedTextFieldElement != null)
                    m_AnimatedTextFieldElement.Text = m_Text;
            }
        }

        public DialogueElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public void SetTypewriterAudioSource(AudioSource typewriterAudioSource)
        {
            m_TypewriterAudioSource = typewriterAudioSource;
        }

        void OnAttachedToPanel(AttachToPanelEvent attachToPanelEvent)
        {
            if (m_AnimatedTextFieldElement == null)
            {
                m_AnimatedTextFieldElement = new AnimatedTextFieldElement();
                Add(m_AnimatedTextFieldElement);
                m_AnimatedTextFieldElement.Text = m_Text;
            }
            else if (m_AnimatedTextFieldElement.parent != this)
            {
                Add(m_AnimatedTextFieldElement);
            }

            if (!Application.isPlaying)
                return;

            Subscribe();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent detachFromPanelEvent)
        {
            if (!Application.isPlaying)
                return;

            Unsubscribe();
        }

        void Subscribe()
        {
            var typewriter = m_AnimatedTextFieldElement?.AnimatedLabel?.Typewriter;
            if (typewriter == null)
                return;

            Unsubscribe();

            typewriter.OnCharacterVisible += PlayTypewriterSound;
            typewriter.OnTextShowed += TextShown;
        }

        void Unsubscribe()
        {
            var typewriter = m_AnimatedTextFieldElement?.AnimatedLabel?.Typewriter;
            if (typewriter == null)
                return;

            typewriter.OnCharacterVisible -= PlayTypewriterSound;
            typewriter.OnTextShowed -= TextShown;
        }

        void TextShown()
        {
        }

        void PlayTypewriterSound(CharacterData characterData)
        {
            var visibleCharacter = characterData.info.character;

            if (char.IsWhiteSpace(visibleCharacter) || !characterData.info.isRendered)
                return;

            if (m_TypewriterAudioSource)
                m_TypewriterAudioSource.Play();
        }
    }
}