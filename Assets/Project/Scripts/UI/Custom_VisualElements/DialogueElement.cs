using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
[assembly: UxmlNamespacePrefix("RuntimeUI", "rt")]
namespace RuntimeUI {
    [UxmlElement]
    public partial class DialogueElement : VisualElement {
        AnimatedTextField m_AnimatedTextField;
        
        [UxmlAttribute("text")]
        public string Text
        {
            get => m_AnimatedTextField.Text;
            set => m_AnimatedTextField.Text = value;
        }
        
    }

}