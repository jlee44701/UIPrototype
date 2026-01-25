using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Filters
{
    public class FilterEffectRunner
    {
        sealed class Handle
        {
            public IVisualElementScheduledItem Tick;
            public IVisualElementScheduledItem Clear;
            public EventCallback<DetachFromPanelEvent> DetachCb;

            public readonly List<FilterFunction> Filters = new(1);
            public FilterFunction Filter;
        }

        readonly Dictionary<VisualElement, Handle> _active = new();

        public void Apply(VisualElement ve, FilterFunction filter)
        {
            if (ve == null)
                return;

            Stop(ve, clearInlineFilter: false);

            var cloned = CloneFilter(filter);
            ve.style.filter = new List<FilterFunction> { cloned };
        }

        public void PlayOnce(VisualElement ve, float durationSeconds, FilterFunction filter)
        {
            if (ve == null) return;

            Stop(ve, clearInlineFilter: true);

            var h = new Handle();
            h.Filter = CloneFilter(filter);
            h.Filters.Add(h.Filter);
            ve.style.filter = h.Filters;

            h.DetachCb = _ => Stop(ve, clearInlineFilter: true);
            ve.RegisterCallback(h.DetachCb);

            _active[ve] = h;

            var delayMs = Mathf.Max(0, Mathf.RoundToInt(durationSeconds * 1000f));
            h.Clear = ve.schedule.Execute(() =>
            {
                if (!_active.TryGetValue(ve, out var live) || !ReferenceEquals(live, h))
                    return;

                Stop(ve, clearInlineFilter: true);
            }).StartingIn(delayMs);
        }

        public void PlayOnceFloatParam(
            VisualElement ve,
            float durationSeconds,
            FilterFunction filter,
            int paramIndex,
            float from,
            float to)
        {
            if (ve == null)
                return;

            Stop(ve, clearInlineFilter: true);

            var h = new Handle();
            h.Filter = CloneFilter(filter);

            if (paramIndex < 0 || paramIndex >= h.Filter.parameterCount)
            {
                PlayOnce(ve, durationSeconds, h.Filter);
                return;
            }

            h.Filter.SetParameter(paramIndex, new FilterParameter(from));
            h.Filters.Add(h.Filter);
            ve.style.filter = h.Filters;

            h.DetachCb = _ => Stop(ve, clearInlineFilter: true);
            ve.RegisterCallback(h.DetachCb);

            _active[ve] = h;

            var start = Time.realtimeSinceStartup;
            var invDur = durationSeconds > 1e-5f ? 1f / durationSeconds : 0f;

            h.Tick = ve.schedule.Execute(() =>
            {
                if (!_active.TryGetValue(ve, out var live) || !ReferenceEquals(live, h))
                    return;

                var rawT = invDur > 0f ? (Time.realtimeSinceStartup - start) * invDur : 1f;
                var t01 = SmoothStep01(Mathf.Clamp01(rawT));

                var value = Mathf.LerpUnclamped(from, to, t01);
                h.Filter.SetParameter(paramIndex, new FilterParameter(value));

                h.Filters[0] = h.Filter;
                ve.style.filter = h.Filters;

                if (rawT >= 1f)
                    Stop(ve, clearInlineFilter: true);
            }).Every(0);
        }

        public void ClearInlineFilter(VisualElement ve)
        {
            if (ve == null)
                return;

            Stop(ve, clearInlineFilter: true);
        }

        public void Stop(VisualElement ve, bool clearInlineFilter)
        {
            if (ve == null)
                return;

            if (_active.TryGetValue(ve, out var h))
            {
                h.Tick?.Pause();
                h.Clear?.Pause();

                if (h.DetachCb != null)
                    ve.UnregisterCallback(h.DetachCb);

                _active.Remove(ve);
            }

            if (clearInlineFilter)
            {
                ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
            }
        }

        static FilterFunction CloneFilter(FilterFunction src)
        {
            var dst = src.type == FilterFunctionType.Custom && src.customDefinition != null
                ? new FilterFunction(src.customDefinition)
                : new FilterFunction(src.type);

            for (var i = 0; i < src.parameterCount; i++)
                dst.AddParameter(src.GetParameter(i));

            return dst;
        }

        static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
