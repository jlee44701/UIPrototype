using System;
using Audio;
using DG.Tweening;
using Game.Mine;
using PixelEngine;
using UIEvents;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VInspector;
using Names = PixelEngine.UIStrings.Runtime.Uxml.DustApostle.Names;

namespace Game.UI {
    [RequireComponent(typeof(UIDocument))]
    public class LecternUISetup : MonoBehaviour {
        [SerializeField] UIDocument _document;
        [Foldout("Assets")]
        [SerializeField] DustProphetSO _dustProphetSo;
        [Header("Data")]
        [SerializeField] LecternViewSO[] _viewData;
        [SerializeField] VisualTreeAsset _footerButtonAsset;
        [SerializeField] VisualTreeAsset _miningStatsAsset;
        [SerializeField] AudioClip _terminalTypingSound;
        [FormerlySerializedAs("bgm")]
        [SerializeField] AudioClip _bgm;
        [EndFoldout]
        
        
        [Foldout("Audio Settings")]
        [SerializeField] AudioParams.Pitch.Variation _pitchVariation;
        [SerializeField] AudioParams.Repetition
            _repetition;
        [SerializeField] AudioParams.Randomization _randomization;
        [SerializeField] AudioParams.Distortion _distortion;
        [Range(0,2)]
        [SerializeField] float _typingVolume = 1f;
        
        [EndFoldout]
        
        // Instance fields
        VisualElement _root;
        EventRegistry _eventRegistry;
        LecternUIPresenter _presenter;
        LecternUIView _lecternUiView;
        protected Coroutine DisplayRoutine;

        // Debug tween fields (easy to rip out after)
        Tween _testOrbitTween;
        float _testOrbitAngleRadians;

        public LecternUISetup(VisualElement root, AudioParams.Repetition repetition) {
            _root = root;
            _repetition = repetition;
        }

        [ContextMenu("test")]
        public void testTween() {
            if (!_document)
                _document = GetComponent<UIDocument>();

            if (!_document)
                return;

            _root ??= _document.rootVisualElement;
            if (_root == null)
                return;

            _root.usageHints |= UsageHints.DynamicTransform;

            if (_testOrbitTween != null) {
                _testOrbitTween.Kill();
                _testOrbitTween = null;
            }

            var orbitRadiusPixels = 15f;
            var secondsPerRevolution = 1.25f;

            _testOrbitAngleRadians = 0f;

            _testOrbitTween = DOTween.To(
                    () => _testOrbitAngleRadians,
                    valueRadians => {
                        _testOrbitAngleRadians = valueRadians;

                        var xPixels = Mathf.Cos(valueRadians) * orbitRadiusPixels;
                        var yPixels = Mathf.Sin(valueRadians) * orbitRadiusPixels;

                        _root.style.translate = new StyleTranslate(new Translate(xPixels, yPixels));
                    },
                    endValue: Mathf.PI * 2f,
                    duration: secondsPerRevolution)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        [ContextMenu("test_stop")]
        public void testTweenStop() {
            if (_testOrbitTween != null) {
                _testOrbitTween.Kill();
                _testOrbitTween = null;
            }

            if (_root != null)
                _root.style.translate = new StyleTranslate(new Translate(0, 0));
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
            
            _lecternUiView = new LecternUIView(root, _viewData, _dustProphetSo, _miningStatsAsset);
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
            t.TypewriterSound = _terminalTypingSound;
            t.SetAudioParams(
                _pitchVariation,
                _randomization,
                _repetition,
                _distortion,
                _typingVolume
                );
        }

        void OnFirstGeometryChanged(GeometryChangedEvent evt)
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);

            _root.schedule.Execute(() =>
            {
                _root.style.visibility = Visibility.Visible;
            });
        }

        void OnDisable() {
            if (_testOrbitTween != null) {
                _testOrbitTween.Kill();
                _testOrbitTween = null;
            }

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
