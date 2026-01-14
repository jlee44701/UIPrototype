using System;
using Game.UI;
using Game.UI.Library;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Mine {
    public class MiningStats {
        readonly VisualElement _parentContainer;
        VisualElement _mineContainer;
        VisualElement
            _depthElement,
            _pressureElement,
            _heatElement,
            _vibrationElement,
            _yieldBufferElement,
            _layerHardnessElement,
            _pressureCenterElement,
            _pressureHalfWidthElement;

        VisualElement
            _pressureBarProgressElement;

        Label
            _depthLabel,
            _pressureLabel,
            _timeInOptimalLabel;
        
        RadialProgress _radialProgressElement;
        BarProgressElement _currentVibrationBarElement;

        VisualTreeAsset _miningStatsAsset;

        DustProphetSO _data;
        public MiningStats(VisualElement parentContainer, VisualTreeAsset miningStatsAsset, DustProphetSO data) {
            _parentContainer = parentContainer;
            _data = data;
            _miningStatsAsset = miningStatsAsset;
            
            SetVisualElements();
            UpdateDataSource();
            UpdatePaths();
        }

        void SetVisualElements() {
            var miningStatsInstance = _miningStatsAsset.Instantiate();
             _parentContainer.Add(miningStatsInstance);
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
            _mineContainer = _parentContainer.Q<VisualElement>("mine-container");
            _radialProgressElement = _parentContainer.Q<Game.UI.Library.RadialProgress>("radial-progress") ??  throw new NullReferenceException(nameof(_radialProgressElement));

            
            // let's try adding a barelement programatically to  contrast with radialprogressElement
            _currentVibrationBarElement = new BarProgressElement();
            _parentContainer.Add(_currentVibrationBarElement);
            _mineContainer.Add(_currentVibrationBarElement);
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
                

                _radialProgressElement.SetMaxValue(1);
                _currentVibrationBarElement.SetMaxValue(1);
            }
        }
        void UpdatePaths() {
            // _depthLabel.SetBinding("text", new DataBinding() {
            //     dataSourcePath = new PropertyPath(nameof(_data.currentDepthThisRun.Value)), bindingMode = BindingMode.ToTarget
            // });
            // _pressureBarProgressElement.SetBinding("style.width", new DataBinding {
            //     dataSourcePath = new PropertyPath(nameof(_data.currentPressure01.Value)), bindingMode = BindingMode.ToTarget
            // });
            // _pressureLabel.SetBinding("text", new  DataBinding() {
            //     dataSourcePath =  new PropertyPath(nameof(_data.currentPressure01.Value)), bindingMode = BindingMode.ToTarget
            // });

            var pressureBinding = new DataBinding() {
                dataSourcePath = new PropertyPath("currentPressure01.Value"), bindingMode = BindingMode.ToTarget
            };
            if (ConverterGroups.TryGetConverterGroup("progressColorInverse", out var inverseColorGroup))
                pressureBinding.ApplyConverterGroupToUI(inverseColorGroup);
            _radialProgressElement.trackColor = Color.black;
            var valueString = nameof(BoundProgressElementBase.value);
              
            _radialProgressElement.SetBinding(valueString, pressureBinding);
            _radialProgressElement.SetBinding("progressColor", pressureBinding);

            //
            var vibrationBinding = new DataBinding() {
                dataSourcePath = new PropertyPath("currentVibration01.Value"), bindingMode = BindingMode.ToTarget
            }; 
            vibrationBinding.ApplyConverterGroupToUI(inverseColorGroup);
            _currentVibrationBarElement.trackColor = Color.black;
            _currentVibrationBarElement.SetBinding(valueString, vibrationBinding);
            _currentVibrationBarElement.SetBinding(nameof(BarProgressElement.progressColor), vibrationBinding);

        }

        public void Cleanup() {
            // Clean up or unbind any resources if necessary
        }
    }
    

}
