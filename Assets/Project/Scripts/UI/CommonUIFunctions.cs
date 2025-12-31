
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PixelEngine {
    public static class SerializedObjectHelpers {
        // 
        // ***************************************
        // SERIALIZED OBJECT CREATION
        // ***************************************
        public static void CreateAndUpdateSerializedObject(
            this UnityEngine.Object targetObject,
            out SerializedObject serializedObject,
            bool updateIfRequired = true
        ) {
            if (!targetObject) {
                throw new NullReferenceException(
                    $"{nameof(CreateAndUpdateSerializedObject)}: targetObject is null."
                );
            }

            serializedObject = new SerializedObject(targetObject);

           
            if (updateIfRequired) {
                serializedObject.UpdateIfRequiredOrScript();
            }
            else {
                serializedObject.Update();
            }
        }

        // 
        // ***************************************
        // REQUIRED PROPERTY VALIDATION
        // ***************************************
        public static void ValidateRequiredProperties(
            this SerializedObject serializedObject,
            string firstPropertyName,
            out SerializedProperty firstProperty,
            string secondPropertyName,
            out SerializedProperty secondProperty
        ) {
            ValidateSerializedObjectNotNull(serializedObject);

            firstProperty = RequireProperty(serializedObject, firstPropertyName);
            secondProperty = RequireProperty(serializedObject, secondPropertyName);
        }

        public static void ValidateRequiredProperties(
            this SerializedObject serializedObject,
            string firstPropertyName,
            out SerializedProperty firstProperty,
            string secondPropertyName,
            out SerializedProperty secondProperty,
            string thirdPropertyName,
            out SerializedProperty thirdProperty
        ) {
            ValidateSerializedObjectNotNull(serializedObject);

            firstProperty = RequireProperty(serializedObject, firstPropertyName);
            secondProperty = RequireProperty(serializedObject, secondPropertyName);
            thirdProperty = RequireProperty(serializedObject, thirdPropertyName);
        }

        static void ValidateSerializedObjectNotNull(SerializedObject serializedObject) {
            if (serializedObject == null) {
                throw new NullReferenceException("SerializedObject is null.");
            }

            // targetObject can become null if the inspected object was destroyed/reloaded.
            if (!serializedObject.targetObject) {
                throw new NullReferenceException("SerializedObject.targetObject is null (destroyed or reloaded).");
            }
        }

        static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName) {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null) {
                var targetName = serializedObject.targetObject ? serializedObject.targetObject.name : "<null>";
                var targetTypeName = serializedObject.targetObject ? serializedObject.targetObject.GetType().FullName : "<null>";

                throw new NullReferenceException(
                    $"Required property '{propertyName}' was not found on '{targetName}' ({targetTypeName})."
                );
            }

            return property;
        }

        public static void AddInspectorToElement(VisualElement container, SerializedObject serializedObject) {
            if (container == null) {
                throw new NullReferenceException(nameof(container));
            }
            if (serializedObject == null) {
                throw new NullReferenceException(nameof(serializedObject));
            }

            // SerializedObject can exist while its target is gone.
            // targetObject is a UnityEngine.Object, so use Unity's lifetime check.
            if (!serializedObject.targetObject) {
                return;
            }

            // If you’re rebuilding this area repeatedly, clear first.
            container.Clear();

            
            var inspectorElement = new InspectorElement();
            inspectorElement.Unbind();
            inspectorElement.Bind(serializedObject);

            container.Add(inspectorElement);
        }
        // public static void BuildInspectorWithHiddenCheck(SerializedObject serializedObject, VisualElement rootVisualElement) {
        //     if (serializedObject == null) throw new NullReferenceException(nameof(serializedObject));
        //     if (rootVisualElement == null) throw new NullReferenceException(nameof(rootVisualElement));
        //     if (!serializedObject.targetObject) return;
        //
        //     rootVisualElement.Unbind();
        //     rootVisualElement.Clear();
        //
        //     serializedObject.UpdateIfRequiredOrScript();
        //
        //     var iterator = serializedObject.GetIterator();
        //     var enterChildren = true;
        //
        //     while (iterator.NextVisible(enterChildren)) {
        //         enterChildren = false;
        //
        //         if (iterator.propertyPath == "m_Script") {
        //             continue;
        //         }
        //
        //         if (!CustomHiddenInspectorSettings.ShowHiddenFields) {
        //             var memberAttributes = EditorUtils.GetMemberAttributes(iterator);
        //             if (memberAttributes != null) {
        //                 for (var attributeIndex = 0; attributeIndex < memberAttributes.Length; attributeIndex++) {
        //                     if (memberAttributes[attributeIndex] is HideInInspectorCustomAttribute) {
        //                         goto SkipProperty;
        //                     }
        //                 }
        //             }
        //         }
        //
        //         var propertyCopy = iterator.Copy();
        //         var propertyField = new PropertyField();
        //         propertyField.BindProperty(propertyCopy);
        //         rootVisualElement.Add(propertyField);
        //
        //         continue;
        //
        //         SkipProperty: ;
        //     }
        // }
        public static void BuildInspectorWithoutScriptField(SerializedObject serializedObject, VisualElement rootVisualElement) {
            if (serializedObject == null) {
                throw new NullReferenceException(nameof(serializedObject));
            }
            if (rootVisualElement == null) {
                throw new NullReferenceException(nameof(rootVisualElement));
            }
            if (!serializedObject.targetObject) {
                return;
            }

            rootVisualElement.Unbind();
            rootVisualElement.Clear();

            serializedObject.UpdateIfRequiredOrScript();

            var iterator = serializedObject.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                
                    if (iterator.propertyPath == "m_Script") {
                        continue;
                    }

                var propertyCopy = iterator.Copy();
                var propertyField = new PropertyField();
                propertyField.BindProperty(propertyCopy);

                rootVisualElement.Add(propertyField);
            }
        }

        public static void AddInspectorToElement(VisualElement container, Object unityObject) {
            if (container == null) throw new NullReferenceException(nameof(container));
            if (!unityObject) throw new NullReferenceException(nameof(unityObject));

            SerializedObject serializedObject;

            try {
                serializedObject = new SerializedObject(unityObject);
            }
            catch (Exception exception) {
                Debug.LogWarning(
                    $"AddInspectorToElement: cannot create SerializedObject for {unityObject.GetType().Name} ({unityObject.name}).\n{exception}"
                );
                return;
            }
            var inspectorElement = new InspectorElement();
            inspectorElement.Bind(serializedObject);

            container.Add(inspectorElement);
        }
        
        public static void AddInspectorToCachedElement(
            VisualElement container,
            ref InspectorElement cachedInspectorElement,
            SerializedObject serializedObject
        ) {
            if (container == null) {
                throw new NullReferenceException(nameof(container));
            }
            if (serializedObject == null) {
                throw new NullReferenceException(nameof(serializedObject));
            }
            if (!serializedObject.targetObject) {
                return;
            }

            // If this container was cleared since last time, the cached inspector is no longer parented.
            if (cachedInspectorElement == null) {
                cachedInspectorElement = new InspectorElement();
            }

            if (cachedInspectorElement.parent != container) {
                cachedInspectorElement.RemoveFromHierarchy();
                container.Clear();
                container.Add(cachedInspectorElement);
            }
            else {
                cachedInspectorElement.Unbind(); // required before rebinding :contentReference[oaicite:0]{index=0}
            }

            serializedObject.UpdateIfRequiredOrScript();

            cachedInspectorElement.Bind(serializedObject); // generates PropertyFields on bind :contentReference[oaicite:1]{index=1}
        }
    }
    
    
}
