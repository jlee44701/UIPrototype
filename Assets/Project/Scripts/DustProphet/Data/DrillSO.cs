using UnityEngine;

namespace Game.Mine {
    [CreateAssetMenu(fileName = "DrillSO",
            menuName = "Game/DrillSO")]
    public class DrillSO : ScriptableObject {
        public string drillName;
        public float hardness01;
        public float durability01; 
    }

}