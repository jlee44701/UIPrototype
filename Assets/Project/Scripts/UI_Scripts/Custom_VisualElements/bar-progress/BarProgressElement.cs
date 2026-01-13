using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Library
{
    [UxmlElement]
    public partial class BarProgressElement : Game.UI.BoundProgressElementBase
    {
        public static readonly string ussClassName = "bar-progress";
        public static readonly string trackUssClassName = ussClassName + "__track";
        public static readonly string fillUssClassName = ussClassName + "__fill";

        VisualElement m_TrackElement;
        VisualElement m_FillElement;

        public BarProgressElement()
        {
            AddToClassList(ussClassName);

            // Container must define a positioning context for absolute children
            style.position = Position.Relative;

            m_TrackElement = new VisualElement();
            m_TrackElement.name = "track";
            m_TrackElement.AddToClassList(trackUssClassName);
            m_TrackElement.style.position = Position.Absolute;
            m_TrackElement.style.left = 0;
            m_TrackElement.style.right = 0;
            m_TrackElement.style.top = 0;
            m_TrackElement.style.bottom = 0;
            m_TrackElement.pickingMode = PickingMode.Ignore;

            m_FillElement = new VisualElement();
            m_FillElement.name = "fill";
            m_FillElement.AddToClassList(fillUssClassName);
            m_FillElement.style.position = Position.Absolute;
            m_FillElement.style.left = 0;
            m_FillElement.style.top = 0;
            m_FillElement.style.bottom = 0;
            m_FillElement.style.width = new Length(0f, LengthUnit.Percent);
            m_FillElement.pickingMode = PickingMode.Ignore;

            // Add internal visuals as children of this element
            hierarchy.Add(m_TrackElement);
            hierarchy.Add(m_FillElement);

            // Ensure initial visuals reflect current values
            OnProgressChanged(GetProgressPercent());
        }

        protected override void OnProgressChanged(float progressPercent)
        {

            // progressPercent is 0..100 (per your base class)
            m_FillElement.style.width = new Length(progressPercent, LengthUnit.Percent);
        }

        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => m_FillElement != null ? m_FillElement.style.backgroundColor.value : default;
            set
            {

                m_FillElement.style.backgroundColor = value;
            }
        }

        [UxmlAttribute, CreateProperty]
        public Color trackColor
        {
            get => m_TrackElement != null ? m_TrackElement.style.backgroundColor.value : default;
            set
            {


                m_TrackElement.style.backgroundColor = value;
            }
        }
    }
}
