using System;
using Audio;
using PixelEngine;
using UIEvents;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VInspector;
using Names = PixelEngine.UIStrings.Runtime.Uxml.DustApostle.Names;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class LecternUISetup : MonoBehaviour {
        [SerializeField] UIDocument _document;
        [Header("Data")]
        [SerializeField] LecternViewSO[] _viewData;
        
        [Foldout("Audio Settings")]
        [SerializeField] AudioParams.Pitch _pitch;
        [SerializeField] AudioParams.Repetition
            _repetition;
        [SerializeField] AudioParams.Randomization _randomization;
        [SerializeField] AudioParams.Distortion _distortion;
        [Range(0,2)]
        [SerializeField] float _typingVolume = 1f;
        
        [EndFoldout]
        
        [Header("Assets")]
        [SerializeField] VisualTreeAsset _footerButtonAsset;
        [SerializeField] AudioClip _terminalTypingSound;
        [FormerlySerializedAs("bgm")]
        [SerializeField] AudioClip _bgm;
        
        // Instance fields
        VisualElement _root;
        EventRegistry _eventRegistry;
        LecternUIPresenter _presenter;
        LecternUIView _lecternUiView;
        protected Coroutine DisplayRoutine;
        
        public LecternUISetup(VisualElement root, AudioParams.Repetition repetition) {
            _root = root;
            _repetition = repetition;
        }
        
        void OnEnable() {
            NullRefChecker.Validate(this);
            
            if (!_document)
                _document = GetComponent<UIDocument>();
            _presenter ??= new LecternUIPresenter(_viewData);
            
            _presenter.OnEnable();
            _root = _document.rootVisualElement;

            if (!Coroutines.IsInitialized)
                Coroutines.Initialize(this);
            
            VisualElement root = _document.rootVisualElement.Q<VisualElement>(Names.LecternContainer) ?? throw new NullReferenceException(nameof(root));
            
            _lecternUiView = new LecternUIView(root, _viewData);
            _lecternUiView.Initialize();
            _lecternUiView.SetupFooter(_viewData.Length, _footerButtonAsset);
            _lecternUiView.RegisterCallbacks();
            if (_terminalTypingSound) 
                SetAudioParams();
            
            if (_bgm)
                AudioManager.Instance.PlayMusic(_bgm);
            
            _presenter.LecternUiView = _lecternUiView; 
            DustProphet.SetupComplete?.Invoke();
            
            RegisterCallbacks();
            _lecternUiView.ShowRootWithDelay();
            //LoadAssetAndSetup();
            // DemoEvents.BackButtonClicked += DemoEvents_BackButtonClicked;
        }
        void SetAudioParams() {
            if (_lecternUiView == null) throw new NullReferenceException(nameof(_lecternUiView));

            var t = _lecternUiView.AnimatedTextField;
            t.Pitch = _pitch;
            t.TypewriterSound = _terminalTypingSound;
            t.TypingVolume = _typingVolume;
            t.Distortion = _distortion;
            t.Repetition = _repetition;
            t.Randomization = _randomization;
        }
        void OnFirstGeometryChanged(GeometryChangedEvent evt)
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);

            // Scheduler runs next frame; we show after layout has settled. :contentReference[oaicite:3]{index=3}
            _root.schedule.Execute(() =>
            {
                _root.style.visibility = Visibility.Visible;
            });
            
        }
        void OnDisable() {
            _root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);
            UnregisterCallbacks();
            
            _presenter.OnDisable();
        }
        

        void RegisterCallbacks() {
            UnregisterCallbacks();
            
            // DustProphet.FooterButtonClicked += OnFooterButtonClicked;
        }
        void UnregisterCallbacks() {
            // DustProphet.FooterButtonClicked -= OnFooterButtonClicked;
        }
        // void OnFooterButtonClicked(int index) {
        //     m_LecternUiView.DisplayStatusText("hi there newton nasd");
        // }

    }
}
