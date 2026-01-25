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
            var depth = d.currentDepth.Value;
            var heat = d.heat01.Value;
            var layerHardness = d.layerHardness01.Value;
            var pressure = d.pressure01.Value;
            var vibration = d.vibration01.Value;
            var yieldBuffer = d.yieldBuffer.Value;
            var pressureCenter = d.targetPressureCenter01.Value;
            var pressureHalfWidth = d.targetPressureHalfWidth01.Value;
            var timeInOptimalBand = d.timeInOptimalBand.Value;
        }
    }

}