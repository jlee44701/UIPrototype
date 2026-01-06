using RuntimeUI.Story.Dialogue;
using UIEvents;
using UnityEngine;

namespace RuntimeUI {
    public class DialogueUIPresenter {
        DialogueUIView m_View;
        public DialogueUIPresenter(DialogueUIView view) {
            m_View = view;
        }

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

        }
        void UnregisterCallbacks() {
            DialogueEvents.DialogueSent -= ProcessDialogue;
        }

        async void ProcessDialogue(DialogueSequenceSO dialogueSequence) {
            var character = dialogueSequence.character;
            m_View.SetPortraitAndVoice(character.portrait, character.voice);
            await m_View.PlayLinesAsync(dialogueSequence.dialogue);
            //m_View.SetDialogue();
        }
    }
}
