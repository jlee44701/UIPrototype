// using Febucci.TextAnimatorCore.Text;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace RuntimeUI {
//     [UxmlElement]
//     public partial class DialogueElement : VisualElement {
//         AnimatedTextFieldElement m_AnimatedTextFieldElement;
//         AudioSource m_TypewriterAudioSource;
//         VisualElement PortraitImage => this.Q("portrait");
//
//         string m_Text;
//
//         AudioSource m_TypeWriterSound;
//         [UxmlAttribute("text")]
//         public string Text
//         {
//             get => m_AnimatedTextFieldElement.Text;
//             set => m_AnimatedTextFieldElement.Text = value;
//         }
//
//         public DialogueElement() {
//
//             // RegisterCallback<AttachToPanelEvent>(_ => {
//             //         m_AnimatedTextFieldElement = new AnimatedTextFieldElement();
//             //         RegisterCallback<DetachFromPanelEvent>(_ => {
//             //             if (m_AnimatedTextFieldElement == null) return;
//             //             Subscribe();
//             //         });
//             //     }
//             //     
//             // );
//         }
//
//         // void Subscribe() {
//         //     if (m_AnimatedTextFieldElement?.AnimatedLabel == null) return;
//         //     m_AnimatedTextFieldElement.AnimatedLabel.Typewriter
//         //         .OnCharacterVisible += PlayTypeWriterSound;
//         //     m_AnimatedTextFieldElement.AnimatedLabel.Typewriter.OnTextShowed += TextShown;
//         // }
//         // void Unsubscribe() {
//         //     if (m_AnimatedTextFieldElement?.AnimatedLabel == null) return;
//         //     
//         //     m_AnimatedTextFieldElement.AnimatedLabel.Typewriter
//         //         .OnCharacterVisible -= PlayTypeWriterSound;
//         //     m_AnimatedTextFieldElement.AnimatedLabel.Typewriter.OnTextShowed -= TextShown;
//         // }
//
//
//         void TextShown() {
//             
//         }
//         
//
//         
//
//     }
// }