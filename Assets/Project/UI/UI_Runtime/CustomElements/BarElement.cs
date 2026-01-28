// BarElement.cs
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    [UxmlElement]
    public partial class BarElement : VisualElement
    {
        // Put BarElement.uxml under:
        // Assets/.../Resources/UI/BarElement.uxml
        public const string DefaultVisualTreeResourcePath = "UI/BarElement";

        public const string Float01ToPercentWidthConverterGroupName = "BarElement Float01 To Percent Width";

        private const float DefaultMinimumHeightPixels = 18f;

        private VisualTreeAsset visualTreeAsset;

        private VisualElement barRootElement;
        private VisualElement barValueGroupElement;
        private VisualElement barBackgroundElement;
        private VisualElement barProgressElement;
        private Label barLabelElement;

        private object currentDataSourceObject;

        private string progressDataSourcePathString = "Value";
        private string labelDataSourcePathString = "Value";

        public BarElement()
        {
            if (hierarchy.childCount == 0)
                BuildFromTemplateOrFallback();

            CacheVisualElements();
            ApplyMinimumSizing();

            RegisterCallback<AttachToPanelEvent>(_ => RefreshBindings());
        }

        /// <summary>
        /// General binding entry point.
        /// If dataSourceObject is a FloatVariable-like object with a Value property, the defaults work:
        /// Bind(floatVariableObject)  -> binds "Value" for both label + progress.
        ///
        /// If dataSourceObject is a larger view model, pass nested paths like:
        /// Bind(viewModel, "currentPressure01.Value", "currentPressure01.Value")
        /// </summary>
        public void Bind(object dataSourceObject, string progressDataSourcePath = "Value", string labelDataSourcePath = "Value")
        {
            currentDataSourceObject = dataSourceObject;
            progressDataSourcePathString = progressDataSourcePath;
            labelDataSourcePathString = labelDataSourcePath;

            dataSource = dataSourceObject;

            RefreshBindings();
        }

        public void RefreshBindings()
        {
            if (barProgressElement == null || barLabelElement == null)
                return;

            barProgressElement.ClearBinding("style.width");
            barLabelElement.ClearBinding("text");

            if (currentDataSourceObject == null)
            {
                barProgressElement.style.width = 0;
                barLabelElement.text = string.Empty;
                return;
            }

            dataSource = currentDataSourceObject;

            RegisterConvertersOnce();

            var progressBinding = new DataBinding
            {
                dataSource = currentDataSourceObject,
                dataSourcePath = new PropertyPath(progressDataSourcePathString),
                bindingMode = BindingMode.ToTarget
            };

            if (ConverterGroups.TryGetConverterGroup(Float01ToPercentWidthConverterGroupName, out var converterGroup))
                progressBinding.ApplyConverterGroupToUI(converterGroup);

            barProgressElement.SetBinding("style.width", progressBinding);

            var labelBinding = new DataBinding
            {
                dataSource = currentDataSourceObject,
                dataSourcePath = new PropertyPath(labelDataSourcePathString),
                bindingMode = BindingMode.ToTarget
            };

            barLabelElement.SetBinding("text", labelBinding);
        }

        private void BuildFromTemplateOrFallback()
        {
            RegisterConvertersOnce();

            visualTreeAsset = Resources.Load<VisualTreeAsset>(DefaultVisualTreeResourcePath);

            if (!visualTreeAsset)
            {
                BuildFallbackTreeInCode();
                return;
            }

            visualTreeAsset.CloneTree(this);

            // Make sure any stylesheets referenced by the VisualTreeAsset get attached to this element too.
            // VisualTreeAsset exposes a stylesheets collection. :contentReference[oaicite:4]{index=4}
            foreach (var styleSheet in visualTreeAsset.stylesheets)
            {
                if (!styleSheet)
                    continue;

                styleSheets.Add(styleSheet);
            }
        }

        private void CacheVisualElements()
        {
            barRootElement = this.Q<VisualElement>("bar-element");
            barValueGroupElement = this.Q<VisualElement>("bar-value-group");
            barBackgroundElement = this.Q<VisualElement>("bar-background");
            barProgressElement = this.Q<VisualElement>("bar-progress");
            barLabelElement = this.Q<Label>("bar-label");
        }

        private void ApplyMinimumSizing()
        {
            // Absolute-positioned children do not contribute to parent layout size,
            // so we enforce a minimum height here and in USS. :contentReference[oaicite:5]{index=5}
            style.minHeight = DefaultMinimumHeightPixels;
            style.flexShrink = 0;

            if (barRootElement != null)
            {
                barRootElement.style.minHeight = DefaultMinimumHeightPixels;
                barRootElement.style.flexShrink = 0;
            }

            if (barValueGroupElement != null)
            {
                barValueGroupElement.style.minHeight = DefaultMinimumHeightPixels;
                barValueGroupElement.style.flexShrink = 0;
            }
        }

        private static void RegisterConvertersOnce()
        {
            if (ConverterGroups.TryGetConverterGroup(Float01ToPercentWidthConverterGroupName, out _))
                return;

            var converterGroup = new ConverterGroup(Float01ToPercentWidthConverterGroupName);

            converterGroup.AddConverter((ref float value01) =>
            {
                var clampedValue01 = Mathf.Clamp01(value01);
                var percent = clampedValue01 * 100f;
                return new StyleLength(Length.Percent(percent));
            });

            converterGroup.AddConverter((ref double value01) =>
            {
                var clampedValue01 = Mathf.Clamp01((float)value01);
                var percent = clampedValue01 * 100f;
                return new StyleLength(Length.Percent(percent));
            });

            ConverterGroups.RegisterConverterGroup(converterGroup);
        }

        private void BuildFallbackTreeInCode()
        {
            var rootElement = new VisualElement { name = "bar-element" };
            rootElement.AddToClassList("bar__container");

            var valueGroupElement = new VisualElement { name = "bar-value-group" };
            valueGroupElement.AddToClassList("bar__value-group");

            var backgroundElement = new VisualElement { name = "bar-background" };
            backgroundElement.AddToClassList("bar__background");

            var progressElement = new VisualElement { name = "bar-progress" };
            progressElement.AddToClassList("bar__progress");

            var labelElement = new Label("000") { name = "bar-label" };
            labelElement.AddToClassList("bar__label");

            valueGroupElement.Add(backgroundElement);
            valueGroupElement.Add(progressElement);
            valueGroupElement.Add(labelElement);

            rootElement.Add(valueGroupElement);
            hierarchy.Add(rootElement);
        }
    }
}
