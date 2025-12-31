using System;
using System.Collections;
using System.Collections.Generic;
using PixelEngine;
using UIEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Names = PixelEngine.UIStrings.Runtime.Uxml.DustApostle.Names;

namespace RuntimeUI {
    public class LecternUIView {
        UIDocument m_Document;

        LecternBaseView m_DrillingView;

        LecternBaseView m_CurrentView;

        EventRegistry m_EventRegistry;

        LecternViewSO[] m_ViewData;

        protected Coroutine m_DisplayRoutine;
        
        const string
            HeaderQ = Names.LecternHeaderContainer,
            StatusBarQ = Names.LeftBayStatusBarContainer,
            TerminalQ = Names.LeftBayTerminalCol,
            ViewportQ = Names.LeftBayViewportCol,
            RightBayQ = Names.RightBayContainer,
            FooterQ = Names.LecternButtonContainer,
            AnimatedStatusQ = "status-text-field";
        
        readonly VisualElement
            m_Root,
            m_HeaderContainer,
            m_StatusBarContainer,
            m_TerminalContainer,
            m_ViewportContainer,
            m_RightBayContainer, // placeholder
            m_FooterContainer;
        readonly AnimatedTextField 
            m_AnimatedStatusLabel;
            
 
        NavigationBar m_NavigationBar;

        public NavigationBar NavigationBar => m_NavigationBar;

        // A list of all Views to show/hide
        List<LecternBaseView> m_Views = new List<LecternBaseView>();

        public LecternBaseView CurrentView => m_CurrentView;
        public UIDocument Document => m_Document;

        public LecternUIView(VisualElement parentElement, LecternViewSO[] data) {

            m_Root = parentElement
                     ?? throw new ArgumentNullException(nameof(parentElement));

            m_ViewData = data;

            m_HeaderContainer =
                m_Root.Q<VisualElement>(HeaderQ)
                ?? throw new NullReferenceException(nameof(m_HeaderContainer));
            m_StatusBarContainer =
                m_Root.Q<VisualElement>(StatusBarQ)
                ?? throw new NullReferenceException(
                    nameof(m_StatusBarContainer));
            m_TerminalContainer =
                m_Root.Q<VisualElement>(TerminalQ)
                ?? throw new NullReferenceException(
                    nameof(m_TerminalContainer));
            m_ViewportContainer =
                m_Root.Q<VisualElement>(ViewportQ)
                ?? throw new NullReferenceException(
                    nameof(m_ViewportContainer));
            m_RightBayContainer =
                m_Root.Q<VisualElement>(RightBayQ)
                ?? throw new NullReferenceException(
                    nameof(m_RightBayContainer));
            m_FooterContainer =
                m_Root.Q<VisualElement>(FooterQ)
                ?? throw new NullReferenceException(nameof(m_FooterContainer));
            m_AnimatedStatusLabel = m_Root.Q<AnimatedTextField>(AnimatedStatusQ)
                                    ?? throw new NullReferenceException(
                                        nameof(m_AnimatedStatusLabel));

        }

        public void Initialize() {

            HideImmediately();
            m_EventRegistry = new EventRegistry();
            // m_EventRegistry.RegisterCallback<TransitionEndEvent>(m_RootElement, ParentElement_TransitionEnd);
            
            // //todo replace with static strings
            // m_DrillingView = new DrillingView(root.Q<VisualElement>("drilling__container"));

            RegisterViews();
            //HideScreens();
        }

        public void PaintViewportBackground() {
            //m_ViewportContainer.
        }

        private void SubscribeToEvents() {
            // Wait for the SplashScreen to finish loading then load the StartScreen
            DustProphet.DrillingViewShown +=
                DustProphetEvents_DrillingViewShown;
        }

        private void UnsubscribeFromEvents() {

            DustProphet.DrillingViewShown -=
                DustProphetEvents_DrillingViewShown;
        }

        public void DisplayStatusText(string text) {
            m_AnimatedStatusLabel.Text = text;
        }


        private void DustProphetEvents_DrillingViewShown() {
            m_CurrentView = m_DrillingView;
            Show(m_DrillingView);
        }

        public void SetupHeader() {
            
        }
        public void SetupFooter(int numberOfButtons, VisualTreeAsset footerAsset) {

            m_NavigationBar = new NavigationBar();
            m_NavigationBar.Initialize(
                m_Root,
                numberOfButtons,
                footerAsset,
                FooterQ,
                false
            );
        }
        public void RegisterCallbacks() {

            for (var i = 0; i < m_ViewData.Length; i++) {
                int index = i; // Closure capture

                NavigationBar.SetButtonLabelTextAtIndex(index,
                    m_ViewData[index].Title);

                var button = NavigationBar.Buttons[index];

                m_EventRegistry.RegisterCallback<ClickEvent>(button,
                    evt => ButtonClickHandler(index, evt));
                m_EventRegistry.RegisterCallback<MouseEnterEvent>(button, MouseEnterHandler);
                m_EventRegistry.RegisterCallback<MouseLeaveEvent>(button, MouseLeaveHandler);
            }
        }



        // Store each UIScreen into a master list so we can hide all of them easily.
        private void RegisterViews() {
            m_Views = new List<LecternBaseView> {
                m_DrillingView
            };
        }

        // Clear history and hide all Views
        private void HideScreens() {
            foreach (LecternBaseView view in m_Views) {
                view.Hide();
            }
        }

        // Shows a View of a specific type T, with the option to add it
        // to the history stack
        public void Show<T>() where T : LecternBaseView {
            foreach (var view in m_Views) {
                if (view is T) {
                    Show(view);
                    break;
                }
            }
        }
        

        // 
        // Shows a UIScreen with the keepInHistory always enabled
        public void Show(LecternBaseView view) {
            view.Show();
        }
        public IEnumerator ShowRootWithDelay(float delayInSecs) {
            yield return new WaitForSeconds(delayInSecs);
            
            m_Root.style.display = DisplayStyle.Flex;

            // // if (m_UseTransition)
            // // {
            //     // m_RootElement.AddToClassList(k_VisibleClass); // Add visible class
            //     m_Root.BringToFront();
            //     // m_RootElement.RemoveFromClassList(k_HiddenClass); // Remove hidden class
            // // }
        }

        public void ShowRootWithDelay() {
            Coroutines.StopCoroutine(ref m_DisplayRoutine);
            m_DisplayRoutine = Coroutines.StartCoroutine(ShowRootWithDelay(0.1f));
        }
        
        

        private void ButtonClickHandler(int index, ClickEvent evt) {
            DustProphet.FooterButtonClicked?.Invoke(index);
        }
        private void MouseEnterHandler(MouseEnterEvent evt) {
            Button hoverOverButton = evt.target as Button;

            if (hoverOverButton != null) {
                // If we are hovering the mouse over a Button in the NavigationBar, check the button's index
                int index = NavigationBar.Buttons.IndexOf(hoverOverButton);

                // If index is valid, highlight the button and pass the index to an event
                if (index != -1 && index < m_ViewData.Length) {
                    // NavigationBar.HighlightButton(index);

                    DustProphet.FooterButtonEntered?.Invoke(index);
                }
            }
        }
        void MouseLeaveHandler(MouseLeaveEvent evt) {
            Button buttonLeave = evt.target as Button;
        }
        public void HideImmediately()
        {
            m_Root.style.display = DisplayStyle.None;
        }
    }
}
