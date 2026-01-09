using Obvious.Soap;
using Unity.Properties;
using UnityEngine;

namespace Game.UI {
    [CreateAssetMenu(fileName = "FloatVariableViewModel", menuName = "Scriptable Objects/Float Variable ViewModel")]
    public class FloatVariableViewModel : ScriptableObject {
        [SerializeField] protected FloatVariable _data;
        [CreateProperty]
        public float Value => _data.Value;
    }

}