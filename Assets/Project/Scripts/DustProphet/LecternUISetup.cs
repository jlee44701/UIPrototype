using System;
using PixelEngine;
using UIEvents;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Names = PixelEngine.UIStrings.Runtime.Uxml.DustApostle.Names;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class LecternUISetup : MonoBehaviour {

        [SerializeField] UIDocument m_Doc;
        LecternUIPresenter m_Presenter;
        LecternUIView m_LecternUiView;
        
        EventRegistry m_EventRegistry;

        VisualElement m_Root;
        [SerializeField] LecternViewSO[] m_ViewData;
        protected Coroutine m_DisplayRoutine;
        public AsyncOperationHandle<VisualTreeAsset> FooterButtonAssetHandle;
        public VisualTreeAsset footerButtonAsset;
        

        void OnEnable() {
            NullRefChecker.Validate(this);
            
            if (!m_Doc)
                m_Doc = GetComponent<UIDocument>();
            if (m_Presenter == null)
                m_Presenter = new LecternUIPresenter(m_ViewData);
            
            m_Presenter.OnEnable();
            
            // wait for layout to finish before displaying
            m_Root = m_Doc.rootVisualElement;
            // m_Root.style.visibility = Visibility.Hidden;
            // m_Root.RegisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);

            if (!Coroutines.IsInitialized)
            {
                Coroutines.Initialize(this);
            }
            
            var header = m_Doc.rootVisualElement.Q<VisualElement>(Names.LecternHeaderContainer);
            if (header == null)
                throw new NullReferenceException($"Missing VisualElement name '{Names.LecternHeaderContainer}' in document '{m_Doc.visualTreeAsset?.name}'.");


            var root = m_Doc.rootVisualElement.Q<VisualElement>(Names.LecternContainer);
            if (root == null)
                throw new NullReferenceException(nameof(root));
            
            m_LecternUiView = new LecternUIView(root, m_ViewData);
            m_LecternUiView.Initialize();
            m_LecternUiView.SetupFooter(m_ViewData.Length, footerButtonAsset);
            m_LecternUiView.RegisterCallbacks();
            //m_LecternUiView.NavigationBar.HighlightButton(0); // ?
            
            m_Presenter.LecternUiView = m_LecternUiView; 
            DustProphet.SetupComplete?.Invoke();
            
            RegisterCallbacks();
            m_LecternUiView.ShowRootWithDelay();
            //LoadAssetAndSetup();
            // DemoEvents.BackButtonClicked += DemoEvents_BackButtonClicked;
        }
        void OnFirstGeometryChanged(GeometryChangedEvent evt)
        {
            m_Root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);

            // Scheduler runs next frame; we show after layout has settled. :contentReference[oaicite:3]{index=3}
            m_Root.schedule.Execute(() =>
            {
                m_Root.style.visibility = Visibility.Visible;
            });
            
        }
        void OnDisable() {
            m_Root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);
            UnregisterCallbacks();
            
            m_Presenter.OnDisable();
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
