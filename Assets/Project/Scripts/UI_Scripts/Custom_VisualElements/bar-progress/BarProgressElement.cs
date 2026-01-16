using Unity.Properties;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Game.UI.Library
{
    [UxmlElement]
    public partial class BarProgressElement : Game.UI.BoundProgressElementBase
    {
        public static readonly string ussClassName = "bar-progress";
        public static readonly string containerUssClassName = ussClassName + "__container";
        public static readonly string backgroundUssClassName = ussClassName + "__background";
        public static readonly string fillUssClassName = ussClassName + "__fill";

        protected override string controlUssClassName => ussClassName;
        protected override void OnProgressChanged(float progressPercent) {
            if (m_FillElement == null)
                return;

            m_FillElement.style.width = new Length(progressPercent, LengthUnit.Percent);
        }
        

        VisualElement m_ContainerElement;
        VisualElement m_BackgroundElement;
        VisualElement m_FillElement;

        
    AsyncOperationHandle<VisualTreeAsset>? _uxmlHandle;
    bool _built;
    const string DefaultTemplateKey = "bar-progress-element.uxml";
    const string DefaultStyleKey = "bar-progress-element.uss";
    [UxmlAttribute("template-key")]
    public string templateKey { get; set; } = DefaultTemplateKey;

    public BarProgressElement()
    {
        RegisterCallback<AttachToPanelEvent>(OnAttach);
        RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }

    void OnAttach(AttachToPanelEvent _)
    {
        if (_built || _uxmlHandle.HasValue)
            return;

        var key = string.IsNullOrEmpty(templateKey) ? DefaultTemplateKey : templateKey;

        var handle = Addressables.LoadAssetAsync<VisualTreeAsset>(key);
        _uxmlHandle = handle;

        handle.Completed += op =>
        {
            if (!_uxmlHandle.HasValue || !op.Equals(_uxmlHandle.Value))
                return;

            if (panel == null)
                return;

            if (op.Status != AsyncOperationStatus.Succeeded || !op.Result)
            {
                Debug.LogError($"Failed to load UXML VisualTreeAsset key='{key}'. {op.OperationException}");
                return;
            }

            contentContainer.Clear();
            op.Result.CloneTree(contentContainer);

            m_ContainerElement = contentContainer.Q<VisualElement>(containerUssClassName);
            m_BackgroundElement = contentContainer.Q<VisualElement>(backgroundUssClassName);
            m_FillElement = contentContainer.Q<VisualElement>(fillUssClassName);

            if (m_ContainerElement == null || m_BackgroundElement == null || m_FillElement == null)
                Debug.LogError("BarProgressElement template is missing expected named elements.");

            _built = true;
            OnProgressChanged(GetProgressPercent());
        };
    }

    void OnDetach(DetachFromPanelEvent _)
    {
        _built = false;
        contentContainer.Clear();

        if (_uxmlHandle.HasValue)
        {
            Addressables.Release(_uxmlHandle.Value);
            _uxmlHandle = null;
        }

        m_ContainerElement = null;
        m_BackgroundElement = null;
        m_FillElement = null;
    }


        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => m_FillElement != null ? m_FillElement.style.backgroundColor.value : default;
            set
            {
                if (m_FillElement == null)
                    return;

                m_FillElement.style.backgroundColor = value;
            }
        }

        [UxmlAttribute, CreateProperty]
        public Color backgroundColor
        {
            get => m_BackgroundElement != null ? m_BackgroundElement.style.backgroundColor.value : default;
            set
            {
                if (m_BackgroundElement == null)
                    return;

                m_BackgroundElement.style.backgroundColor = value;
            }
        }
    }
}
