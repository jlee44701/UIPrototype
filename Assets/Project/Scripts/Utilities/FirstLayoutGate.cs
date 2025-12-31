using UnityEngine.UIElements;

namespace RuntimeUI
{
    [UxmlElement]
    public partial class FirstLayoutGate : VisualElement
    {
        public FirstLayoutGate()
        {
            style.visibility = Visibility.Hidden;

            RegisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);
        }

        void OnFirstGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            UnregisterCallback<GeometryChangedEvent>(OnFirstGeometryChanged);

            schedule.Execute(() =>
            {
                style.visibility = Visibility.Visible;
            });
        }
    }
}
