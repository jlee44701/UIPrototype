// #if UNITY_EDITOR
// using System;
// using UnityEditor;
// using UnityEngine;
//
// [CustomPropertyDrawer(typeof(ScriptableObject), true)]
// public class AnyScriptableObjectInlineDrawer : PropertyDrawer
// {
//     private const float BoxPadX = 6f;
//     private const float BoxPadY = 4f;
//     private const float VerticalSpacing = 2f;
//     private const float FallbackBodyHeight = 120f;
//
//     private Editor _editor;
//     private UnityEngine.Object _editorTarget;
//
//     public override void OnGUI(Rect totalRect, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(totalRect, label, property);
//
//         var lineH = EditorGUIUtility.singleLineHeight;
//         var headerRect = new Rect(totalRect.x, totalRect.y, totalRect.width, lineH);
//
//         var indented = EditorGUI.IndentedRect(headerRect);
//         var labelW = EditorGUIUtility.labelWidth;
//         var labelRect = new Rect(indented.x, indented.y, labelW, indented.height);
//         var fieldRect = new Rect(indented.x + labelW, indented.y, indented.width - labelW, indented.height);
//
//         var openKey = BuildOpenKey(property);
//         var isOpen = SessionState.GetBool(openKey, false);
//         var nextOpen = EditorGUI.Foldout(labelRect, isOpen, label, true);
//         if (nextOpen != isOpen)
//         {
//             SessionState.SetBool(openKey, nextOpen);
//             isOpen = nextOpen;
//         }
//
//         Type objType = typeof(ScriptableObject);
//         if (fieldInfo != null && typeof(ScriptableObject).IsAssignableFrom(fieldInfo.FieldType))
//             objType = fieldInfo.FieldType;
//
//         var allowSceneObjects = !EditorUtility.IsPersistent(property.serializedObject.targetObject);
//
//         EditorGUI.BeginChangeCheck();
//         var newRef = EditorGUI.ObjectField(fieldRect, GUIContent.none, property.objectReferenceValue, objType, allowSceneObjects);
//         if (EditorGUI.EndChangeCheck())
//             property.objectReferenceValue = newRef;
//
//         if (!isOpen || !property.objectReferenceValue)
//         {
//             EditorGUI.EndProperty();
//             return;
//         }
//
//         var bodyKey = BuildBodyHeightKey(property);
//         var bodyH = SessionState.GetFloat(bodyKey, FallbackBodyHeight);
//
//         var boxRect = new Rect(
//             totalRect.x,
//             headerRect.yMax + VerticalSpacing,
//             totalRect.width,
//             bodyH
//         );
//
//         GUI.Box(boxRect, GUIContent.none, (GUIStyle)"HelpBox");
//
//         var contentRect = new Rect(
//             boxRect.x + BoxPadX,
//             boxRect.y + BoxPadY,
//             boxRect.width - 2f * BoxPadX,
//             boxRect.height - 2f * BoxPadY
//         );
//
//         var target = property.objectReferenceValue;
//         if (_editorTarget != target)
//         {
//             _editorTarget = target;
//             _editor = null;
//         }
//
//         Editor.CreateCachedEditor(_editorTarget, null, ref _editor);
//
//         // Draw the real inspector for the referenced asset (buttons, custom inspectors, etc).
//         GUILayout.BeginArea(contentRect);
//         _editor.OnInspectorGUI();
//         
//
//         // Cache height on repaint so GetPropertyHeight can expand correctly.
//         if (Event.current.type == EventType.Repaint)
//         {
//             var used = GUILayoutUtility.GetLastRect().yMax + 2f * BoxPadY;
//             used = Mathf.Max(FallbackBodyHeight, Mathf.Ceil(used));
//
//             var prev = SessionState.GetFloat(bodyKey, FallbackBodyHeight);
//             if (Mathf.Abs(prev - used) > 0.5f)
//             {
//                 SessionState.SetFloat(bodyKey, used);
//                 GUI.changed = true;
//             }
//         }
//
//         GUILayout.EndArea();
//
//         EditorGUI.EndProperty();
//     }
//
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         var h = EditorGUIUtility.singleLineHeight;
//
//         if (property.propertyType != SerializedPropertyType.ObjectReference)
//             return h;
//
//         if (!property.objectReferenceValue)
//             return h;
//
//         var openKey = BuildOpenKey(property);
//         if (!SessionState.GetBool(openKey, false))
//             return h;
//
//         var bodyKey = BuildBodyHeightKey(property);
//         var bodyH = SessionState.GetFloat(bodyKey, FallbackBodyHeight);
//
//         return h + VerticalSpacing + bodyH;
//     }
//
//     private static string BuildOpenKey(SerializedProperty property)
//     {
//         var host = property.serializedObject.targetObject;
//         var hostId = host.GetInstanceID().ToString();
//         return $"InlineSO:Open:{hostId}:{property.propertyPath}";
//     }
//
//     private static string BuildBodyHeightKey(SerializedProperty property)
//     {
//         var host = property.serializedObject.targetObject;
//         var hostId = host.GetInstanceID().ToString();
//         return $"InlineSO:BodyH:{hostId}:{property.propertyPath}";
//     }
// }
// #endif
