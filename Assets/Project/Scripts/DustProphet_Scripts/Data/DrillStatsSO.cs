using UnityEngine;

namespace Game.Mine {
    [
        CreateAssetMenu(fileName = "DrillSO",
            menuName = "Game/DrillSO")]
    public class DrillStatsSO : ScriptableObject {
        public float currentHealth;
        public float maximumHealth;
    }
}
