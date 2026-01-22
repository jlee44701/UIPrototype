using System;
using System.Collections.Generic;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity;
using UIEvents;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class DialogueUIView : IDisposable
    {
        const string PortraitQ = "portrait__container";
        const string PortraitImageQ = "portrait__image";
        const string AnimatedTextQ = "text-area__animated-text";
        const string DialogueContainerQ = "dialogue__container";
 
        readonly VisualElement 
            m_Root, 
            m_DialoguePortraitContainer,
            m_PortraitImageElement,
            m_DialogueContainer;

        AudioClip m_CurrentCharacterVoice;

        readonly AnimatedTextFieldElement m_AnimatedTextField;
        readonly TypewriterCore m_TypewriterCore;


        bool _isSubscribed;
        int _dialogueIndex;
        int _dialogueLength;

        readonly AwaitableCompletionSource m_LineShownCompletionSource = new AwaitableCompletionSource();
        public AnimatedTextFieldElement AnimatedTextField => m_AnimatedTextField;
        
        List<string> DialogueLines { get; set; }
        

        public DialogueUIView(VisualElement root)
        {
            m_Root = root ?? throw new ArgumentNullException(nameof(root));

            m_DialoguePortraitContainer = m_Root.Q<VisualElement>(PortraitQ) ?? throw new NullReferenceException(nameof(m_DialoguePortraitContainer));
            m_PortraitImageElement = m_Root.Q<VisualElement>(PortraitImageQ);
            
            m_DialogueContainer = m_Root.Q<VisualElement>(DialogueContainerQ) ?? throw new NullReferenceException(nameof(m_DialogueContainer));

            m_AnimatedTextField = m_Root.Q<AnimatedTextFieldElement>(AnimatedTextQ) ?? throw new NullReferenceException(nameof(m_AnimatedTextField));
            
            m_TypewriterCore = m_AnimatedTextField.AnimatedLabel.Typewriter ?? throw new NullReferenceException(nameof(m_TypewriterCore));

            m_TypewriterCore.OnTextShowed -= HandleTextShowed;
            m_TypewriterCore.OnTextShowed += HandleTextShowed;
            m_TypewriterCore.OnTextShowed -= ShowNextDialogue;
            m_TypewriterCore.OnTextShowed += ShowNextDialogue;
            _isSubscribed = true;
            
        }

        public void ShowDialogueUI() {
            m_DialogueContainer.style.opacity = 1;

        }

        public void HideDialogueUI() {
            m_DialogueContainer.style.opacity = 0;
        }
        public void Dispose()
        {
            if (_isSubscribed && m_TypewriterCore != null)
            {
                m_TypewriterCore.OnTextShowed -= HandleTextShowed;
                _isSubscribed = false;
            }
            if (m_TypewriterCore != null)
                m_TypewriterCore.OnTextShowed -= ShowNextDialogue;
        }

        void HandleTextShowed()
        {
            m_LineShownCompletionSource.TrySetResult();
        }


        public void ShowDialogueSequence(List<string> stringsList) {
            if (stringsList == null)
                throw new NullReferenceException(nameof(stringsList));
            
            _dialogueLength = stringsList.Count;
            _dialogueIndex = 0;
            DialogueLines = stringsList;
            m_AnimatedTextField.Text = stringsList[_dialogueIndex];
        }

        void ShowNextDialogue() {
            if (_dialogueIndex < _dialogueLength) {
                m_AnimatedTextField.AnimatedLabel.Typewriter.ShowText(DialogueLines[_dialogueIndex++]);
            }
            else {
                DialogueEvents.DialogueSequenceFinished?.Invoke();
            }
        }


        public async Awaitable PlayLineAsync(string line) {
            m_DialogueContainer.style.opacity = 1;

            // We let UI Toolkit do a layout/repaint pass before we start the typewriter + audio.
            await Awaitable.NextFrameAsync();
            
            m_TypewriterCore.SkipTypewriter();

            m_LineShownCompletionSource.Reset();
            m_AnimatedTextField.Text = line;
            
            // We resume outside UI Toolkit rendering and we start the next line at a frame start.
            await Awaitable.NextFrameAsync();
        }
        public async Awaitable PlayLinesAsync(IReadOnlyList<string> stringsList)
        {
            if (stringsList == null)
                throw new ArgumentNullException(nameof(stringsList));
            
            
            
            // We let UI Toolkit do a layout/repaint pass before we start the typewriter + audio.
            //await Awaitable.NextFrameAsync();
            m_LineShownCompletionSource.Reset();
            foreach (var line in stringsList)
            {
                //m_TypewriterCore.SkipTypewriter();
                m_TypewriterCore.StopShowingText();

                m_LineShownCompletionSource.Reset();
                //m_AnimatedTextField.Text = line;
                m_TypewriterCore.ShowText(line);
                //m_TypewriterCore.StartShowingText();
                
                
                await m_LineShownCompletionSource.Awaitable;
                
                // We resume outside UI Toolkit rendering and we start the next line at a frame start.
                await Awaitable.NextFrameAsync();
                //m_TypewriterCore.StopShowingText();
            }

            //await Awaitable.WaitForSecondsAsync(1.0f);
            //await Awaitable.NextFrameAsync() ;
            
            
        }

        public void SetPortraitAndVoice(Sprite sprite, AudioClip voice)
        {
            if (m_PortraitImageElement != null && sprite)
                m_PortraitImageElement.style.backgroundImage = new StyleBackground(sprite);
            m_AnimatedTextField.TypewriterSound = voice;
        }
    }
}
