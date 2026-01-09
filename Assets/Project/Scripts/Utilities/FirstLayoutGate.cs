using UnityEngine.UIElements;

namespace Game.UI
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
