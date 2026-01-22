using System;
using Game.UI.Story.Dialogue;
using UIEvents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI {
    public class DialogueUIPresenter : MonoBehaviour {

        public DialogueUIView View { get; set; }

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
        
    }
}
