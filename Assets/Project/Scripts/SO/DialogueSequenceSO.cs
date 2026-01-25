using System.Collections.Generic;
using Game;
using PixelCrushers.DialogueSystem;
using Events;
using Events.UI.Dialogue;
using UnityEngine;
using VInspector;

namespace Game.UI.Story.Dialogue {
    [CreateAssetMenu(fileName = "DialogueSO",
        menuName = "Dialogue/DialogueSequence")]
    public class DialogueSequenceSO : ScriptableObject {
        [SerializeField] public bool hideWhenFinished = true;
        
        [TextArea(3, 10)]
        public List<string> dialogue;
        public CharacterSO  character;
        
        [Button("Raise")]
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

    