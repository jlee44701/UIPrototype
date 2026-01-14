using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Library
{
    [UxmlElement]
    public partial class BarProgressElement : Game.UI.BoundProgressElementBase
    {
        public static readonly string ussClassName = "bar-progress";
        public static readonly string containerUssClassName = ussClassName + "__container";
        public static readonly string backgroundUssClassName = ussClassName + "__background";
        public static readonly string fillUssClassName = ussClassName + "__fill";

        VisualElement m_ContainerElement;
        VisualElement _backgroundElement;
        VisualElement _fillElement;

       

        public BarProgressElement()
        {
            AddToClassList(ussClassName);

            m_ContainerElement = new VisualElement { name = "container" };
            m_ContainerElement.AddToClassList(containerUssClassName);

            // Container must have a size even if label is hidden, because absolute children do not size parents.
            m_ContainerElement.style.position = Position.Relative;
            
            hierarchy.Add(m_ContainerElement);

            if (m_LabelElement != null)
            {
                m_LabelElement.RemoveFromHierarchy();
                m_ContainerElement.Add(m_LabelElement);

                // If we want the label overlayed centered, we style it here or in USS.
                // Leaving layout decisions to USS is usually cleaner.
                m_LabelElement.BringToFront();
            }

            _backgroundElement = new VisualElement { name = "background" };
            _backgroundElement.AddToClassList(backgroundUssClassName);
            _backgroundElement.style.position = Position.Absolute;
            _backgroundElement.style.left = 0;
            _backgroundElement.style.right = 0;
            _backgroundElement.style.top = 0;
            _backgroundElement.style.bottom = 0;
            _backgroundElement.pickingMode = PickingMode.Ignore;

            _fillElement = new VisualElement { name = "fill" };
            _fillElement.AddToClassList(fillUssClassName);

            _fillElement.style.left = 0;
            _fillElement.style.top = 0;
            _fillElement.style.bottom = 0;
            _fillElement.style.width = new Length(0f, LengthUnit.Percent);
            _fillElement.pickingMode = PickingMode.Ignore;

            m_ContainerElement.hierarchy.Add(_backgroundElement);
            m_ContainerElement.hierarchy.Add(_fillElement);

            m_LabelElement?.BringToFront();
            OnProgressChanged(GetProgressPercent());
        }

        protected override void OnProgressChanged(float progressPercent)
        {
            if (_fillElement == null)
                return;

            _fillElement.style.width = new Length(progressPercent, LengthUnit.Percent);
        }

        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => _fillElement != null ? _fillElement.style.backgroundColor.value : default;
            set
            {
                if (_fillElement == null)
                    return;

                _fillElement.style.backgroundColor = value;
            }
        }

        [UxmlAttribute, CreateProperty]
        public Color backgroundColor
        {
            get => _backgroundElement != null ? _backgroundElement.style.backgroundColor.value : default;
            set
            {
                if (_backgroundElement == null)
                    return;

                _backgroundElement.style.backgroundColor = value;
            }
        }
    }
}
