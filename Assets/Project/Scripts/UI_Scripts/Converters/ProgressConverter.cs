using Game.UI.Library;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Converters {
    public static class ProgressConverter {
        
        static readonly Color s_FullColor = new Color(0.2f, 1f, 0.2f);
        static readonly Color s_MidColor = Color.yellow;
        static readonly Color s_LowColor = new Color(1f, 0.3f, 0f);
        static readonly Color s_CriticalColor = Color.red;
        
        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod] // Register in Editor for UI Builder
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Ensure it's registered at runtime
#endif
        
        public static void RegisterConverters()
        {
            RegisterProgressColorConverter();
            RegisterProgressInverseColorConverter();
        }
        
        static void RegisterProgressColorConverter()
        {
            var converterGroup = new ConverterGroup("progressColor");

            // We return Color because RadialProgress.progressColor is Color.
            converterGroup.AddConverter((ref float progressPercentage) =>
            {
                if (progressPercentage > 0.5f)
                {
                    return Color.Lerp(s_MidColor, s_FullColor, (progressPercentage - 0.5f) * 2f);
                }

                if (progressPercentage > 0.25f)
                {
                    return Color.Lerp(s_LowColor, s_MidColor, (progressPercentage - 0.25f) * 4f);
                }

                return Color.Lerp(s_CriticalColor, s_LowColor, progressPercentage * 4f);
            });

            ConverterGroups.RegisterConverterGroup(converterGroup);
        }
        
        static void RegisterProgressInverseColorConverter()
        {
            var converterGroup = new ConverterGroup("progressColorInverse");

            // We return Color because RadialProgress.progressColor is Color.
            converterGroup.AddConverter((ref float progressPercentage) =>
            {
                if (progressPercentage > 0.5f)
                {
                    return Color.Lerp(s_LowColor, s_CriticalColor, (progressPercentage - 0.5f) * 2f);
                }

                if (progressPercentage > 0.25f)
                {
                    return Color.Lerp(s_MidColor, s_LowColor, (progressPercentage - 0.25f) * 4f);
                }

                return Color.Lerp(s_FullColor, s_MidColor, progressPercentage * 4f);
            });

            ConverterGroups.RegisterConverterGroup(converterGroup);
        }
        

    }

}