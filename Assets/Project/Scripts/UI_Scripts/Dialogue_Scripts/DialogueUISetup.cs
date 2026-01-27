using System;
using Audio;
using Game.UI.Story.Dialogue;
using PixelEngine;
using Events;
using Events.UI.Dialogue;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VInspector;

namespace Game.UI {
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUISetup : MonoBehaviour {
        [FormerlySerializedAs("m_Doc")]
        [SerializeField] UIDocument _doc;
        
        [Header("Filters")]
        [SerializeField] FilterFunctionDefinition _pixelGlitchFilter;

        
        [Foldout("Audio Settings")]
        [SerializeField] AudioParams.Pitch.Variation _pitchVariation;
        [SerializeField] AudioParams.Repetition
            _repetition;
        [SerializeField] AudioParams.Randomization _randomization;
        [SerializeField] AudioParams.Distortion _distortion;
        [Range(0, 2)]
        [SerializeField] float _typingVolume = 1f;
        [EndFoldout]
        DialogueUIView _view;
        DialogueUIPresenter _presenter;
        EventRegistry _eventRegistry;
        VisualElement _root;
        [SerializeField]
        public string message = "test clip";
        public AudioClip testClip;
        void OnEnable() {
            ValidateRequired();

            if (!_doc) _doc = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement ?? throw new ArgumentNullException(nameof(_root));

            _view ??= new DialogueUIView(_root);
            

            _view.TypewriterAudio = new TypewriterAudio(_pitchVariation, _randomization, _repetition, _distortion, _typingVolume, _view.AnimatedLabel.Typewriter);
            
            if (_pixelGlitchFilter) {
                _view.PixelGlitchFilter = _pixelGlitchFilter;
            }
            
            if (!_presenter) _presenter = GetComponent<DialogueUIPresenter>();
            _presenter.View = _view;

           //_presenter ??= new DialogueUIPresenter(_view);

            //TEST
            DialogueEvents.DialogueSent += Test;

            return;

            void ValidateRequired() {
                NullRefChecker.Validate(this);
                if (!Coroutines.IsInitialized)
                    Coroutines.Initialize(this);
            }
        }
        
        void OnDisable() {
            _presenter?.OnDisable();
            _view = null;
            _presenter = null;

            //TEST
            DialogueEvents.DialogueSent -= Test;
        }

        void Test(DialogueSequenceSO d) {
            testClip = d.character.voice;
        }





    }
}
