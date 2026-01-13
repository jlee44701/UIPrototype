using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Converters {
    public static class PercentageConverter {
        public static void RegisterPercentage(string propertyString) {
            var percentageConverter = new ConverterGroup(propertyString);
            percentageConverter.AddConverter((ref float percentageValue) =>
            {
                var percentage = Mathf.RoundToInt(percentageValue * 100);
                return percentage + "%";
            });
            ConverterGroups.RegisterConverterGroup(percentageConverter);
        }

        public static class Conversions
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            private static void RegisterBindingConverters()
            {
                var normalized01ToPercentConverterGroup = new ConverterGroup("Normalized01ToPercent");
                normalized01ToPercentConverterGroup.AddConverter((ref float normalizedValue) =>
                {
                    normalizedValue = Mathf.Clamp01(normalizedValue) * 100f;
                    return normalizedValue + "%";
                });

                ConverterGroups.RegisterConverterGroup(normalized01ToPercentConverterGroup);
            }
        }  
    }
  
}