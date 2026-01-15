using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>
    /// Base class for UI elements that display value/maxValue as a computed progress percent.
    /// Handles: binding hookup, label formatting, change notification, repaint.
    /// Provides label buckets:
    /// - {block}__label-group
    /// - {block}__value-group
    /// - {block}__value-label
    /// where {block} is supplied by the concrete control.
    /// </summary>
    public abstract class BoundProgressElementBase : BindableElement, IDataSourceViewHashProvider
    {
        public static readonly BindingId valueProperty = nameof(value);
        public static readonly BindingId maxValueProperty = nameof(maxValue);

        const string k_LabelGroupSuffix = "__label-group";
        const string k_ValueGroupSuffix = "__value-group";
        const string k_ValueLabelSuffix = "__value-label";

        // Concrete controls must provide their “default type class”, eg "bar-progress", "radial-progress".
        protected abstract string controlUssClassName { get; }

        string m_ValueSourcePath = "Value";
        string m_MaxValueSourcePath = "MaxValue";
        bool m_ShowLabel = true;
        string m_LabelFormat = "{0}%";

        float m_Value;
        float m_MaxValue = 100f;

        protected float m_ProgressPercent;
        long m_ViewVersion;

        bool m_AutoBoundValue;
        bool m_AutoBoundMaxValue;

        protected VisualElement m_LabelGroupElement;
        protected VisualElement m_ValueGroupElement;
        protected Label m_ValueLabel;

        protected VisualElement labelGroupElement => m_LabelGroupElement;
        protected VisualElement valueGroupElement => m_ValueGroupElement;
        protected Label valueLabel => m_ValueLabel;

        [UxmlAttribute("value-source-path")]
        public string valueSourcePath
        {
            get => m_ValueSourcePath;
            set
            {
                if (m_ValueSourcePath == value)
                    return;

                m_ValueSourcePath = value;

                if (panel != null)
                    EnsureBindingsIfNeeded(rebindAutoBindings: true);
            }
        }

        [UxmlAttribute("max-value-source-path")]
        public string maxValueSourcePath
        {
            get => m_MaxValueSourcePath;
            set
            {
                if (m_MaxValueSourcePath == value)
                    return;

                m_MaxValueSourcePath = value;

                if (panel != null)
                    EnsureBindingsIfNeeded(rebindAutoBindings: true);
            }
        }

        [UxmlAttribute("show-label")]
        public bool showLabel
        {
            get => m_ShowLabel;
            set
            {
                if (m_ShowLabel == value)
                    return;

                m_ShowLabel = value;
                ApplyLabelVisibility();
                RefreshComputedProgress();
            }
        }

        [UxmlAttribute("label-format")]
        public string labelFormat
        {
            get => m_LabelFormat;
            set
            {
                if (m_LabelFormat == value)
                    return;

                m_LabelFormat = value;
                RefreshComputedProgress();
            }
        }

        [UxmlAttribute, CreateProperty]
        public float value
        {
            get => m_Value;
            set
            {
                if (Mathf.Approximately(m_Value, value))
                    return;

                m_Value = value;
                NotifyPropertyChanged(nameof(value));
                RefreshComputedProgress();
            }
        }

        [UxmlAttribute, CreateProperty]
        public float maxValue
        {
            get => m_MaxValue;
            set
            {
                if (Mathf.Approximately(m_MaxValue, value))
                    return;

                m_MaxValue = value;
                NotifyPropertyChanged(nameof(maxValue));
                RefreshComputedProgress();
            }
        }

        public void SetMaxValue(float maxValueValue)
        {
            maxValue = maxValueValue;
        }

        protected BoundProgressElementBase()
        {
            var block = ResolveBlockClass();
            AddToClassList(block);

            m_LabelGroupElement = new VisualElement { name = "label-group" };
            m_ValueGroupElement = new VisualElement { name = "value-group" };
            m_ValueLabel = new Label { name = "value-label" };

            m_LabelGroupElement.AddToClassList(block + k_LabelGroupSuffix);
            m_ValueGroupElement.AddToClassList(block + k_ValueGroupSuffix);
            m_ValueLabel.AddToClassList(block + k_ValueLabelSuffix);

            m_ValueGroupElement.Add(m_ValueLabel);
            m_LabelGroupElement.Add(m_ValueGroupElement);
            Add(m_LabelGroupElement);

            ApplyLabelVisibility();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                EnsureBindingsIfNeeded(rebindAutoBindings: false);
                RefreshComputedProgress();
            });
        }

        string ResolveBlockClass()
        {
            var block = controlUssClassName;
            if (!string.IsNullOrWhiteSpace(block))
                return block;

            return ToKebabCase(GetType().Name);
        }

        static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "element";

            var sb = new System.Text.StringBuilder(value.Length + 8);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                        sb.Append('-');

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        void ApplyLabelVisibility()
        {
            if (m_LabelGroupElement == null)
                return;

            m_LabelGroupElement.style.display = m_ShowLabel ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void EnsureBindingsIfNeeded(bool rebindAutoBindings)
        {
            if (!string.IsNullOrWhiteSpace(m_ValueSourcePath))
            {
                if (!HasBinding(valueProperty))
                {
                    SetBinding(valueProperty, new DataBinding
                    {
                        dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(m_ValueSourcePath),
                        bindingMode = BindingMode.ToTarget
                    });
                    m_AutoBoundValue = true;
                }
                else if (rebindAutoBindings && m_AutoBoundValue)
                {
                    SetBinding(valueProperty, new DataBinding
                    {
                        dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(m_ValueSourcePath),
                        bindingMode = BindingMode.ToTarget
                    });
                }
            }
            else if (rebindAutoBindings && m_AutoBoundValue)
            {
                ClearBinding(valueProperty);
                m_AutoBoundValue = false;
            }

            if (!string.IsNullOrWhiteSpace(m_MaxValueSourcePath))
            {
                if (!HasBinding(maxValueProperty))
                {
                    SetBinding(maxValueProperty, new DataBinding
                    {
                        dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(m_MaxValueSourcePath),
                        bindingMode = BindingMode.ToTarget
                    });
                    m_AutoBoundMaxValue = true;
                }
                else if (rebindAutoBindings && m_AutoBoundMaxValue)
                {
                    SetBinding(maxValueProperty, new DataBinding
                    {
                        dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(m_MaxValueSourcePath),
                        bindingMode = BindingMode.ToTarget
                    });
                }
            }
            else if (rebindAutoBindings && m_AutoBoundMaxValue)
            {
                ClearBinding(maxValueProperty);
                m_AutoBoundMaxValue = false;
            }
        }

        void RefreshComputedProgress()
        {
            var safeMax = Mathf.Max(0.0001f, m_MaxValue);
            var computedPercent = Mathf.Clamp01(m_Value / safeMax) * 100f;

            if (Mathf.Approximately(m_ProgressPercent, computedPercent))
                return;

            m_ProgressPercent = computedPercent;
            ++m_ViewVersion;

            if (m_ShowLabel && m_ValueLabel != null)
                m_ValueLabel.text = string.Format(m_LabelFormat, Mathf.Round(m_ProgressPercent));

            OnProgressChanged(m_ProgressPercent);
            MarkDirtyRepaint();
        }

        protected float GetProgressPercent() => m_ProgressPercent;

        protected abstract void OnProgressChanged(float progressPercent);

        public long GetViewHashCode() => m_ViewVersion;
    }

    internal static class PropertyPathUtility
    {
        public static PropertyPath FromDotSeparatedPath(string dotSeparatedPath)
        {
            if (string.IsNullOrWhiteSpace(dotSeparatedPath))
                return default;

            var parts = dotSeparatedPath.Split('.');
            var path = PropertyPath.FromName(parts[0]);

            for (var i = 1; i < parts.Length; i++)
                path = PropertyPath.AppendName(path, parts[i]);

            return path;
        }
    }
}
