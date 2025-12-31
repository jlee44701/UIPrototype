// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UIEvents;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace RuntimeUI {
//     /// <summary>
//     /// A SequenceManager controls the overall flow of the application using a state machine.
//     /// 
//     /// Use this class to define how each State will transition to the next. Each state can
//     /// transition to the next state when receiving an event or reaching a specific condition.
//     ///
//     /// Note: this class currently is only used for demonstration/diagnostic purposes. You can use
//     ///       the start and end of each state to instantiate GameObjects/play effects. Another simple
//     ///       state machine for UI screens (UIManager) actually drives most of the quiz gameplay.
//     /// 
//     /// </summary>
//     // todo inherit from generic
//     public class LecternUIManager : MonoBehaviour {
//         const string k_FooterButtonPath = "Assets/Project/UI/Runtime/Uxml/DustApostle/FooterButton.uxml";
//         [Tooltip("Required UI Document")]
//         [SerializeField] UIDocument m_Document;
//         
//         LecternBaseView m_DrillingView;
//         
//         LecternBaseView m_CurrentView;
//         
//         LecternUI m_LecternUI;
//
//         //todo replace with statics
//         const string 
//             StatusBarQ = "left-bay-status-bar__container",
//             TerminalQ = "left-bay__terminal__container",
//             ViewportQ = "left-bay__viewport__container",
//             RightBayQ = "right-bay__container",
//             FooterQ = "lectern__footer__container"
//             ;
//         
//         
//         VisualElement 
//             m_StatusBarContainer,
//             m_TerminalContainer,
//             m_ViewportContainer,
//             m_RightBayContainer, // placeholder
//             m_FooterContainer;
//         
//         NavigationBar m_NavigationBar;
//
//         // A list of all Views to show/hide
//         List<LecternBaseView> m_Views = new List<LecternBaseView>();
//         
//         public LecternBaseView CurrentView => m_CurrentView;
//         public UIDocument Document => m_Document;
//
//         // Register event listeners to game events
//         private void OnEnable() {
//             SubscribeToEvents();
//
//             // Because non-MonoBehaviours can't run coroutines, the Coroutines helper utility allows us to
//             // designate a MonoBehaviour to manage starting/stopping coroutines
//             Coroutines.Initialize(this);
//
//             Initialize();
//         }
//
//         // Unregister the listeners to prevent errors
//         private void OnDisable() {
//             UnsubscribeFromEvents();
//         }
//         private void Initialize() {
//             NullRefChecker.Validate(this);
//             VisualElement root = m_Document.rootVisualElement;
//             
//             //m_StatusBarContainer = new 
//             // m_DrillingView = new DrillingView(root.Q<VisualElement>(dril))
//
//             SetupNavigationBar(root);
//             
//             
//             // //todo replace with static strings
//             // m_DrillingView = new DrillingView(root.Q<VisualElement>("drilling__container"));
//
//             RegisterViews();
//             HideScreens();
//         }
//
//         private void SubscribeToEvents() {
//             // Wait for the SplashScreen to finish loading then load the StartScreen
//             DustProphet.DrillingViewShown +=
//                 DustProphetEvents_DrillingViewShown;
//         }
//
//         private void UnsubscribeFromEvents() {
//
//             DustProphet.DrillingViewShown -=
//                 DustProphetEvents_DrillingViewShown;
//         }
//
//         
//         private void DustProphetEvents_DrillingViewShown() {
//             m_CurrentView = m_DrillingView;
//             Show(m_DrillingView);
//         }
//         
//
//         // Clears history and hides all Views except the Start Screen
//         void SetupNavigationBar(VisualElement root) {
//             var footerContainer = root.Q<VisualElement>("footer__container");
//             
//             m_NavigationBar = new NavigationBar();
//             m_NavigationBar.Initialize(
//                 footerContainer,
//                 4,
//                 k_FooterButtonPath,
//                 true
//                 );
//         }
//         // Store each UIScreen into a master list so we can hide all of them easily.
//         private void RegisterViews() {
//             m_Views = new List<LecternBaseView> {
//                 m_DrillingView
//             };
//         }
//
//         // Clear history and hide all Views
//         private void HideScreens() {
//             foreach (LecternBaseView view in m_Views) {
//                 view.Hide();
//             }
//         }
//
//         // Shows a View of a specific type T, with the option to add it
//         // to the history stack
//         public void Show<T>() where T : LecternBaseView {
//             foreach (var view in m_Views) {
//                 if (view is T) {
//                     Show(view);
//                     break;
//                 }
//             }
//         }
//
//         // 
//         // Shows a UIScreen with the keepInHistory always enabled
//         public void Show(LecternBaseView view) {
//             view.Show();
//         }
//
//         private void ButtonClickHandler(int index, ClickEvent evt) {
//             DustProphet.DrillingButtonClicked?.Invoke(index);
//         }
//     }
// }