using System.Collections.Generic;
using Game;
using PixelCrushers.DialogueSystem;
using UIEvents;
using UnityEngine;

namespace RuntimeUI.Story.Dialogue {
    [CreateAssetMenu(fileName = "DialogueSO",
        menuName = "Dialogue/DialogueSequence")]
    public class DialogueSequenceSO : ScriptableObject {
        [TextArea(3, 10)]
        public List<string> dialogue;
        public CharacterSO  character;
        
        [ContextMenu("Raise event")]
        public void RaiseEvent() {
            DialogueEvents.DialogueSent?.Invoke(this);
        }
    }
    [System.Serializable]
    public struct DialogueLine {
        public string line;
        public AudioClip voiceOverride;
    }
    public enum Delivery {
        Default,
    }
}

    