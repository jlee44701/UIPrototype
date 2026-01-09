using System;
using Game.UI;
using Game.UI.Story.Dialogue;
using UnityEngine;

namespace UIEvents {
    public static class DialogueEvents {
        public static Action<DialogueSequenceSO> DialogueSent;
        public static Action DialogueSequenceFinished;
        public static Action<AudioClip> SoundPlayed;
    }
}
