using System;
using System.Collections.Generic;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI
{
    public sealed class DialogueUIView : IDisposable
    {
        const string PortraitQ = "dialogue-portrait-container";
        const string AnimatedTextQ = "animated-text";
        const string DialogueContainerQ = "dialogue-container";

        readonly VisualElement 
            m_Root, 
            m_DialoguePortraitContainer,
            m_DialogueContainer;

        AudioClip m_CurrentCharacterVoice;

        readonly AnimatedTextFieldElement m_AnimatedTextField;
        readonly TypewriterCore m_TypewriterCore;

        bool m_IsSubscribed;

        readonly AwaitableCompletionSource m_LineShownCompletionSource = new AwaitableCompletionSource();

        public AnimatedTextFieldElement AnimatedTextField => m_AnimatedTextField;

        public DialogueUIView(VisualElement root)
        {
            m_Root = root ?? throw new ArgumentNullException(nameof(root));

            m_DialoguePortraitContainer = m_Root.Q<VisualElement>(PortraitQ) ?? throw new NullReferenceException(nameof(m_DialoguePortraitContainer));
            
            m_DialogueContainer = m_Root.Q<VisualElement>(DialogueContainerQ) ?? throw new NullReferenceException(nameof(m_DialogueContainer));

            m_AnimatedTextField = m_Root.Q<AnimatedTextFieldElement>(AnimatedTextQ) ?? throw new NullReferenceException(nameof(m_AnimatedTextField));
            
            m_TypewriterCore = m_AnimatedTextField.AnimatedLabel.Typewriter ?? throw new NullReferenceException(nameof(m_TypewriterCore));

            m_TypewriterCore.OnTextShowed += HandleTextShowed;
            m_IsSubscribed = true;
        }

        public void Dispose()
        {
            if (m_IsSubscribed && m_TypewriterCore != null)
            {
                m_TypewriterCore.OnTextShowed -= HandleTextShowed;
                m_IsSubscribed = false;
            }
        }

        void HandleTextShowed()
        {
            m_LineShownCompletionSource.TrySetResult();
        }

        public async Awaitable PlayLinesAsync(IReadOnlyList<string> stringsList)
        {
            if (stringsList == null)
                throw new ArgumentNullException(nameof(stringsList));
            
            m_DialogueContainer.style.display = DisplayStyle.Flex;
            foreach (var line in stringsList)
            {
                m_LineShownCompletionSource.Reset();
                m_AnimatedTextField.Text = line;
                await m_LineShownCompletionSource.Awaitable;
            }
            //todo replace w/ constant or something
            await Awaitable.WaitForSecondsAsync(1.0f);
            
            m_DialogueContainer.style.display = DisplayStyle.None;
        }

        public void SetPortraitAndVoice(Texture2D image, AudioClip voice)
        {
            if (m_DialoguePortraitContainer != null)
                m_DialoguePortraitContainer.style.backgroundImage = new StyleBackground(image);
            m_AnimatedTextField.TypewriterSound = voice;
        }
    }
}
