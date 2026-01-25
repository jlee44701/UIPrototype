using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public abstract class BoundProgressElementBase : BindableElement, IDataSourceViewHashProvider
    {
        
        public static readonly BindingId valueProperty = nameof(value);
        public static readonly BindingId maxValueProperty = nameof(maxValue);
        public static readonly BindingId labelProperty = nameof(label);

        protected const string k_ContainerSuffix = "__container";
        protected const string k_LabelGroupSuffix = "__label-group";
        protected const string k_LabelSuffix = "__label";
        protected const string k_ValueGroupSuffix = "__value-group";
        protected const string k_ValueLabelSuffix = "__value-label";

        protected abstract string controlUssClassName { get; }

        float m_Value;
        float m_MaxValue = 100f;
        float m_ProgressPercent;
        long m_ViewVersion;

        string m_LabelText = "";

        readonly string m_Block;
        readonly VisualElement m_Container;
        readonly VisualElement m_LabelGroup;
        readonly Label m_LabelElement;
        readonly VisualElement m_ValueGroup;
        readonly Label m_ValueLabel;

        public override VisualElement contentContainer => m_Container;

        protected string BlockClass => m_Block;

        protected readonly VisualElement ProgressLabelGroup;
        protected readonly VisualElement LabelElement;
        protected readonly VisualElement ValueGroup;
        protected readonly VisualElement ValueLabel;

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

        // Distinct from value label. Stable part name/class: {block}__label
        [UxmlAttribute("label"), CreateProperty]
        public string label
        {
            get => m_LabelText;
            set
            {
                value ??= "";

                if (m_LabelText == value)
                    return;

                m_LabelText = value;
                NotifyPropertyChanged(nameof(label));

                if (m_LabelElement != null)
                    m_LabelElement.text = m_LabelText;

                ++m_ViewVersion;
            }
        }

        protected BoundProgressElementBase()
        {
            m_Block = string.IsNullOrWhiteSpace(controlUssClassName)
                ? ProgressElementNaming.ToKebabCase(GetType().Name)
                : controlUssClassName;

            AddToClassList(m_Block);

            m_Container = new VisualElement { name = m_Block + k_ContainerSuffix };
            m_Container.AddToClassList(m_Block + k_ContainerSuffix);
            hierarchy.Add(m_Container);

            m_LabelGroup = new VisualElement { name = m_Block + k_LabelGroupSuffix };
            m_LabelGroup.AddToClassList(m_Block + k_LabelGroupSuffix);
            hierarchy.Add(m_LabelGroup);

            m_LabelElement = new Label { name = m_Block + k_LabelSuffix };
            m_LabelElement.AddToClassList(m_Block + k_LabelSuffix);
            m_LabelElement.text = m_LabelText;

            m_ValueGroup = new VisualElement { name = m_Block + k_ValueGroupSuffix };
            m_ValueGroup.AddToClassList(m_Block + k_ValueGroupSuffix);

            m_ValueLabel = new Label { name = m_Block + k_ValueLabelSuffix };
            m_ValueLabel.AddToClassList(m_Block + k_ValueLabelSuffix);

            m_ValueGroup.Add(m_ValueLabel);

            // Direct children of label-group: label + value group
            m_LabelGroup.Add(m_LabelElement);
            m_LabelGroup.Add(m_ValueGroup);

            ProgressLabelGroup = m_LabelGroup;
            LabelElement = m_LabelElement;
            ValueGroup = m_ValueGroup;
            ValueLabel = m_ValueLabel;

            RegisterCallback<AttachToPanelEvent>(_ => RefreshComputedProgress());
        }

        void RefreshComputedProgress()
        {
            var safeMax = Mathf.Max(0.0001f, m_MaxValue);
            var computedPercent = Mathf.Clamp01(m_Value / safeMax) * 100f;

            if (Mathf.Approximately(m_ProgressPercent, computedPercent))
                return;

            m_ProgressPercent = computedPercent;
            ++m_ViewVersion;

            if (m_ValueLabel != null)
                m_ValueLabel.text = $"{Mathf.Round(m_ProgressPercent)}%";

            OnProgressChanged(m_ProgressPercent);
            MarkDirtyRepaint();
        }

        protected float GetProgressPercent() => m_ProgressPercent;

        protected abstract void OnProgressChanged(float progressPercent);

        public long GetViewHashCode() => m_ViewVersion;
    }

    internal static class ProgressElementNaming
    {
        public static string ToKebabCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
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
                    continue;
                }

                if (char.IsWhiteSpace(c) || c == '_')
                {
                    sb.Append('-');
                    continue;
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString().Trim('-');
        }
    }
}
