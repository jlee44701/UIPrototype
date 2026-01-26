using Codice.Client.Common.Connection;
using Game.UI.Story.Dialogue;
using Obvious.Soap;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "DialogueTester", menuName = "Scriptable Objects/DialogueTester")]
public class DialogueTester : ScriptableObject
{
    public DialogueSequenceSO  _dialogueSequence;
    public ScriptableEventBase _event1;
    public ScriptableEventBase _event2;
    public ScriptableEventBase _event3;
    public ScriptableEventBase _event4;
    public ScriptableEventBase _event5;
}
#endif
