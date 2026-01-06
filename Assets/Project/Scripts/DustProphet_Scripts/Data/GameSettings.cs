using UnityEngine;

namespace Game.Mine {
    [
        CreateAssetMenu(fileName = "GameSettings",
            menuName = "Scriptable Objects/GameSettings")]
    public class GameSettings : ScriptableObject {
        [Range(0f, 1f)]
        public float tickRatePerSecond = 0.02f;
        
    }

}