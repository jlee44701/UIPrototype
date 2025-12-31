using System;
using UnityEngine.UIElements;

namespace PixelEngine
{
    public static class UIHelpers
    {
        public static IVisualElementScheduledItem ExecuteNextFrame(this VisualElement rootElement, Action action)
        {
            if (rootElement == null)
                throw new ArgumentNullException(nameof(rootElement));

            return rootElement.schedule.Execute(() => action?.Invoke());
        }
    }
}
