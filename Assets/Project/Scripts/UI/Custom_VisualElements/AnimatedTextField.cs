using Febucci.TextAnimatorForUnity;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("RuntimeUI", "rt")]

namespace RuntimeUI
{ 
    [UxmlElement]
    public partial class AnimatedTextField : VisualElement
    {
        AnimatedLabel m_AnimatedLabel;
        string m_Text;

        [UxmlAttribute("text")]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                TryPlayTypewriter();
            }
        }

        public AnimatedTextField()
        {
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (!Application.isPlaying)
                    return;

                m_AnimatedLabel ??= this.Q<AnimatedLabel>("animated-label");
                TryPlayTypewriter();
            });
        }

        void TryPlayTypewriter()
        {
            if (m_AnimatedLabel == null)
                return;

            if (string.IsNullOrEmpty(m_Text))
                return;

            // Febucci recommends driving typewriter via ShowText. :contentReference[oaicite:2]{index=2}
            m_AnimatedLabel.Typewriter.ShowText(m_Text);
            

            // If Start Typewriter Mode is FromScriptOnly, we must start it explicitly. :contentReference[oaicite:3]{index=3}
            m_AnimatedLabel.Typewriter.StartShowingText(true);
        }
    }
}
