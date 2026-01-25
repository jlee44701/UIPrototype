using System;
using UI;
using UI.Filters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Events.UI {
    public static class FilterEvents {
        public static Action<VisualElement, PixelGlitchSweepParams> ApplyPixelGlitchSweep;
        public static Action<VisualElement> ApplyCrtFilter;
    }
}
