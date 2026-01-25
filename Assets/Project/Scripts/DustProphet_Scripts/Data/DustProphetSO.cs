using Game.Data;
using Obvious.Soap;
using UnityEngine;

namespace Game.Mine {
    [
        CreateAssetMenu(fileName = "DustProphetSO",
            menuName = "Game/DustProphetSO")]
    public class DustProphetSO : ScriptableObject {
        public enum State {
            AutoBore,
            ManualDrive,
            Reboot
        }
        public State state;
        [Header("Manual drive op data")]


        public FloatData currentDepth;
        public FloatData yieldBuffer;
        public FloatData timeInOptimalBand;
        
        public FloatData pressure01;
        public FloatData heat01;
        public FloatData vibration01;
        public FloatData layerHardness01;
        public FloatData targetPressureCenter01;
        public FloatData targetPressureHalfWidth01;
        
        // - Target band behavior (the “fishing bar” part)
        // - targetPressureCenter drifts over time (random walk), drift speed scales with currentLayerHardness
        // - targetPressureHalfWidth starts narrow (AI broken) and widens as repairs/progression improve
        // - optionally add occasional “lurch” events (small sudden shift) to keep it lively

        //Band check
        //inBand = abs(currentPressure - targetPressureCenter) <= targetPressureHalfWidth

        //use a brownian motion function to control the targetPressureCenter 

        // pressureError01 = currentPressure01 - targetPressureCenter01
        //
        //     isInOptimalBand = abs(pressureError01) <= targetPressureHalfWidth01
        //
        // overPressure01 = max(0, abs(pressureError01) - targetPressureHalfWidth01) (this becomes your risk driver)
    }

}