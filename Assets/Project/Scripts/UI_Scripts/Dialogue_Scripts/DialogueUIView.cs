using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audio;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity;
using Events;
using Events.UI.Dialogue;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class DialogueUIView
    {
        const string PortraitQ = "portrait__container";
        const string PortraitImageQ = "portrait__image";
        const string DialogueContainerQ = "dialogue__container"; 
        const string AnimatedLabelQ = "animated-text__text";
        
        readonly VisualElement 
            m_Root, 
            m_DialoguePortraitContainer,
            m_PortraitImageElement,
            m_DialogueContainer;

        AnimatedLabel m_AnimatedLabel;
        AudioClip m_CurrentCharacterVoice;
        
        public AnimatedLabel AnimatedLabel => m_AnimatedLabel;
        public VisualElement Root => m_Root;
        public VisualElement  DialoguePortraitContainer => m_DialoguePortraitContainer;
        public VisualElement  DialogueContainer => m_DialogueContainer;
        
        public TypewriterAudio TypewriterAudio { get; set; }
        
        bool _isSubscribed;
        int _dialogueIndex;
        int _dialogueLength;

        readonly AwaitableCompletionSource m_LineShownCompletionSource = new AwaitableCompletionSource();
        
        List<string> DialogueLines { get; set; }
        public FilterFunctionDefinition PixelGlitchFilter { get; set;}

        public DialogueUIView(VisualElement root)
        {
            m_Root = root ?? throw new ArgumentNullException(nameof(root));

            m_DialoguePortraitContainer = m_Root.Q<VisualElement>(PortraitQ) ?? throw new NullReferenceException(nameof(m_DialoguePortraitContainer));
            m_PortraitImageElement = m_Root.Q<VisualElement>(PortraitImageQ);
            
            m_DialogueContainer = m_Root.Q<VisualElement>(DialogueContainerQ) ?? throw new NullReferenceException(nameof(m_DialogueContainer));

            
            m_AnimatedLabel = m_Root.Q<Febucci.TextAnimatorForUnity.AnimatedLabel>(AnimatedLabelQ) ?? throw new NullReferenceException(nameof(m_AnimatedLabel));
            
            m_AnimatedLabel.Typewriter.OnTextShowed += HandleTextShowed;
            _isSubscribed = true;
            
        }
        

        public void ShowDialogueUI() {
            m_DialogueContainer.style.opacity = 1; //defunct
        }
        void HandleTypewriterStart()
        {
            m_AnimatedLabel.style.opacity = 1;
        }
        public void HideDialogueUI() {
            m_DialogueContainer.style.opacity = 0; //defunct
        }

        void HandleTextShowed()
        {
            m_LineShownCompletionSource.TrySetResult();
        }
        public async Awaitable PlayLinesAsync(IReadOnlyList<string> stringsList)
        {
            if (stringsList == null)
                throw new ArgumentNullException(nameof(stringsList));
           
           
            m_LineShownCompletionSource.Reset();
            foreach (var line in stringsList)
            {
                await Awaitable.NextFrameAsync();

                m_LineShownCompletionSource.Reset();
                //m_AnimatedTextField.Text = line;
                //m_TypewriterCore.ShowText(line);
                
                m_AnimatedLabel.Typewriter.ShowText(line);
                //m_TypewriterCore.StartShowingText();
                
                await m_LineShownCompletionSource.Awaitable;
                
                // We resume outside UI Toolkit rendering and we start the next line at a frame start.
                await Awaitable.NextFrameAsync();
                //m_TypewriterCore.StopShowingText();
            }
            
        }

        public void SetPortraitAndVoice(Sprite sprite, AudioClip voice)
        {
            if (m_PortraitImageElement != null && sprite)
                m_PortraitImageElement.style.backgroundImage = new StyleBackground(sprite);
            
            TypewriterAudio.TypeWriterSound = voice; 
        }

    }
}
