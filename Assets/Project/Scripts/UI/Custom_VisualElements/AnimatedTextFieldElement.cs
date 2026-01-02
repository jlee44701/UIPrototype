using System.Reflection;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using UnityEngine.UIElements;

namespace RuntimeUI
{
    [UxmlElement]
    public partial class AnimatedTextFieldElement : VisualElement
    {
        const string AnimatedLabelElementName = "animated-label";
        const string DefaultTimingsResourcesPath = "AnimatedText/Delays_By_Character";

        AnimatedLabel m_AnimatedLabel;
        TypingsTimingsScriptableBase m_DefaultTimings;
        TypingsTimingsScriptableBase m_TimingsOverride;
        string m_Text;

        [UxmlAttribute("text")]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                TryPlayTypewriter();
            }
        }

        public AnimatedLabel AnimatedLabel => m_AnimatedLabel;

        public AnimatedTextFieldElement()
        {
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (!Application.isPlaying)
                    return;

                EnsureAnimatedLabelExists();
                EnsureTimingsAssigned();
                TryPlayTypewriter();
            });
        }

        public AnimatedTextFieldElement(TypingsTimingsScriptableBase timingsTimings) : this()
        {
            SetTimings(timingsTimings, restartIfTextAlreadySet: false);
        }

        public void SetTimings(TypingsTimingsScriptableBase timingsTimings, bool restartIfTextAlreadySet = true)
        {
            m_TimingsOverride = timingsTimings;

            if (!Application.isPlaying)
                return;

            EnsureAnimatedLabelExists();
            EnsureTimingsAssigned();

            if (restartIfTextAlreadySet)
                TryPlayTypewriter();
        }

        void EnsureAnimatedLabelExists()
        {
            if (m_AnimatedLabel != null)
                return;

            m_AnimatedLabel = this.Q<AnimatedLabel>(AnimatedLabelElementName);

            if (m_AnimatedLabel != null)
                return;

            m_AnimatedLabel = new AnimatedLabel { name = AnimatedLabelElementName };
            hierarchy.Add(m_AnimatedLabel);
        }

        void EnsureTimingsAssigned()
        {
            var timingsToUse = m_TimingsOverride;
            if (!timingsToUse)
            {
                if (!m_DefaultTimings)
                    m_DefaultTimings = Resources.Load<TypingsTimingsScriptableBase>(DefaultTimingsResourcesPath);

                timingsToUse = m_DefaultTimings;
            }

            if (!timingsToUse || m_AnimatedLabel == null)
                return;

            // Resources.Load path is relative to a Resources folder and omits extension. :contentReference[oaicite:2]{index=2}
            TryAssignFirstTimingsSlot(m_AnimatedLabel, timingsToUse);
            TryAssignFirstTimingsSlot(m_AnimatedLabel.Typewriter, timingsToUse);
        }

        static void TryAssignFirstTimingsSlot(object targetObject, TypingsTimingsScriptableBase timingsAsset)
        {
            if (targetObject == null || !timingsAsset)
                return;

            var timingsBaseType = typeof(TypingsTimingsScriptableBase);
            var targetType = targetObject.GetType();

            foreach (var propertyInfo in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!propertyInfo.CanWrite)
                    continue;

                if (!timingsBaseType.IsAssignableFrom(propertyInfo.PropertyType))
                    continue;

                if (propertyInfo.GetValue(targetObject) is UnityEngine.Object existing && existing)
                    return;

                propertyInfo.SetValue(targetObject, timingsAsset);
                return;
            }

            foreach (var fieldInfo in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!timingsBaseType.IsAssignableFrom(fieldInfo.FieldType))
                    continue;

                if (fieldInfo.GetValue(targetObject) is UnityEngine.Object existing && existing)
                    return;

                fieldInfo.SetValue(targetObject, timingsAsset);
                return;
            }
        }

        void TryPlayTypewriter()
        {
            if (!Application.isPlaying)
                return;

            if (m_AnimatedLabel == null)
                return;

            if (string.IsNullOrEmpty(m_Text))
                return;

            m_AnimatedLabel.Typewriter.ShowText(m_Text);
            m_AnimatedLabel.Typewriter.StartShowingText(true);
        }
    }
}
