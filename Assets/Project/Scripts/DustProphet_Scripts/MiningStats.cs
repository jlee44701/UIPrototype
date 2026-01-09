using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Game.Mine {
    public class MiningStats {
        readonly VisualElement _root;
        VisualElement
            _depthElement,
            _pressureElement,
            _heatElement,
            _vibrationElement,
            _yieldBufferElement,
            _layerHardnessElement,
            _pressureCenterElement,
            _pressureHalfWidthElement;

        Label
            _depthLabel,
            _timeInOptimalLabel;

        VisualTreeAsset _miningStatsAsset;

        DustProphetSO _data;
        public MiningStats(VisualElement root, VisualTreeAsset miningStatsAsset, DustProphetSO data) {
            _root = root;
            _data = data;
            _miningStatsAsset = miningStatsAsset;
            
            SetVisualElements();
            UpdateDataSource(root);
            UpdatePaths();
        }

        void SetVisualElements() {
            var miningStatsInstance = _miningStatsAsset.Instantiate();
            _root.Add(miningStatsInstance);
            _depthElement = _root.Q<VisualElement>("depth") ??  throw new NullReferenceException(nameof(_depthElement));
            _depthLabel = _root.Q<Label>("depth-label") ??  throw new NullReferenceException(nameof(_depthLabel));
            _pressureElement = _root.Q<VisualElement>("pressure") ??  throw new NullReferenceException(nameof(_pressureElement));
            _heatElement = _root.Q<VisualElement>("heat") ??  throw new NullReferenceException(nameof(_heatElement));
            _vibrationElement = _root.Q<VisualElement>("vibration") ??  throw new NullReferenceException(nameof(_vibrationElement));
            _yieldBufferElement = _root.Q<VisualElement>("yield-buffer") ??  throw new NullReferenceException(nameof(_yieldBufferElement));
            _layerHardnessElement = _root.Q<VisualElement>("layer-hardness") ??  throw new NullReferenceException(nameof(_layerHardnessElement));
            _pressureCenterElement = _root.Q<VisualElement>("pressure-center") ??  throw new NullReferenceException(nameof(_pressureCenterElement));
            _pressureHalfWidthElement = _root.Q<VisualElement>("pressure-half-width") ??  throw new NullReferenceException(nameof(_pressureHalfWidthElement));
        }
        void UpdateDataSource(VisualElement root) {
            if (_data) {
                // _pressureElement.dataSource = _data.currentPressure01;
                _depthElement.dataSource = _data.currentDepthThisRun;
                // _heatElement.dataSource = _data.currentHeat01;
                _vibrationElement.dataSource = _data.currentVibration01;
                _yieldBufferElement.dataSource = _data.currentYieldBuffer;
                _layerHardnessElement.dataSource = _data.currentLayerHardness01;
                _pressureCenterElement.dataSource = _data.targetPressureCenter01;
                _pressureHalfWidthElement.dataSource = _data.targetPressureHalfWidth;
            }
        }
        void UpdatePaths() {
            _depthLabel.SetBinding("text", new DataBinding() {
                dataSourcePath = new PropertyPath(nameof(_data.currentDepthThisRun.Value)), bindingMode = BindingMode.ToTarget
            });
        }

        public void Cleanup() {
            // Clean up or unbind any resources if necessary
        }
        
    }
}
