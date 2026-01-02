using System;
using System.Collections;
using PixelEngine;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI {
    public abstract class LecternBaseView {
        
        //todo replace with statics
        const string
            StatusBarQ = "left-bay-status-bar__container",
            TerminalQ = "left-bay__terminal__container",
            ViewportQ = "left-bay__viewport__container",
            RightBayQ = "right-bay__container";
        
        public const string k_VisibleClass = "screen-visible";
        public const string k_HiddenClass = "screen-hidden";
        
        #region Inspector fields
        protected bool m_HideOnAwake = true;

        // Is the UI partially see-through? (i.e. use overlay effect)
        protected bool m_IsTransparent;

        // Use USS style classes to transition to the next View
        protected bool m_UseTransition = true;
        protected float m_TransitionDelay = 0.1f;

        // The topmost visual element of the screen (this often is the rootVisualElement
        // but can be a different element if necessary)
        protected VisualElement m_RootElement;
        protected EventRegistry m_EventRegistry;
        
        protected VisualElement 
            m_StatusBarContainer,
            m_TerminalContainer,
            m_ViewportContainer,
            m_RightBayContainer
            ;
        
        // Used by helper class to hide/show with a delay
        protected Coroutine m_DisplayRoutine;
        #endregion

        #region Properties
        public VisualElement ParentElement => m_RootElement;

        public bool IsTransparent => m_IsTransparent;
        public bool IsHidden => m_RootElement.style.display == DisplayStyle.None;
        #endregion

        // Constructor
        public LecternBaseView(
            VisualElement parentElement
        )
        {
            // add parameter: footer, use navigation bar pattern to figure out how to manage the click events. maybe just subscribe to the button specific to this view which i can specify 
            //add button container to footer for navigation bar
            //
            
            // Required topmost VisualElement 
            m_RootElement = parentElement ?? throw new ArgumentNullException(nameof(parentElement));
            
            Initialize();
        }

        #region Methods
        // Registers a callback, TransitionEndEvent, on the m_ParentElement; creates a default EventRegistry
        public virtual void Initialize() {
            
            m_EventRegistry = new EventRegistry();
            m_EventRegistry.RegisterCallback<TransitionEndEvent>(m_RootElement, ParentElement_TransitionEnd);

            
            m_StatusBarContainer = m_RootElement.Q<VisualElement>(StatusBarQ);
            m_TerminalContainer = m_RootElement.Q<VisualElement>(TerminalQ);
            m_ViewportContainer = m_RootElement.Q<VisualElement>(ViewportQ);
            m_RightBayContainer = m_RootElement.Q<VisualElement>(RightBayQ);

            
            
            
        }
        public void DisplaySections() {
            DisplayTerminal();
            DisplayViewport();
            DisplayRightBay();
        }
        protected void DisplayTerminal() {
            
        }
        protected void DisplayViewport() {
            
        }
        protected void DisplayRightBay() {
            
        }

        // Unregister events from an external objects
        public virtual void Disable()
        {
            m_EventRegistry.Dispose();
        }

        // Event-handling method

        // If the m_ParentElement is fading off, hide it once the USS transition is complete
        private void ParentElement_TransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target == m_RootElement && m_RootElement.ClassListContains(k_HiddenClass))
            {
                HideImmediately();
            }
        }

        // Show and use the transition, if enabled
        public virtual void Show()
        {
            // Use helper class to run coroutines
            Coroutines.StopCoroutine(ref m_DisplayRoutine);
            m_DisplayRoutine = Coroutines.StartCoroutine(ShowWithDelay(m_TransitionDelay));
        }

        // Show with a variable second delay
        private IEnumerator ShowWithDelay(float delayInSecs)
        {
            yield return new WaitForSeconds(delayInSecs);

            m_RootElement.style.display = DisplayStyle.Flex;

            if (m_UseTransition)
            {
                m_RootElement.AddToClassList(k_VisibleClass); // Add visible class
                m_RootElement.BringToFront();
                m_RootElement.RemoveFromClassList(k_HiddenClass); // Remove hidden class
            }
        }

        // Hide and use transition, if enabled
        public virtual void Hide(float delay = 0f)
        {
            // Use helper class to run coroutines
            Coroutines.StopCoroutine(ref m_DisplayRoutine);

            m_DisplayRoutine = Coroutines.StartCoroutine(HideWithDelay(delay));
        }

        // Hide with a variable second delay 
        private IEnumerator HideWithDelay(float delayInSecs)
        {
            yield return new WaitForSeconds(delayInSecs);

            if (m_UseTransition)
            {
                m_RootElement.AddToClassList(k_HiddenClass); // Add hidden class
                m_RootElement.RemoveFromClassList(k_VisibleClass); // Remove visible class
            }
            else
            {
                HideImmediately();
            }
        }

        // Hide without a transition
        public void HideImmediately()
        {
            m_RootElement.style.display = DisplayStyle.None;
        }
        #endregion
    
    }

}