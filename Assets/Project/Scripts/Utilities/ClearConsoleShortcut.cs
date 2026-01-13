#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class ConsoleClearShortcut
{
    private const string ShortcutIdentifier = "Cheryl/Clear Console";

    // Uses Unity's Shortcut system (shows up in Edit -> Shortcuts...)
    [Shortcut(ShortcutIdentifier, KeyCode.C, ShortcutModifiers.Control | ShortcutModifiers.Alt)]
    private static void ClearConsoleFromShortcut()
    {
        ClearConsoleLogs();
    }

    // Also adds a menu item so we can trigger it manually.
    [MenuItem("Tools/Clear Console")]
    private static void ClearConsoleFromMenu()
    {
        ClearConsoleLogs();
    }

    private static void ClearConsoleLogs()
    {
        var logEntriesType =
            Type.GetType("UnityEditor.LogEntries, UnityEditor.dll") ??
            Type.GetType("UnityEditorInternal.LogEntries, UnityEditor.dll");

        if (logEntriesType == null)
        {
            Debug.LogWarning("Could not find Unity's internal LogEntries type. Unity may have changed its internal API.");
            return;
        }

        var clearMethodInfo =
            logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public) ??
            logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (clearMethodInfo == null)
        {
            Debug.LogWarning("Could not find LogEntries.Clear() via reflection. Unity may have changed its internal API.");
            return;
        }

        clearMethodInfo.Invoke(null, null);
    }
}
#endif
