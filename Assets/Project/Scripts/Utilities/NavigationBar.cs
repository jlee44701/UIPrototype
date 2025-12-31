using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI {
    [System.Serializable]
    public class NavigationBar
    {
        
       
        const string k_DefaultButtonContainerName = "button--container";
        const int k_MinButtonSize = 100;
        const int k_TextSize = 50;

        bool m_UseSquareButtons = true;
        bool m_CapitalizeButtonTitles = true;

        // UI elements
        VisualElement m_Root;  // rootVisualElement for the screen
        VisualElement m_ButtonContainer;    // Flex container for all buttons

        VisualTreeAsset m_ButtonAsset; // Template used to create buttons in the NavigationBar
        
        List<Button> m_Buttons = new List<Button>();

        public List<Button> Buttons => m_Buttons;

        // Sets up any necessary dependencies from the MessageBar
        // TO-DO: Make this the constructor
        public void Initialize(VisualElement rootElement, int numberOfButtons, VisualTreeAsset treeAsset, string buttonContainerName, bool useSquareButtons = true )
        {
            m_Root = rootElement;
            
            m_ButtonContainer = m_Root.Q<VisualElement>(buttonContainerName);

            if (m_ButtonContainer != null)
            {
                RemovePlaceHolders();
            }
            else
            {
                Debug.Log("[NavigationBar]: Missing button container");
                return;
            }

            m_UseSquareButtons = useSquareButtons;
            SetupButtons(numberOfButtons, treeAsset, null);
        }

        
        public void SetupButtons(int numberOfButtons, VisualTreeAsset asset, Sprite icon = null)
        {
            for (int i = 1; i <= numberOfButtons; i++)
            {
                InstantiateButton(asset, i.ToString(), icon);
            }
        }

        void InstantiateButton(VisualTreeAsset buttonAsset, string name, Sprite icon) {
            var buttonInstance = buttonAsset.Instantiate() ?? throw new NullReferenceException(nameof(buttonAsset.Instantiate));
            
            //Debug.Log("NavigationBar ButtonSetup: " + name);
            m_ButtonContainer.Add(buttonInstance);

            // Get the UI Button component, then set up name and callback
            Button button = buttonInstance.Q<Button>();

            button.userData = "nav-bar-button--" + name;

            // Set the initial size to be square based on the minimum dimension of the container
            if (m_UseSquareButtons)
                MakeSquare(button, m_ButtonContainer);

            // Set the button name and icon
            SetButtonLabelText(button, name);
            button.name = button.userData as string;
            SetButtonIcon(button, icon);

            // Track the UI button in a List
            m_Buttons.Add(button);
        }

        
        // Set the button name
        public void SetButtonLabelText(Button button, string labelText)
        {
            Label label = button.Q<Label>("button__label");

            if (m_CapitalizeButtonTitles)
            {
                labelText = labelText.ToUpper();
            }
 
            label.text = labelText;
            label.style.fontSize = k_TextSize;
        }

        public void SetButtonLabelTextAtIndex(int index, string name)
        {
            if (index >= m_Buttons.Count)
            {
                Debug.Log("[NavigationBar]: Button index out of range");
                return;
            }

            SetButtonLabelText(m_Buttons[index], name);
        }

        // Set the button icon
        public void SetButtonIcon(Button button, Sprite icon)
        {
            VisualElement buttonIcon = button.Q<VisualElement>("button__icon");
            if (!icon)
            {
                buttonIcon.style.display = DisplayStyle.None;
            }
            else
            {
                buttonIcon.style.backgroundImage = new StyleBackground(icon);
            }
        }

        // Set the icon based on index within the m_Buttons
        public void SetButtonIconAtIndex(int index, Sprite icon)
        {
            if (index >= m_Buttons.Count)
            {
                Debug.Log("[NavigationBar]: Button index out of range");
                return;
            }

            SetButtonIcon(m_Buttons[index], icon);
        }

        // Set the button tooltip
        public void SetButtonTooltip(Button button, string tooltip)
        {
            button.tooltip = tooltip;
        }

        // Removes any temporary buttons used for layout in UI Builder. This allows you to
        // keep placeholders in the UXML for visualization and then clear them at runtime.
        private void RemovePlaceHolders()
        {
            List<TemplateContainer> placeholders = m_ButtonContainer.Query<TemplateContainer>().ToList();

            foreach (TemplateContainer placeholder in placeholders)
            {
                m_ButtonContainer.Remove(placeholder);
            }
        }

        // Highlight the clicked button
        public void HighlightButton(int buttonIndex, string selectedButtonClassName)
        {

            // Clear all currently selected buttons
            UnhighlightButtons(selectedButtonClassName);

            if (buttonIndex >= 0 && buttonIndex < m_Buttons.Count)
            {
                // Get the clicked button using its index
                Button clickedButton = m_Buttons[buttonIndex];

                // Add the selected button class to the clicked button
                clickedButton.AddToClassList(selectedButtonClassName);
                
            }
            else
            {
                Debug.LogWarning("[NavigationBar]: Button index out of range");
            }
        }

        // Removes highlight styles from all Buttons
        private void UnhighlightButtons(string selectedButtonClassName)
        {
            List<Button> selectedButtons = m_ButtonContainer.Query<Button>()
                .Where(x => x.ClassListContains(selectedButtonClassName)).ToList();

            foreach (Button button in selectedButtons)
            {
                button.RemoveFromClassList(selectedButtonClassName);
            }
        }

        // Forces a button to be a square; uses minimum dimension from
        // another reference VisualElement (e.g., the parent container)
        private void MakeSquare(Button button, VisualElement referenceElement)
        {
            // Get a minimum size from another Visual Element
            float size = Mathf.Min(referenceElement.resolvedStyle.width, referenceElement.resolvedStyle.height);

            // Set a minimum size constraint to avoid buttons becoming too small
            size = Mathf.Max(size, k_MinButtonSize);

            // Constrain both width and height
            button.style.width = size;
            button.style.height = size;

            // Center the button within the container using FlexBox rules
            button.style.alignSelf = Align.Center;
            button.style.justifyContent = Justify.Center;
        }
    }

}