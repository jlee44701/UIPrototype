using UnityEngine;

namespace Game.Mine {
    [
        CreateAssetMenu(fileName = "DrillSO",
            menuName = "Game/DrillSO")]
    public class DrillStatsSO : ScriptableObject {
        public float health;
        public float shield;
    }
}
