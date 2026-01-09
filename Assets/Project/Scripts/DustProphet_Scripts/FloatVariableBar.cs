using Obvious.Soap;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Game.Data {
    public class FloatVariableBar : FloatVariable {
        [CreateProperty]
        public StyleLength BarProgress => new StyleLength(new Length(Value * 100f, LengthUnit.Percent)); 
    }
}
