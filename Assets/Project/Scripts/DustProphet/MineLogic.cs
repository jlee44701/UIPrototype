using UnityEngine;

namespace Game.Mine {
    public class MineLogic {
        DustProphetSO d;
        GameSettings s;
        public MineLogic(DustProphetSO d, GameSettings s) {
            this.d = d;
            this.s = s;
        }
        public void Tick() {
            var tickRate = s.tickRatePerSecond;
            var depth = d.currentDepthThisRun.Value;
            var heat = d.currentHeat01.Value;
            var layerHardness = d.currentLayerHardness01.Value;
            var pressure = d.currentPressure01.Value;
            var vibration = d.currentVibration01.Value;
            var yieldBuffer = d.currentYieldBuffer.Value;
            var pressureCenter = d.targetPressureCenter01.Value;
            var pressureHalfWidth = d.targetPressureHalfWidth.Value;
            var timeInOptimalBand = d.timeInOptimalBand.Value;
        }
    }

}