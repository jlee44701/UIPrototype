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
        public static readonly string trackUssClassName = ussClassName + "__track";
        public static readonly string fillUssClassName = ussClassName + "__fill";

        VisualElement m_ContainerElement;
        VisualElement m_TrackElement;
        VisualElement m_FillElement;

        [UxmlAttribute("bar-height"), CreateProperty]
        public float barHeight { get; set; } = 16f;

        public BarProgressElement()
        {
            AddToClassList(ussClassName);

            m_ContainerElement = new VisualElement { name = "container" };
            m_ContainerElement.AddToClassList(containerUssClassName);

            // Container must have a size even if label is hidden, because absolute children do not size parents.
            m_ContainerElement.style.position = Position.Relative;
            m_ContainerElement.style.height = barHeight;

            hierarchy.Add(m_ContainerElement);

            if (m_LabelElement != null)
            {
                m_LabelElement.RemoveFromHierarchy();
                m_ContainerElement.Add(m_LabelElement);

                // If we want the label overlayed centered, we style it here or in USS.
                // Leaving layout decisions to USS is usually cleaner.
                m_LabelElement.BringToFront();
            }

            m_TrackElement = new VisualElement { name = "track" };
            m_TrackElement.AddToClassList(trackUssClassName);
            m_TrackElement.style.position = Position.Absolute;
            m_TrackElement.style.left = 0;
            m_TrackElement.style.right = 0;
            m_TrackElement.style.top = 0;
            m_TrackElement.style.bottom = 0;
            m_TrackElement.pickingMode = PickingMode.Ignore;

            m_FillElement = new VisualElement { name = "fill" };
            m_FillElement.AddToClassList(fillUssClassName);

            m_FillElement.style.left = 0;
            m_FillElement.style.top = 0;
            m_FillElement.style.bottom = 0;
            m_FillElement.style.width = new Length(0f, LengthUnit.Percent);
            m_FillElement.pickingMode = PickingMode.Ignore;

            m_ContainerElement.hierarchy.Add(m_TrackElement);
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
        public Color trackColor
        {
            get => m_TrackElement != null ? m_TrackElement.style.backgroundColor.value : default;
            set
            {
                if (m_TrackElement == null)
                    return;

                m_TrackElement.style.backgroundColor = value;
            }
        }
    }
}
