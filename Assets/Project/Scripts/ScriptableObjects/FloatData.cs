using Obvious.Soap;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Data {
    /// <summary>
    /// The only difference with FloatVariable is compatability with UI Toolkit's data binding by using the CreateProperty attribute;
    /// </summary>
    
    [CreateAssetMenu(fileName = "FloatData.asset", menuName = "Game/Variables/FloatData")]
    public class FloatData : FloatVariable {
        [CreateProperty]
        public float ValueUI => Value;
        [CreateProperty]
        public float MaxUI => Max;
        [CreateProperty]
        public float MinUI => Min;
    }

}