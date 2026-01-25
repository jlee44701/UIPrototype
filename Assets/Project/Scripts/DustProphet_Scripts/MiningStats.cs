using System;
using Game.UI;
using Game.UI.Library;
using Game.UI.Utilities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Mine {
    public class MiningStats {
        readonly VisualElement _rightBayContainer;


        VisualElement
            _pressureBarProgressElement;

        
        RadialProgress _radialProgressElement;
        BarProgressElement _currentVibrationBarElement;
        BarProgressElement _depthElement,
            _heatElement,
            _yieldBufferElement,
            _layerHardnessElement,
            _pressureCenterElement;

        
        VisualTreeAsset _mineStatsUxmlAsset;
        VisualElement _mineContainer;
        public MiningStats(VisualElement rightBayContainer, VisualTreeAsset mineStatsUxmlAsset) {
            _rightBayContainer = rightBayContainer;
            _mineStatsUxmlAsset = mineStatsUxmlAsset;
            
            SetVisualElements();
            UpdatePaths();
        }

        void SetVisualElements() {

            var miningStatsInstance = _mineStatsUxmlAsset.Instantiate();
            _rightBayContainer.Add(miningStatsInstance);
            _mineContainer = _rightBayContainer.Q<VisualElement>("mine-container");
            //_radialProgressElement = _parentContainer.Q<Game.UI.Library.RadialProgress>("radial-progress") ??  throw new NullReferenceException(nameof(_radialProgressElement));
            _radialProgressElement = new RadialProgress();
           
            //_radialProgressElement.SetBlockNameOverride("current-pressure");
            
            // let's try adding a barelement programatically to  contrast with radialprogressElement
            _currentVibrationBarElement = _rightBayContainer.Q<BarProgressElement>("vibration");
            _heatElement = _rightBayContainer.Q<BarProgressElement>("heat");
            _yieldBufferElement = _rightBayContainer.Q<BarProgressElement>("yield-buffer");
            _layerHardnessElement = _rightBayContainer.Q<BarProgressElement>("layer-hardness");
            _pressureCenterElement = _rightBayContainer.Q<BarProgressElement>("pressure-center");
            

            _mineContainer.Add(_currentVibrationBarElement);
            _mineContainer.Add(_radialProgressElement);
            _mineContainer.Add(_heatElement);
            _mineContainer.Add(_yieldBufferElement);
            _mineContainer.Add(_layerHardnessElement);
            _mineContainer.Add(_pressureCenterElement);
// Bind to a viewmodel that exposes:
// float currentPressure01
// string currentPressureText
            


            

        }
        void UpdatePaths() {
            
            var valueString = nameof(BoundProgressElementBase.value);
            var maxValueString = nameof(BoundProgressElementBase.maxValue);
            var inverseConverterGroup = "progressColorInverse";

            SetLabels();

            Bindables.SetBindingWithConverter(
                _radialProgressElement,
                "pressure01.Value",
                valueString, 
                BindingMode.ToTarget, 
                inverseConverterGroup,
                out var binding
                );
            
            _radialProgressElement.SetBinding("progressColor", binding);
            //_radialProgressElement.SetBinding();
            
            //
            Bindables.SetBindingWithConverter(
                _currentVibrationBarElement,
                "vibration01.ValueUI",
                valueString, 
                BindingMode.ToTarget, 
                inverseConverterGroup,
                out var vibrationBinding
                );
            Bindables.SetBinding(
                _currentVibrationBarElement,
                "vibration01.MaxUI",
                maxValueString,
                BindingMode.ToTargetOnce,
                out var maxVibration
                );

            Bindables.SetBindingWithConverter(
                _heatElement,
                "heat01.ValueUI",
                valueString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var heatBinding
                );
            Bindables.SetBinding(
                _heatElement,
                "heat01.MaxUI",
                maxValueString,
                BindingMode.ToTargetOnce,
                out var maxHeat
            );
            
            Bindables.SetBindingWithConverter(
                _yieldBufferElement,
                "yieldBuffer.ValueUI",
                valueString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var yieldBufferBinding
                );
            Bindables.SetBinding(
                _yieldBufferElement,
                "yieldBuffer.MaxUI",
                valueString,
                BindingMode.ToTargetOnce,
                out var maxYieldBinding
            );
            
            Bindables.SetBindingWithConverter(
                _layerHardnessElement,
                "layerHardness01.ValueUI",
                valueString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var layerHardnessBinding
                );
            Bindables.SetBinding(
                _layerHardnessElement,
                "layerHardness01.MaxUI",
                maxValueString,
                BindingMode.ToTargetOnce,
                out var maxLayerHardness
            );
            
            Bindables.SetBindingWithConverter(
                _pressureCenterElement,
                "pressureCenter01.ValueUI",
                valueString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var targetPressureHalfWidthBinding
                );
            Bindables.SetBinding(
                _pressureCenterElement,
                "pressureCenter01.MaxUI",
                maxValueString,
                BindingMode.ToTargetOnce,
                out var maxPressureCenter
            );
            SetColors();
            
            // Bindables.SetBinding(_currentVibrationBarElement,
            //     "");
            return;


            void SetLabels() {
                _currentVibrationBarElement.label = "Vibration";
                _heatElement.label = "Heat";
                _yieldBufferElement.label = "Yield Buffer";
                _layerHardnessElement.label = "Layer Hardness";
                _pressureCenterElement.label = "Pressure Center";
                
            }

            void SetColors() {
                var colorPropertyStringPath = nameof(BarProgressElement.progressColor);
                _currentVibrationBarElement.SetBinding(colorPropertyStringPath, vibrationBinding);
                _heatElement.SetBinding(colorPropertyStringPath, heatBinding);
                _yieldBufferElement.SetBinding(colorPropertyStringPath, yieldBufferBinding);
                _layerHardnessElement.SetBinding(colorPropertyStringPath, layerHardnessBinding);
                _pressureCenterElement.SetBinding(colorPropertyStringPath, targetPressureHalfWidthBinding);
            }
        }

        public void Cleanup() {
            // Clean up or unbind any resources if necessary
        }
    }
    

}
