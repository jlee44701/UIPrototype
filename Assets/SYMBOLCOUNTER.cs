using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CSharpSymbolFrequencyWindow : EditorWindow
{
    string m_RootDirectoryPath;
    int m_TopResultsToShow = 40;

    static readonly string[] s_MultiCharacterTokens =
    {
        ">>>=", ">>>",
        "<<=", ">>=",
        "??=", "??",
        "?.",
        "==", "!=", "<=", ">=",
        "&&", "||",
        "++", "--",
        "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=",
        "=>",
        "::",
        "..",
        "->"
    };

    [MenuItem("Tools/C# Symbol Frequency")]
    static void OpenWindow()
    {
        var windowInstance = GetWindow<CSharpSymbolFrequencyWindow>("C# Symbol Frequency");
        windowInstance.minSize = new Vector2(520, 170);
        windowInstance.m_RootDirectoryPath = Application.dataPath;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Root folder to scan (.cs files):");

        using (new EditorGUILayout.HorizontalScope())
        {
            m_RootDirectoryPath = EditorGUILayout.TextField(m_RootDirectoryPath);

            if (GUILayout.Button("Use Assets", GUILayout.Width(90)))
                m_RootDirectoryPath = Application.dataPath;
        }

        m_TopResultsToShow = EditorGUILayout.IntSlider("Top results", m_TopResultsToShow, 10, 200);

        var isValidRootDirectory = !string.IsNullOrWhiteSpace(m_RootDirectoryPath) && Directory.Exists(m_RootDirectoryPath);

        using (new EditorGUI.DisabledScope(!isValidRootDirectory))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan (log to Console)"))
                    ScanAndLogResults();

                if (GUILayout.Button("Scan and Save…"))
                    ScanAndSaveResults();
            }
        }

        EditorGUILayout.HelpBox("Counts operators/punctuators while skipping comments and strings. Good for designing a symbols layer.", MessageType.Info);
    }

    void ScanAndLogResults()
    {
        var reportText = BuildReportText(out var reportTitleLine);
        Debug.Log(reportTitleLine);
        Debug.Log(reportText);
    }

    void ScanAndSaveResults()
    {
        var reportText = BuildReportText(out var reportTitleLine);

        var defaultFileName = $"csharp_symbol_frequency_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var defaultDirectoryPath = Directory.Exists(m_RootDirectoryPath) ? m_RootDirectoryPath : Application.dataPath;

        var saveFilePath = EditorUtility.SaveFilePanel(
            "Save C# Symbol Frequency Report",
            defaultDirectoryPath,
            defaultFileName,
            "txt");

        if (string.IsNullOrWhiteSpace(saveFilePath))
            return;

        File.WriteAllText(saveFilePath, reportText, Encoding.UTF8);

        Debug.Log($"{reportTitleLine}\nSaved to: {saveFilePath}");
        EditorUtility.RevealInFinder(saveFilePath);
    }

    string BuildReportText(out string reportTitleLine)
    {
        var tokenTextToCount = new Dictionary<string, long>(StringComparer.Ordinal);

        var filePaths = Directory.GetFiles(m_RootDirectoryPath, "*.cs", SearchOption.AllDirectories);
        foreach (var filePath in filePaths)
        {
            var fileText = File.ReadAllText(filePath);
            CountTokensInFileText(fileText, tokenTextToCount);
        }

        var orderedResults = tokenTextToCount
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Take(m_TopResultsToShow)
            .ToArray();

        reportTitleLine = $"C# symbol frequency results (top {m_TopResultsToShow}) from: {m_RootDirectoryPath}";

        var stringBuilder = new StringBuilder(8_192);
        stringBuilder.AppendLine(reportTitleLine);
        stringBuilder.AppendLine($"Scanned files: {filePaths.Length}");
        stringBuilder.AppendLine();

        foreach (var pair in orderedResults)
            stringBuilder.AppendLine($"{pair.Value,10}  {pair.Key}");

        return stringBuilder.ToString();
    }

    static void CountTokensInFileText(string fileText, Dictionary<string, long> tokenTextToCount)
    {
        var isInSingleLineComment = false;
        var isInMultiLineComment = false;
        var isInStringLiteral = false;
        var isInVerbatimStringLiteral = false;
        var isInCharacterLiteral = false;

        for (var index = 0; index < fileText.Length; index++)
        {
            var currentCharacter = fileText[index];
            var nextCharacter = index + 1 < fileText.Length ? fileText[index + 1] : '\0';

            if (isInSingleLineComment)
            {
                if (currentCharacter == '\n')
                    isInSingleLineComment = false;
                continue;
            }

            if (isInMultiLineComment)
            {
                if (currentCharacter == '*' && nextCharacter == '/')
                {
                    isInMultiLineComment = false;
                    index++;
                }
                continue;
            }

            if (isInStringLiteral)
            {
                if (currentCharacter == '\\')
                {
                    index++;
                    continue;
                }

                if (currentCharacter == '"')
                    isInStringLiteral = false;

                continue;
            }

            if (isInVerbatimStringLiteral)
            {
                if (currentCharacter == '"' && nextCharacter == '"')
                {
                    index++;
                    continue;
                }

                if (currentCharacter == '"')
                    isInVerbatimStringLiteral = false;

                continue;
            }

            if (isInCharacterLiteral)
            {
                if (currentCharacter == '\\')
                {
                    index++;
                    continue;
                }

                if (currentCharacter == '\'')
                    isInCharacterLiteral = false;

                continue;
            }

            if (currentCharacter == '/' && nextCharacter == '/')
            {
                isInSingleLineComment = true;
                index++;
                continue;
            }

            if (currentCharacter == '/' && nextCharacter == '*')
            {
                isInMultiLineComment = true;
                index++;
                continue;
            }

            if (currentCharacter == '@' && nextCharacter == '"')
            {
                isInVerbatimStringLiteral = true;
                index++;
                continue;
            }

            if (currentCharacter == '"')
            {
                isInStringLiteral = true;
                continue;
            }

            if (currentCharacter == '\'')
            {
                isInCharacterLiteral = true;
                continue;
            }

            var matchedMultiToken = false;
            foreach (var tokenText in s_MultiCharacterTokens)
            {
                if (index + tokenText.Length > fileText.Length)
                    continue;

                if (!IsExactMatchAtIndex(fileText, index, tokenText))
                    continue;

                IncrementCount(tokenTextToCount, tokenText);
                index += tokenText.Length - 1;
                matchedMultiToken = true;
                break;
            }

            if (matchedMultiToken)
                continue;

            if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_' || char.IsWhiteSpace(currentCharacter))
                continue;

            IncrementCount(tokenTextToCount, currentCharacter.ToString());
        }
    }

    static bool IsExactMatchAtIndex(string fileText, int startIndex, string tokenText)
    {
        for (var offset = 0; offset < tokenText.Length; offset++)
        {
            if (fileText[startIndex + offset] != tokenText[offset])
                return false;
        }

        return true;
    }

    static void IncrementCount(Dictionary<string, long> tokenTextToCount, string tokenText)
    {
        tokenTextToCount.TryGetValue(tokenText, out var existingCount);
        tokenTextToCount[tokenText] = existingCount + 1;
    }
}
