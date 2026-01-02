using System;
using PixelEngine;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class DialogueUISetup : MonoBehaviour {
        [SerializeField] UIDocument m_Doc;
        DialogueUIView m_View;
        DialogueUIPresenter m_Presenter;
        EventRegistry m_EventRegistry;
        VisualElement m_Root;

        void OnEnable() {
            NullRefChecker.Validate(this);
            
            if (!m_Doc)
                m_Doc = GetComponent<UIDocument>();
            if (m_Presenter == null)
                m_Presenter = new DialogueUIPresenter();
            if (m_View == null)
                m_View = new DialogueUIView();
            
            m_Root = m_Doc.rootVisualElement;
            if (!Coroutines.IsInitialized)
                Coroutines.Initialize(this);
            
            
        }
        void OnDisable() {
            
        }
        
    }
}
