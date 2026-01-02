using UnityEngine;

namespace RuntimeUI {
    public class DialogueUIPresenter {
        DialogueUIView m_View;
        
        public DialogueUIPresenter(DialogueUIView view) {
            m_View = view;
        }

        public void OnEnable() {
            UnregisterEvents();
            RegisterEvents();
        }
        public void OnDisable() {
            UnregisterEvents();
        }
        void RegisterEvents() {
            
        }
        void UnregisterEvents() {
            
        }
    }
}
