using System;
using Audio;
using PixelEngine;
using RuntimeUI.Story.Dialogue;
using UIEvents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VInspector;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUISetup : MonoBehaviour {
        [FormerlySerializedAs("m_Doc")]
        [SerializeField] UIDocument _doc;
        
        [Foldout("Audio Settings")]
        [SerializeField] AudioParams.Pitch.Variation _pitchVariation;
        [SerializeField] AudioParams.Repetition
            _repetition;
        [SerializeField] AudioParams.Randomization _randomization;
        [SerializeField] AudioParams.Distortion _distortion;
        [Range(0,2)]
        [SerializeField] float _typingVolume = 1f;
        [EndFoldout]
        
        DialogueUIView _view;
        DialogueUIPresenter _presenter;
        EventRegistry _eventRegistry;
        VisualElement _root;
[SerializeField]
        public string message ="test clip";
        public AudioClip testClip; 
        void OnEnable() {
            ValidateRequired();
            
            if (!_doc) _doc = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement ?? throw new ArgumentNullException(nameof(_root));
            
            _view ??= new DialogueUIView(_root);
            _view.AnimatedTextField.SetAudioParams(
                _pitchVariation,
                _randomization,
                _repetition,
                _distortion,
                _typingVolume
                );
            
            _presenter ??= new DialogueUIPresenter(_view);
            _presenter.OnEnable();
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
            _view?.Dispose();
            _view = null;
            _presenter = null;
            
            //TEST
            DialogueEvents.DialogueSent -= Test;
        }
        
        void Test(DialogueSequenceSO d) {
            testClip = d.character.voice;
        }
        [ContextMenu("Clear text")]
        void ClearText() {
            _view.AnimatedTextField.AnimatedLabel.text = "";
        }
[ContextMenu("run line single test")]
        async void RunLine() {
            _view.SetPortraitAndVoice(null, testClip);
            await _view.PlayLineAsync(message);
        }
        

        
    }
}
