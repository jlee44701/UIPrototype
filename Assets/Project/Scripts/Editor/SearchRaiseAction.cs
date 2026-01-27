#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

static class SearchRaiseAction
{
    [SearchActionsProvider]
    static IEnumerable<SearchAction> ActionProvider()
    {
        return new[]
        {
            new SearchAction("asset", "raise", new GUIContent("Raise"))
            {
                handler = item =>
                {
                    var obj = item.ToObject() as ScriptableObject;
                    if (!obj) return;

                    var mi = obj.GetType().GetMethod("Raise",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, Type.EmptyTypes, null);

                    if (mi == null) return;

                    Undo.RecordObject(obj, "Raise");
                    mi.Invoke(obj, null);
                    EditorUtility.SetDirty(obj);
                }
            }
        };
    }
}
#endif
