// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace RuntimeUI {
//     public static class TypewriterEffects {
//         public static async Awaitable PlayAnimatedTextAsync(IReadOnlyList<string> stringsList, AwaitableCompletionSource completionSource, AnimatedTextFieldElement animatedTextElement)
//         {
//             if (stringsList == null)
//                 throw new ArgumentNullException(nameof(stringsList));
//             
//             foreach (var line in stringsList)
//             {
//                 m_LineShownCompletionSource.Reset();
//                 m_AnimatedTextField.Text = line;
//                 await m_LineShownCompletionSource.Awaitable;
//             }
//             //todo replace w/ constant or something
//             await Awaitable.WaitForSecondsAsync(1.0f);
//             
//             m_DialogueContainer.style.display = DisplayStyle.None;
//         }
//     }
// }
