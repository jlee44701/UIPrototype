using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    /// <summary>
    /// Base class for UI elements that display a value/maxValue as a computed progress percent.
    /// Handles: binding hookup, label formatting, change notification, repaint scheduling.
    /// </summary>
    public abstract class BoundProgressElementBase : BindableElement, IDataSourceViewHashProvider
    {
        public static readonly BindingId valueProperty = nameof(value);
        public static readonly BindingId maxValueProperty = nameof(maxValue);

        public static readonly string ussClassName = "progress";
        public static readonly string labelGroupUssClassName = ussClassName + "__label-group";
        public static readonly string valueLabelGroupModifierUssClassName = labelGroupUssClassName + "--value";
        public static readonly string labelUssClassName = ussClassName + "__label";
        

        [UxmlAttribute("value-source-path")]
        public string valueSourcePath { get; set; } = "Value";

        [UxmlAttribute("max-value-source-path")]
        public string maxValueSourcePath { get; set; } = "MaxValue";

        bool m_ShowLabel = true;

        [UxmlAttribute("show-label")]
        public bool showLabel
        {
            get => m_ShowLabel;
            set
            {
                if (m_ShowLabel == value)
                    return;

                m_ShowLabel = value;
                RefreshLabelVisibility();
            }
        }

        [UxmlAttribute("label-format")]
        public string labelFormat { get; set; } = "{0}%";

        float m_Value;
        float m_MaxValue = 100f;

        protected float m_ProgressPercent;
        long m_ViewVersion;

        protected VisualElement m_LabelGroupElement;
        protected VisualElement m_ValueGroupElement;
        protected Label m_LabelElement;

        /// <summary>Bucket for any/all labels an inheritor wants to add.</summary>
        protected VisualElement labelGroupElement => m_LabelGroupElement;

        /// <summary>Bucket specifically for value-related labels (the default value label lives here).</summary>
        protected VisualElement ValueGroupElement => m_ValueGroupElement;

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

        public void SetMaxValue(float maxValue)
        {
            this.maxValue = maxValue;
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

        protected BoundProgressElementBase()
        {
            AddToClassList(ussClassName);

            m_LabelGroupElement = new VisualElement { name = "label-group" };
            m_LabelGroupElement.AddToClassList(labelGroupUssClassName);

            m_ValueGroupElement = new VisualElement { name = "value-label-group" };
            m_ValueGroupElement.AddToClassList(labelGroupUssClassName);
            m_ValueGroupElement.AddToClassList(valueLabelGroupModifierUssClassName);

            m_LabelElement = new Label { name = "value-label" };
            m_LabelElement.AddToClassList(labelUssClassName);

            m_ValueGroupElement.Add(m_LabelElement);
            m_LabelGroupElement.Add(m_ValueGroupElement);
            Add(m_LabelGroupElement);

            RefreshLabelVisibility();

            RegisterCallback<AttachToPanelEvent>(_ => EnsureBindingsIfNeeded());
        }

        void RefreshLabelVisibility()
        {
            if (m_LabelGroupElement == null)
                return;

            m_LabelGroupElement.style.display = m_ShowLabel ? DisplayStyle.Flex : DisplayStyle.None;
            MarkDirtyRepaint();
        }

        void EnsureBindingsIfNeeded()
        {
            if (!string.IsNullOrWhiteSpace(valueSourcePath) && !HasBinding(valueProperty))
            {
                SetBinding(valueProperty, new DataBinding
                {
                    dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(valueSourcePath),
                    bindingMode = BindingMode.ToTarget
                });
            }

            if (!string.IsNullOrWhiteSpace(maxValueSourcePath) && !HasBinding(maxValueProperty))
            {
                SetBinding(maxValueProperty, new DataBinding
                {
                    dataSourcePath = PropertyPathUtility.FromDotSeparatedPath(maxValueSourcePath),
                    bindingMode = BindingMode.ToTarget
                });
            }
        }

        void RefreshComputedProgress()
        {
            var safeMaxValue = Mathf.Max(0.0001f, m_MaxValue);
            var computedPercent = Mathf.Clamp01(m_Value / safeMaxValue) * 100f;

            if (Mathf.Approximately(m_ProgressPercent, computedPercent))
                return;

            m_ProgressPercent = computedPercent;
            ++m_ViewVersion;

            RefreshLabelVisibility();

            if (m_ShowLabel && m_LabelElement != null)
                m_LabelElement.text = string.Format(labelFormat, Mathf.Round(m_ProgressPercent));

            OnProgressChanged(m_ProgressPercent);
            MarkDirtyRepaint();
        }

        protected float GetProgressPercent()
        {
            return m_ProgressPercent;
        }

        protected abstract void OnProgressChanged(float progressPercent);

        public long GetViewHashCode()
        {
            return m_ViewVersion;
        }
    }

    internal static class PropertyPathUtility
    {
        public static PropertyPath FromDotSeparatedPath(string dotSeparatedPath)
        {
            if (string.IsNullOrWhiteSpace(dotSeparatedPath))
                return default;

            var pathParts = dotSeparatedPath.Split('.');
            var propertyPath = PropertyPath.FromName(pathParts[0]);

            for (var index = 1; index < pathParts.Length; index++)
                propertyPath = PropertyPath.AppendName(propertyPath, pathParts[index]);

            return propertyPath;
        }
    }
}
