using PixelEngine;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI {
    [RequireComponent(typeof(UIDocument))]
    public class StartMenu : MonoBehaviour {
        protected UIDocument Doc;
        // these were in the base demobase class
        Slider m_Slider;
        TextField m_TextField;
        Button m_Button;

        Label m_SliderLabel;
        Label m_TextFieldLabel;
        Label m_ButtonLabel;
        
        protected VisualElement m_Root;

        // this was in an child object
        EventRegistry _eventRegistry;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable() {
            _eventRegistry = new EventRegistry();
            
            NullRefChecker.Validate(this);
        }

        protected virtual void SetVisualElements() {
            if (!Doc)
                Doc = GetComponent<UIDocument>();

            m_Root = Doc.rootVisualElement;
        }
        // Update is called once per frame
        void Update() {

        }
    }

}