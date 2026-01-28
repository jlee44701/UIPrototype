using System;
using Game.UI;
using Game.UI.Story.Dialogue;
using UnityEngine;

namespace Events.UI.Dialogue {
    public static class DialogueEvents {
        public static Action<DialogueSequenceSO> DialogueSent;
        public static Action ShowDialogueUI;
        public static Action HideDialogueUI;
        public static Action DialogueSequenceFinished;
        public static Action<AudioClip> SoundPlayed;
         
    }
}
