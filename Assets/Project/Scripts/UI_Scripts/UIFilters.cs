
using System;
using Events.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VInspector;

namespace UI.Filters {
    public class UIFilters : MonoBehaviour {
        [Foldout("Filter Definitions")]
        [SerializeField] FilterFunctionDefinition _crtFilter;
        [SerializeField] FilterFunctionDefinition  _pixelGlitchSweepFilter;
        [EndFoldout]
        
        
        FilterEffectRunner _runner = new FilterEffectRunner();  
        void OnEnable() {
            Unsubscribe();
            FilterEvents.ApplyPixelGlitchSweep += OnApplyPixelGlitchSweep;
            FilterEvents.ApplyCrtFilter += OnApplyCrtFilter;
        }

        void OnDisable() {
            Unsubscribe();
        }
        void Unsubscribe() {
            FilterEvents.ApplyPixelGlitchSweep -= OnApplyPixelGlitchSweep;
            FilterEvents.ApplyCrtFilter -= OnApplyCrtFilter;
        }

        void  OnApplyPixelGlitchSweep(VisualElement target, PixelGlitchSweepParams settings) {

            var filterFunction = new FilterFunction(_pixelGlitchSweepFilter);
            filterFunction.AddParameter(new FilterParameter(1)); 
            filterFunction.AddParameter(new FilterParameter(settings._pixelSizePx));
            filterFunction.AddParameter(new FilterParameter(settings._amplitudePx));
            filterFunction.AddParameter(new FilterParameter(settings._directionDeg));
            
            _runner.PlayOnceFloatParam(
                target,
                settings._duration,
                filterFunction,
                paramIndex: 0,
                from: 0f,
                to: 1f);
        }

        void OnApplyCrtFilter(VisualElement target) {
            
        }
    }
    [Serializable]
    public struct PixelGlitchSweepParams {
        [Range(0, 10)] public float _duration;
        [Range(2,50)] public float _pixelSizePx;
        [Range(0.1f,30)] public  float _amplitudePx;
        [Range(0,180)] public float _directionDeg;
    }

}
