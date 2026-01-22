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
        public static readonly string backgroundUssClassName = ussClassName + "__background";
        public static readonly string fillUssClassName = ussClassName + "__fill";
        public const string DefaultFontKey = "ShareTechMono-Upper";
        protected override string controlUssClassName => ussClassName;

        const string DefaultTemplateKey = "bar-progress-element.uxml";
        const string DefaultStyleKey = "bar-progress-element.uss";

        [UxmlAttribute("template-key")]
        public string templateKey { get; set; } = DefaultTemplateKey;

        [UxmlAttribute("style-key")]
        public string styleKey { get; set; } = DefaultStyleKey;

        VisualElement m_BackgroundElement;
        VisualElement m_FillElement;

        bool m_Built;

        AsyncOperationHandle<VisualTreeAsset>? m_UxmlHandle;
        AsyncOperationHandle<StyleSheet>? m_UssHandle;

        StyleSheet m_LoadedStyle;

        Color m_ProgressColor;
        bool m_HasProgressColor;

        Color m_BackgroundColor;
        bool m_HasBackgroundColor;

        public BarProgressElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        protected override void OnProgressChanged(float progressPercent)
        {
            if (m_FillElement == null)
                return;

            m_FillElement.style.width = new Length(progressPercent, LengthUnit.Percent);
            
            
        }

        [UxmlAttribute, CreateProperty]
        public Color progressColor
        {
            get => m_FillElement != null ? m_FillElement.style.backgroundColor.value : m_ProgressColor;
            set
            {
                m_ProgressColor = value;
                m_HasProgressColor = true;

                if (m_FillElement != null)
                    m_FillElement.style.backgroundColor = value;
            }
        }


        
        [UxmlAttribute, CreateProperty]
        public Color backgroundColor
        {
            get => m_BackgroundElement != null ? m_BackgroundElement.style.backgroundColor.value : m_BackgroundColor;
            set
            {
                m_BackgroundColor = value;
                m_HasBackgroundColor = true;

                if (m_BackgroundElement != null)
                    m_BackgroundElement.style.backgroundColor = value;
            }
        }

        void OnAttach(AttachToPanelEvent _)
        {
            if (m_Built || m_UxmlHandle.HasValue || m_UssHandle.HasValue)
                return;

            var uxmlKey = string.IsNullOrEmpty(templateKey) ? DefaultTemplateKey : templateKey;
            var ussKey = string.IsNullOrEmpty(styleKey) ? DefaultStyleKey : styleKey;

            m_UxmlHandle = Addressables.LoadAssetAsync<VisualTreeAsset>(uxmlKey);
            m_UssHandle = Addressables.LoadAssetAsync<StyleSheet>(ussKey);

            m_UssHandle.Value.Completed += OnStyleLoaded;
            m_UxmlHandle.Value.Completed += OnTemplateLoaded;
        }

        void OnStyleLoaded(AsyncOperationHandle<StyleSheet> op)
        {
            if (!m_UssHandle.HasValue || !op.Equals(m_UssHandle.Value))
                return;

            if (panel == null)
                return;

            if (op.Status != AsyncOperationStatus.Succeeded || !op.Result)
            {
                Debug.LogError($"Failed to load USS StyleSheet key='{styleKey}'. {op.OperationException}");
                return;
            }

            if (m_LoadedStyle != null)
                styleSheets.Remove(m_LoadedStyle);

            m_LoadedStyle = op.Result;

            if (!styleSheets.Contains(m_LoadedStyle))
                styleSheets.Add(m_LoadedStyle);
        }

        void OnTemplateLoaded(AsyncOperationHandle<VisualTreeAsset> op)
        {
            if (!m_UxmlHandle.HasValue || !op.Equals(m_UxmlHandle.Value))
                return;

            if (panel == null)
                return;

            if (op.Status != AsyncOperationStatus.Succeeded || !op.Result)
            {
                Debug.LogError($"Failed to load UXML VisualTreeAsset key='{templateKey}'. {op.OperationException}");
                return;
            }

            // contentContainer is bar-progress__container now (created by the base).
            contentContainer.Clear();
            op.Result.CloneTree(contentContainer);

            m_BackgroundElement = contentContainer.Q<VisualElement>(backgroundUssClassName);
            m_FillElement = contentContainer.Q<VisualElement>(fillUssClassName);

            if (m_BackgroundElement == null || m_FillElement == null)
                Debug.LogError("BarProgressElement template is missing expected named elements.");

            if (m_HasBackgroundColor && m_BackgroundElement != null)
                m_BackgroundElement.style.backgroundColor = m_BackgroundColor;

            if (m_HasProgressColor && m_FillElement != null)
                m_FillElement.style.backgroundColor = m_ProgressColor;

            m_Built = true;
            OnProgressChanged(GetProgressPercent());
        }

        void OnDetach(DetachFromPanelEvent _)
        {
            m_Built = false;

            contentContainer.Clear();

            if (m_LoadedStyle != null)
            {
                styleSheets.Remove(m_LoadedStyle);
                m_LoadedStyle = null;
            }

            if (m_UxmlHandle.HasValue)
            {
                Addressables.Release(m_UxmlHandle.Value);
                m_UxmlHandle = null;
            }

            if (m_UssHandle.HasValue)
            {
                Addressables.Release(m_UssHandle.Value);
                m_UssHandle = null;
            }

            m_BackgroundElement = null;
            m_FillElement = null;
        }
    }
}
