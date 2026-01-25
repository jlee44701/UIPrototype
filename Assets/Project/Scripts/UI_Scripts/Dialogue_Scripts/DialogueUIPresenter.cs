using System;
using Game.UI.Story.Dialogue;
using Events;
using Events.UI;
using Events.UI.Dialogue;
using UI;
using UI.Filters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using VInspector;

namespace Game.UI {
    public class DialogueUIPresenter : MonoBehaviour {

        public DialogueUIView View { get; set; }

        [SerializeField] PixelGlitchSweepParams _pixelGlitchSweepParams;
        
        [Foldout("CRT Filter Settings")]
        [SerializeField] [Range(0,1)] float _scanlineStrength = 0.005f;
        [SerializeField] [Range(0,10)] float _scanlineFrequency = 1;
        [SerializeField] [Range(0.0011f,1)] float _chromaticOffset = 0.0011f;
        [SerializeField] [Range(0,1)] float _curvature = 0.25f;
        
        [EndFoldout]
        
        readonly PixelGlitchEffectRunner _fx = new();
        FilterFunctionDefinition _crtFilter;

        public void OnEnable() {
            UnregisterCallbacks();
            RegisterCallbacks();
        }
        public void OnDisable() {
            UnregisterCallbacks();
        }
        void RegisterCallbacks() {
            UnregisterCallbacks();
            DialogueEvents.DialogueSent += ProcessDialogue;
            DialogueEvents.ShowDialogueUI += OnShowDialogue;
            DialogueEvents.HideDialogueUI += OnHideDialogueUI; 
            

        }
        void UnregisterCallbacks() {
            DialogueEvents.DialogueSent -= ProcessDialogue;
            DialogueEvents.ShowDialogueUI -= OnShowDialogue;
            DialogueEvents.HideDialogueUI -= OnHideDialogueUI;
        }

        public void OnShowDialogue() {
            if (View == null) throw new NullReferenceException(nameof(View));
            View.ShowDialogueUI();
        }
        public void OnHideDialogueUI() {
            if (View == null) throw new NullReferenceException(nameof(View));
            View.HideDialogueUI();
        }

        async void ProcessDialogue(DialogueSequenceSO dialogueSequence) {
            
            var character = dialogueSequence.character ?? throw new NullReferenceException(nameof(dialogueSequence.character));
            
            View.SetPortraitAndVoice(character.sprite, character.voice);
            //await m_View.PlayLinesAsync(dialogueSequence.dialogue);
            
            View.ShowDialogueUI();
            // m_View.ShowDialogueSequence(dialogueSequence.dialogue);
            await View.PlayLinesAsync(dialogueSequence.dialogue);
            
            if (dialogueSequence.hideWhenFinished) View.HideDialogueUI();
            
            //m_View.SetDialogue();
        }

        public void ApplyFilter_PixelGlitchSweep() {
            FilterEvents.ApplyPixelGlitchSweep?.Invoke(View.DialoguePortraitContainer, _pixelGlitchSweepParams);
            
            // _fx.PlayOnce(View.DialoguePortraitContainer, View.PixelGlitchFilter, _durationSeconds, _pixelSizePx, _amplitudePx, _directionDeg);
            // PixelGlitchFilterPlayer.PlayOnce(
            //     View.Root,
            //     View.PixelGlitchFilter,
            //     _durationSeconds,
            //     _pixelSizePx,
            //     _amplitudePx,
            //     _directionDeg
            // );
        }
        public void ApplyFilter_CRT(VisualElement element) {
            var filter = new FilterFunction();
            filter.AddParameter(new FilterParameter(_scanlineStrength));
            filter.AddParameter(new  FilterParameter(_scanlineFrequency));
            filter.AddParameter(new  FilterParameter(_chromaticOffset));
            filter.AddParameter(new  FilterParameter(_curvature));

            _fx.ApplyFilter(element, filter);

        }
        
    }
}
