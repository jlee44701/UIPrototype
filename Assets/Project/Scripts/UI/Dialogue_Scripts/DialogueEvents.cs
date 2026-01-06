using System;
using RuntimeUI;
using RuntimeUI.Story.Dialogue;
using UnityEngine;

namespace UIEvents {
    public static class DialogueEvents {
        public static Action<DialogueSequenceSO> DialogueSent;
        public static Action<AudioClip> SoundPlayed;
    }
}
