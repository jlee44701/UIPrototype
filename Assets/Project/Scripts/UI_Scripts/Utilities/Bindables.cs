using Unity.Properties;
using UnityEngine.UIElements;

namespace Game.UI.Utilities
{
    public static class Bindables
    {
        public static void SetBinding(
            VisualElement destination,
            string sourcePath,
            string destinationPath,
            BindingMode bindingMode,
            out DataBinding binding)
        {
            SetBindingInternal(
                element: destination,
                sourcePath: sourcePath,
                destinationPath: destinationPath,
                bindingMode: bindingMode,
                converterGroupString: null,
                out binding);
        }

        public static void SetBindingWithConverter(
            VisualElement destination,
            string sourcePath,
            string destinationPath,
            BindingMode bindingMode,
            string converterGroupString,
            out DataBinding binding)
        {
            SetBindingInternal(
                element: destination,
                sourcePath: sourcePath,
                destinationPath: destinationPath,
                bindingMode: bindingMode,
                converterGroupString: converterGroupString,
                out binding);
        }

        static void SetBindingInternal(
            VisualElement element,
            string sourcePath,
            string destinationPath,
            BindingMode bindingMode,
            string converterGroupString,
            out DataBinding binding)
        {
            binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(sourcePath),
                bindingMode = bindingMode
            };

            if (!string.IsNullOrEmpty(converterGroupString))
                ApplyConverterGroup(binding, converterGroupString);

            element.SetBinding(destinationPath, binding);

            return;
            static void ApplyConverterGroup(DataBinding binding, string converterGroupString)
            {
                if (!ConverterGroups.TryGetConverterGroup(converterGroupString, out var converterGroup)) return;

                binding.ApplyConverterGroupToUI(converterGroup);
            }
        }

        
    }
}
