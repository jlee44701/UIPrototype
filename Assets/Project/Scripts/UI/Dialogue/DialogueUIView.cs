using System;
using UnityEngine.UIElements;

namespace RuntimeUI {
    public class DialogueUIView {
        const string
            A = "",
            B = "";
        readonly VisualElement
            m_Root;
        public DialogueUIView(VisualElement root) {
            m_Root = root ?? throw new ArgumentNullException(nameof(root));
            
        }
    }
}
