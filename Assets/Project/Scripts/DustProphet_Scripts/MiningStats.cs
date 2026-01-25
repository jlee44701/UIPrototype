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
        DustProphetSO _data;
        public MiningStats(VisualElement rightBayContainer, VisualTreeAsset mineStatsUxmlAsset, DustProphetSO data) {
            _rightBayContainer = rightBayContainer;
            _data = data;
            _mineStatsUxmlAsset = mineStatsUxmlAsset;
            
            SetVisualElements();
            UpdateDataSource();
            UpdatePaths();
        }

        void SetVisualElements() {
            var miningStatsInstance = _mineStatsUxmlAsset.Instantiate();
             _rightBayContainer.Add(miningStatsInstance);
            // _depthElement = _root.Q<VisualElement>("depth") ??  throw new NullReferenceException(nameof(_depthElement));
            // _depthLabel = _root.Q<Label>("depth-label") ??  throw new NullReferenceException(nameof(_depthLabel));
            // _pressureElement = _root.Q<VisualElement>("pressure") ??  throw new NullReferenceException(nameof(_pressureElement));
            // _pressureLabel =  _root.Q<Label>("pressure-bar-label") ??  throw new NullReferenceException(nameof(_pressureLabel));
            // _heatElement = _root.Q<VisualElement>("heat") ??  throw new NullReferenceException(nameof(_heatElement));
            // _vibrationElement = _root.Q<VisualElement>("vibration") ??  throw new NullReferenceException(nameof(_vibrationElement));
            // _yieldBufferElement = _root.Q<VisualElement>("yield-buffer") ??  throw new NullReferenceException(nameof(_yieldBufferElement));
            // _layerHardnessElement = _root.Q<VisualElement>("layer-hardness") ??  throw new NullReferenceException(nameof(_layerHardnessElement));
            // _pressureCenterElement = _root.Q<VisualElement>("pressure-center") ??  throw new NullReferenceException(nameof(_pressureCenterElement));
            // _pressureHalfWidthElement = _root.Q<VisualElement>("pressure-half-width") ??  throw new NullReferenceException(nameof(_pressureHalfWidthElement));
            // _pressureBarProgressElement = _root.Q<VisualElement>("pressure-bar-progress") ??  throw new NullReferenceException(nameof(_pressureBarProgressElement));
            //
            //-----------------------
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
            
           // _currentVibrationBarElement.name = "current-vibration-bar";
            //_currentVibrationBarElement.SetBlockNameOverride("vibration");
            //_parentContainer.Add(_currentVibrationBarElement);
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
        void UpdateDataSource() {
            if (_data) {
                // _pressureElement.dataSource = _data.currentPressure01;
                // _pressureBarProgressElement.dataSource = _data.currentPressure01;
                // _pressureLabel.dataSource = _data.currentPressure01;
                //
                // _depthElement.dataSource = _data.currentDepthThisRun;
                // _heatElement.dataSource = _data.currentHeat01;
                // _vibrationElement.dataSource = _data.currentVibration01;
                // _yieldBufferElement.dataSource = _data.currentYieldBuffer;
                // _layerHardnessElement.dataSource = _data.currentLayerHardness01;
                // _pressureCenterElement.dataSource = _data.targetPressureCenter01;
                // _pressureHalfWidthElement.dataSource = _data.targetPressureHalfWidth;
                

                
                _radialProgressElement.maxValue = 1;
                _currentVibrationBarElement.maxValue = 1;
                _heatElement.maxValue = 1;
                _yieldBufferElement.maxValue = 1;
                _layerHardnessElement.maxValue = 1;
                _pressureCenterElement.maxValue = 1;
                
            }
        }
        void UpdatePaths() {
            
            var destinationString = nameof(BoundProgressElementBase.value);
            var inverseConverterGroup = "progressColorInverse";

            SetLabels();
            
            Bindables.SetBindingWithConverter(
                _radialProgressElement,
                "currentPressure01.Value",
                destinationString, 
                BindingMode.ToTarget, 
                inverseConverterGroup,
                out var binding
                );
            _radialProgressElement.SetBinding("progressColor", binding);
            //_radialProgressElement.SetBinding();
            
            //
            Bindables.SetBindingWithConverter(
                _currentVibrationBarElement,
                "currentVibration01.Value",
                destinationString, 
                BindingMode.ToTarget, 
                inverseConverterGroup,
                out var vibrationBinding
                );

            Bindables.SetBindingWithConverter(
                _heatElement,
                "currentHeat01.Value",
                destinationString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var heatBinding
                );
            
            Bindables.SetBindingWithConverter(
                _yieldBufferElement,
                "currentYieldBuffer.Value",
                destinationString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var yieldBufferBinding
                );
            
            Bindables.SetBindingWithConverter(_layerHardnessElement,
                "currentLayerHardness01.Value",
                destinationString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var layerHardnessBinding
                );
            Bindables.SetBindingWithConverter(
                _pressureCenterElement,
                "targetPressureCenter01.Value",
                destinationString,
                BindingMode.ToTarget,
                inverseConverterGroup,
                out var targetPressureHalfWidthBinding
                );
            
            SetColors();
            
            // Bindables.SetBinding(_currentVibrationBarElement,
            //     "");

            void SetLabels() {
                _currentVibrationBarElement.label = "Vibration";
                _heatElement.label = "Heat";
                _yieldBufferElement.label = "Yield Buffer";
                _layerHardnessElement.label = "Layer Hardness";
                _pressureCenterElement.label = "Pressure Center";
                
            }

            void SetColors() {
                var colorProperty = nameof(BarProgressElement.progressColor);
                _currentVibrationBarElement.SetBinding(colorProperty, vibrationBinding);
                _heatElement.SetBinding(colorProperty, heatBinding);
                _yieldBufferElement.SetBinding(colorProperty, yieldBufferBinding);
                _layerHardnessElement.SetBinding(colorProperty, layerHardnessBinding);
                _pressureCenterElement.SetBinding(colorProperty, targetPressureHalfWidthBinding);
            }
        }

        public void Cleanup() {
            // Clean up or unbind any resources if necessary
        }
    }
    

}
