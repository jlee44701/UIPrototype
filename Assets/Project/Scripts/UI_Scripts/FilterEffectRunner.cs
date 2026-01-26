using System;
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
            float to,
            Func<float, float>? easing = null)
        {
            if (ve == null)
                return;

            easing ??= expImpulse;

            // Cancel any prior run, but do not clear inline filter here.
            // Clearing is deferred and guarded so we do not wipe the new run.
            Stop(ve, clearInlineFilter: false);

            var h = new Handle(this, ve, easing);

            h.Func = CloneFilter(filter);

            if (paramIndex < 0 || paramIndex >= h.Func.parameterCount)
                return;

            h.ParamIndex = paramIndex;
            h.From = from;
            h.To = to;
            h.Start = Time.realtimeSinceStartup;
            h.InvDur = durationSeconds > 1e-5f ? 1f / durationSeconds : 0f;

            h.Func.SetParameter(h.ParamIndex, new FilterParameter(h.From));
            h.WriteFilter();

            _active[ve] = h;

            h.Tick = ve.schedule.Execute(h.OnTick).Every(0);
        }

        public void Stop(VisualElement ve, bool clearInlineFilter = true)
        {
            if (ve == null)
                return;

            if (!_active.TryGetValue(ve, out var h))
            {
                if (clearInlineFilter)
                    ClearInlineIfNoActiveEffect(ve);

                return;
            }

            h.Tick?.Pause();
            h.Clear?.Pause();
            h.Tick = null;
            h.Clear = null;

            if (h.DetachCb != null)
                ve.UnregisterCallback(h.DetachCb);

            _active.Remove(ve);

            if (clearInlineFilter)
                ClearInlineIfNoActiveEffect(ve);
        }

        void ClearInlineIfNoActiveEffect(VisualElement ve)
        {
            // We do both: immediate clear plus a guarded deferred clear.
            // The deferred clear is the one that avoids the UI Toolkit timing pitfall,
            // while the guard prevents wiping a newly started effect.
            ve.style.filter = StyleKeyword.Null;

            ve.schedule.Execute(() =>
            {
                if (!_active.ContainsKey(ve))
                    ve.style.filter = StyleKeyword.Null;
            });
        }

        internal bool IsLive(VisualElement ve, Handle h)
        {
            return _active.TryGetValue(ve, out var live) && ReferenceEquals(live, h);
        }

        internal void CleanupIfLive(Handle h)
        {
            if (!IsLive(h.Ve, h))
                return;

            // Remove first so any other scheduled work sees the run as ended.
            _active.Remove(h.Ve);

            if (h.DetachCb != null)
                h.Ve.UnregisterCallback(h.DetachCb);

            h.Ve.style.filter = StyleKeyword.Null;

            // Also guard a deferred clear, matching the known good workaround.
            h.Ve.schedule.Execute(() =>
            {
                if (!_active.ContainsKey(h.Ve))
                    h.Ve.style.filter = StyleKeyword.Null;
            });
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
        static float expImpulse( float x) {
            const float k = 1f;
            float h = k*x;
            return h*Mathf.Exp(1.0f-h);
        }
        public sealed class Handle
        {
            readonly FilterEffectRunner _runner;

            public readonly VisualElement Ve;
            readonly Func<float, float> _easing;

            public IVisualElementScheduledItem Tick;
            public IVisualElementScheduledItem Clear;
            public readonly EventCallback<DetachFromPanelEvent> DetachCb;

            public readonly List<FilterFunction> Filters = new(1);

            public FilterFunction Func;
            public int ParamIndex;
            public float From;
            public float To;
            public float Start;
            public float InvDur;

            public bool CleanupQueued;

            public Handle(FilterEffectRunner runner, VisualElement ve, Func<float, float> easing)
            {
                _runner = runner;
                Ve = ve;
                _easing = easing;

                DetachCb = OnDetach;
                ve.RegisterCallback(DetachCb);
            }

            void OnDetach(DetachFromPanelEvent _)
            {
                _runner.Stop(Ve, clearInlineFilter: true);
            }

            public void WriteFilter()
            {
                if (Filters.Count == 0)
                    Filters.Add(Func);
                else
                    Filters[0] = Func;

                Ve.style.filter = Filters;
            }

            public void OnTick()
            {
                if (!_runner.IsLive(Ve, this))
                {
                    Tick?.Pause();
                    Tick = null;
                    return;
                }

                var rawT = InvDur > 0f ? (Time.realtimeSinceStartup - Start) * InvDur : 1f;

                if (rawT >= 1f)
                {
                    // Apply final value before cleanup.
                    Func.SetParameter(ParamIndex, new FilterParameter(To));
                    WriteFilter();

                    if (!CleanupQueued)
                    {
                        CleanupQueued = true;

                        Tick?.Pause();
                        Tick = null;

                        // Guarded deferred cleanup so we do not wipe a new run.
                        Clear?.Pause();
                        Clear = Ve.schedule.Execute(() => _runner.CleanupIfLive(this));
                    }

                    return;
                }

                var t01 = _easing(Mathf.Clamp01(rawT));
                var value = Mathf.LerpUnclamped(From, To, t01);

                Func.SetParameter(ParamIndex, new FilterParameter(value));
                WriteFilter();
            }
        }
    }
}
