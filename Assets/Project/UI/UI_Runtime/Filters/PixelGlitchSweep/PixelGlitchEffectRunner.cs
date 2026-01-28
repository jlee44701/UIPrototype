using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PixelGlitchEffectRunner
{
    sealed class Handle
    {
        public IVisualElementScheduledItem Tick;
        public bool CleanupQueued;

        public List<FilterFunction> Filters = new(1);
        public FilterFunction Func;

        public EventCallback<DetachFromPanelEvent> DetachCb;
    }

    readonly Dictionary<VisualElement, Handle> _active = new();

     // Applies exactly one filter as an inline override (replaces any existing inline filters).
    public void ApplyFilter(VisualElement ve, FilterFunction filter)
    {
        if (ve == null)
            return;

        Stop(ve);

        ve.style.filter = new List<FilterFunction> { filter };
    }

    // Applies multiple filters as an inline override (replaces any existing inline filters).
    public void ApplyFilters(VisualElement ve, IReadOnlyList<FilterFunction> filters)
    {
        if (ve == null)
            return;

        Stop(ve);

        if (filters == null || filters.Count == 0)
        {
            ClearInlineFilter(ve);
            return;
        }

        var list = new List<FilterFunction>(filters.Count);
        for (var i = 0; i < filters.Count; i++)
            list.Add(filters[i]);

        ve.style.filter = list;
    }

    // Appends one filter to the current inline list (creates a list if none exists).
    public void AppendFilter(VisualElement ve, FilterFunction filter)
    {
        if (ve == null)
            return;

        Stop(ve);

        var current = ve.style.filter.value;
        var list = current != null ? new List<FilterFunction>(current) : new List<FilterFunction>(1);
        list.Add(filter);
        ve.style.filter = list;
    }

    // Removes the inline override entirely (so USS can apply again).
    // Deferring by one frame avoids the “texture not allocated” spam you hit.
    public void ClearInlineFilter(VisualElement ve)
    {
        if (ve == null)
            return;

        Stop(ve);
        ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
    }

    // Removes inline filters that match a predicate.
    // Returns true if anything was removed.
    public bool RemoveInlineFilters(VisualElement ve, Predicate<FilterFunction> match, bool removeAll = true)
    {
        if (ve == null || match == null)
            return false;

        Stop(ve);

        var current = ve.style.filter.value;
        if (current == null || current.Count == 0)
            return false;

        var list = new List<FilterFunction>(current);
        var removed = false;

        for (var i = list.Count - 1; i >= 0; --i)
        {
            if (!match(list[i]))
                continue;

            list.RemoveAt(i);
            removed = true;

            if (!removeAll)
                break;
        }

        if (!removed)
            return false;

        if (list.Count == 0)
            ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
        else
            ve.style.filter = list;

        return true;
    }

    // Convenience: remove custom filters by definition.
    public bool RemoveInlineCustomFilter(VisualElement ve, FilterFunctionDefinition def, bool removeAll = true)
    {
        if (ve == null || !def)
            return false;

        return RemoveInlineFilters(ve, f => f.type == FilterFunctionType.Custom && f.customDefinition == def, removeAll);
    }

    // Removes only matching custom filter(s) from the current inline filter list.
    // Returns true if we removed something.
    public bool RemoveInlineCustomFilter(VisualElement ve, FilterFunctionDefinition def)
    {
        if (ve == null || !def)
            return false;

        StopAnimationOnly(ve);

        var list = ve.style.filter.value;
        if (list == null || list.Count == 0)
            return false;

        var removed = false;

        for (var i = list.Count - 1; i >= 0; --i)
        {
            var f = list[i];
            if (f.type == FilterFunctionType.Custom && f.customDefinition == def)
            {
                list.RemoveAt(i);
                removed = true;
            }
        }

        if (!removed)
            return false;

        if (list.Count == 0)
        {
            ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
        }
        else
        {
            ve.style.filter = list;
        }

        return true;
    }

    // If you want to force "no filter" even if USS defines one, set the inline keyword to None.
    // StyleKeyword.None is the "none" keyword value. :contentReference[oaicite:2]{index=2}
    public void OverrideNoFilter(VisualElement ve)
    {
        if (ve == null)
            return;

        Stop(ve);
        ve.style.filter = StyleKeyword.None;
    }

    public void PlayOnce(
        VisualElement ve,
        FilterFunctionDefinition def,
        float durationSeconds,
        float pixelSizePx,
        float amplitudePx,
        float directionDeg,
        Func<float, float> easing = null)
    {
        if (ve == null || !def)
            return;

        easing ??= SmoothStep01;

        Stop(ve);

        var h = new Handle();

        h.Func = new FilterFunction(def);
        h.Func.AddParameter(new FilterParameter(0f));
        h.Func.AddParameter(new FilterParameter(pixelSizePx));
        h.Func.AddParameter(new FilterParameter(amplitudePx));
        h.Func.AddParameter(new FilterParameter(directionDeg));

        h.Filters.Add(h.Func);
        ve.style.filter = h.Filters;

        h.DetachCb = _ => Stop(ve);
        ve.RegisterCallback(h.DetachCb);

        _active[ve] = h;

        var start = Time.realtimeSinceStartup;
        var invDur = durationSeconds > 1e-5f ? 1f / durationSeconds : 0f;

        h.Tick = ve.schedule.Execute(() =>
        {
            if (!_active.TryGetValue(ve, out var live) || !ReferenceEquals(live, h))
                return;

            var t = (Time.realtimeSinceStartup - start) * invDur;

            if (t >= 1f)
            {
                SetAmount(ve, h, 1f);

                if (!h.CleanupQueued)
                {
                    h.CleanupQueued = true;
                    h.Tick?.Pause();
                    h.Tick = null;

                    ve.schedule.Execute(() => ve.style.filter = StyleKeyword.Null);
                    _active.Remove(ve);
                }

                return;
            }

            SetAmount(ve, h, easing(Mathf.Clamp01(t)));
        }).Every(0);
    }

    // Stops animation bookkeeping; does not touch the element's filter.
    void StopAnimationOnly(VisualElement ve)
    {
        if (!_active.TryGetValue(ve, out var h))
            return;

        h.Tick?.Pause();
        h.Tick = null;

        if (h.DetachCb != null)
        {
            ve.UnregisterCallback(h.DetachCb);
            h.DetachCb = null;
        }

        _active.Remove(ve);
    }

    public void Stop(VisualElement ve)
    {
        if (ve == null)
            return;

        if (!_active.TryGetValue(ve, out var h))
            return;

        h.Tick?.Pause();
        h.Tick = null;

        if (h.DetachCb != null)
        {
            ve.UnregisterCallback(h.DetachCb);
            h.DetachCb = null;
        }

        ve.style.filter = StyleKeyword.Null;
        _active.Remove(ve);
    }

    static void SetAmount(VisualElement ve, Handle h, float amount01)
    {
        h.Func.SetParameter(0, new FilterParameter(Mathf.Clamp01(amount01)));
        h.Filters[0] = h.Func;
        ve.style.filter = h.Filters;
    }

    static float SmoothStep01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
