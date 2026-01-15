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

        protected override string controlUssClassName => ussClassName;

        VisualElement m_ContainerElement;
        VisualElement m_BackgroundElement;
        VisualElement m_FillElement;

        public BarProgressElement()
        {
            m_ContainerElement = new VisualElement { name = containerUssClassName };
            m_ContainerElement.AddToClassList(containerUssClassName);
            m_ContainerElement.style.position = Position.Relative;

            contentContainer.Add(m_ContainerElement);

            m_BackgroundElement = new VisualElement { name = backgroundUssClassName };
            m_BackgroundElement.AddToClassList(backgroundUssClassName);
            m_BackgroundElement.style.position = Position.Absolute;
            m_BackgroundElement.style.left = 0;
            m_BackgroundElement.style.right = 0;
            m_BackgroundElement.style.top = 0;
            m_BackgroundElement.style.bottom = 0;
            m_BackgroundElement.pickingMode = PickingMode.Ignore;

            m_FillElement = new VisualElement { name = fillUssClassName };
            m_FillElement.AddToClassList(fillUssClassName);
            m_FillElement.style.left = 0;
            m_FillElement.style.top = 0;
            m_FillElement.style.bottom = 0;
            m_FillElement.style.width = new Length(0f, LengthUnit.Percent);
            m_FillElement.pickingMode = PickingMode.Ignore;

            m_ContainerElement.hierarchy.Add(m_BackgroundElement);
            m_ContainerElement.hierarchy.Add(m_FillElement);

            OnProgressChanged(GetProgressPercent());
        }

        protected override void OnProgressChanged(float progressPercent)
        {
            if (m_FillElement == null)
                return;

            m_FillElement.style.width = new Length(progressPercent, LengthUnit.Percent);
        }

        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => m_FillElement != null ? m_FillElement.style.backgroundColor.value : default;
            set
            {
                if (m_FillElement == null)
                    return;

                m_FillElement.style.backgroundColor = value;
            }
        }

        [UxmlAttribute, CreateProperty]
        public Color backgroundColor
        {
            get => m_BackgroundElement != null ? m_BackgroundElement.style.backgroundColor.value : default;
            set
            {
                if (m_BackgroundElement == null)
                    return;

                m_BackgroundElement.style.backgroundColor = value;
            }
        }
    }
}
