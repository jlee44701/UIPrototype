using Unity.Properties;
using UnityEngine.UIElements;

namespace Game.UI.Utilities
{
    public static class Bind
    {
        public static void SetBinding(
            VisualElement destination,
            string sourcePath,
            string destinationPath,
            BindingMode bindingMode,
            out DataBinding binding)
        {
            SetBindingInternal(
                destination: destination,
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
                destination: destination,
                sourcePath: sourcePath,
                destinationPath: destinationPath,
                bindingMode: bindingMode,
                converterGroupString: converterGroupString,
                out binding);
        }

        static void SetBindingInternal(
            VisualElement destination,
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

            TryApplyConverterGroup(binding, converterGroupString);

            destination.SetBinding(destinationPath, binding);
        }

        static void TryApplyConverterGroup(DataBinding binding, string converterGroupString)
        {
            if (string.IsNullOrEmpty(converterGroupString)) return;

            if (!ConverterGroups.TryGetConverterGroup(converterGroupString, out var converterGroup)) return;

            binding.ApplyConverterGroupToUI(converterGroup);
        }
    }
}
