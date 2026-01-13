using Obvious.Soap;
using Unity.Properties;
using UnityEngine;

namespace Game.UI {

    public abstract class FloatVariableViewModel : ScriptableObject {
        [SerializeField] protected FloatVariable _data;
        [CreateProperty]
        public float Value => _data.Value;
    }

}