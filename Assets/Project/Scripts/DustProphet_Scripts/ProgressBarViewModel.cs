using Obvious.Soap;
using Unity.Properties;
using UnityEngine;

namespace Game.UI {
    [CreateAssetMenu(fileName = "ProgressBarSO", menuName = "Scriptable Objects/Progress Bar Model")]
    public class ProgressBarViewModel : ScriptableObject {
        [SerializeField] FloatVariable _data;
        [CreateProperty]
        public float Value => _data.Value;
    }

}