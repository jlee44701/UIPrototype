using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Filters
{
    public sealed class FilterEffectRunner
    {
        readonly Dictionary<VisualElement, Handle> _active = new();

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

            if (!_active.TryGetValue(ve, out var h))
            {
                h = new Handle(ve, this);
                _active.Add(ve, h);
            }
            else
            {
                h.StopScheduled();
            }

            h.Filter = CloneFilter(filter);

            if (paramIndex < 0 || paramIndex >= h.Filter.parameterCount)
                return;

            h.ParamIndex = paramIndex;
            h.From = from;
            h.To = to;
            h.Start = Time.realtimeSinceStartup;
            h.InvDur = durationSeconds > 1e-5f ? 1f / durationSeconds : 0f;

            h.Filter.SetParameter(h.ParamIndex, new FilterParameter(h.From));

            // UI Toolkit can clear the assigned list when we later set StyleKeyword.Null.
            // Keep the list instance but re-ensure index 0 exists before we assign.
            EnsureSlot(h.Filters);
            h.Filters[0] = h.Filter;
            ve.style.filter = h.Filters;

            h.Tick = ve.schedule.Execute(h.OnTick).Every(0);
        }

        public void Stop(VisualElement ve, bool clearInlineFilter)
        {
            if (ve == null)
                return;

            if (!_active.TryGetValue(ve, out var h))
                return;

            h.StopScheduled();

            if (clearInlineFilter)
            {
                // Defer one tick so the style system reliably observes the change.
                h.Clear = ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
            }
        }

        internal void Release(VisualElement ve)
        {
            if (ve == null)
                return;

            if (!_active.TryGetValue(ve, out var h))
                return;

            h.StopScheduled();

            if (h.DetachCb != null)
                ve.UnregisterCallback(h.DetachCb);

            _active.Remove(ve);

            ve.style.filter = StyleKeyword.Null;
        }

        static void EnsureSlot(List<FilterFunction> filters)
        {
            if (filters.Count == 0)
                filters.Add(default);
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

        sealed class Handle
        {
            readonly FilterEffectRunner _runner;

            public readonly VisualElement Ve;

            public IVisualElementScheduledItem Tick;
            public IVisualElementScheduledItem Clear;
            public readonly EventCallback<DetachFromPanelEvent> DetachCb;

            public readonly List<FilterFunction> Filters = new(1);

            public FilterFunction Filter;
            public int ParamIndex;
            public float From;
            public float To;
            public float Start;
            public float InvDur;

            public Handle(VisualElement ve, FilterEffectRunner runner)
            {
                Ve = ve;
                _runner = runner;

                DetachCb = OnDetach;
                ve.RegisterCallback(DetachCb);
            }

            public void StopScheduled()
            {
                Tick?.Pause();
                Clear?.Pause();
                Tick = null;
                Clear = null;
            }

            void OnDetach(DetachFromPanelEvent _)
            {
                _runner.Release(Ve);
            }

            public void OnTick()
            {
                var rawT = InvDur > 0f ? (Time.realtimeSinceStartup - Start) * InvDur : 1f;

                var t01 = rawT;
                if (t01 < 0f) t01 = 0f;
                else if (t01 > 1f) t01 = 1f;

                // Smoothstep
                t01 = t01 * t01 * (3f - 2f * t01);

                var value = Mathf.LerpUnclamped(From, To, t01);
                Filter.SetParameter(ParamIndex, new FilterParameter(value));

                EnsureSlot(Filters);
                Filters[0] = Filter;
                Ve.style.filter = Filters;

                if (rawT >= 1f)
                {
                    Tick?.Pause();
                    Tick = null;

                    Clear?.Pause();
                    Clear = Ve.schedule.Execute(() => Ve.style.filter = StyleKeyword.Null);
                }
            }
        }
    }
}
