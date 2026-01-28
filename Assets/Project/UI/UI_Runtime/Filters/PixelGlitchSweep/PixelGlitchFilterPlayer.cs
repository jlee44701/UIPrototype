using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class PixelGlitchFilterPlayer
{
    public sealed class Handle
    {
        internal IVisualElementScheduledItem TickItem;
        internal bool CleanupQueued;

        internal readonly List<FilterFunction> Filters = new(1);
        internal FilterFunction Func;
    }

    public static Handle PlayOnce(
        VisualElement ve,
        FilterFunctionDefinition def,
        float durationSeconds,
        float pixelSizePx,
        float amplitudePx,
        float directionDeg,
        Func<float, float> easing = null)
    {
        easing ??= SmoothStep01;

        var h = new Handle();

        // Follow Unity’s “apply custom filters in C#” model: create FilterFunction and apply a List<FilterFunction>. :contentReference[oaicite:2]{index=2}
        h.Func = new FilterFunction(def);
        h.Func.AddParameter(new FilterParameter(0f));           // Amount
        h.Func.AddParameter(new FilterParameter(pixelSizePx));  // PixelSize
        h.Func.AddParameter(new FilterParameter(amplitudePx));  // Amplitude
        h.Func.AddParameter(new FilterParameter(directionDeg)); // Direction

        h.Filters.Add(h.Func);
        ve.style.filter = h.Filters;

        var start = Time.realtimeSinceStartup;
        var invDur = durationSeconds > 1e-5f ? 1f / durationSeconds : 0f;

        h.TickItem = ve.schedule.Execute(() =>
        {
            var t = (Time.realtimeSinceStartup - start) * invDur;

            if (t >= 1f)
            {
                // Final write, keep filter for this frame.
                SetAmount(ve, h, 1f);

                // Stop ticking immediately, but clear next frame.
                if (!h.CleanupQueued)
                {
                    h.CleanupQueued = true;
                    h.TickItem?.Pause();

                    ve.schedule.Execute(() =>
                    {
                        // Remove the inline style value (don’t set “none”).
                        ve.style.filter = StyleKeyword.Null; // documented inline-style removal :contentReference[oaicite:3]{index=3}
                    }); // Execute runs next frame :contentReference[oaicite:4]{index=4}
                }

                return;
            }

            SetAmount(ve, h, easing(Mathf.Clamp01(t)));
        }).Every(0);

        return h;
    }

    static void SetAmount(VisualElement ve, Handle h, float amount01)
    {
        h.Func.SetParameter(0, new FilterParameter(Mathf.Clamp01(amount01)));
        h.Filters[0] = h.Func;       // FilterFunction is a struct; reassign into list
        ve.style.filter = h.Filters; // re-apply list
    }

    static float SmoothStep01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
