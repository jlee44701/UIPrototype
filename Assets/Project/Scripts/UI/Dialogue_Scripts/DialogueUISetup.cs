using System;
using Audio;
using PixelEngine; 
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUISetup : MonoBehaviour {
        [SerializeField] UIDocument m_Doc;
        DialogueUIView m_View;
        DialogueUIPresenter m_Presenter;
        EventRegistry m_EventRegistry;
        VisualElement m_Root;
        
        [SerializeField] AudioParams.Pitch m_Pitch;
        [SerializeField] AudioParams.Repetition m_Repetition;
        [SerializeField] AudioParams.Randomization m_Randomization;
        [SerializeField] AudioParams.Distortion m_Distortion;
        
        void OnEnable() {
            Validate();
            
            if (!m_Doc) m_Doc = GetComponent<UIDocument>();
            m_Root = m_Doc.rootVisualElement ?? throw new ArgumentNullException(nameof(m_Root));
            
            
            m_View ??= new DialogueUIView(m_Root);
            
            m_Presenter ??= new DialogueUIPresenter(m_View);
            m_Presenter.OnEnable();
            
            return;
            void Validate() {
                NullRefChecker.Validate(this);
                if (!Coroutines.IsInitialized)
                    Coroutines.Initialize(this);
            }
        }
        
        
        
        void OnDisable() {
            m_Presenter?.OnDisable();
            m_View?.Dispose();
        }
        
    }
}
