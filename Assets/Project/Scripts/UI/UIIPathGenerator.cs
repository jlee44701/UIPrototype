#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using static UIIds.TagGeneratorHelper;

namespace UIIds
{
    internal static class UIIdsPaths
    {
        public const string UiRootAssetPath = "Assets/Project/UI";
        public const string GeneratedFolderName = "UI_Strings";
        public const string OutputFileName = "UIStrings.cs";

        public static string UiRootFullPath
        {
            get
            {
                var physicalPath = FileUtil.GetPhysicalPath(UiRootAssetPath);
                return Path.GetFullPath(physicalPath);
            }
        }

        public static string OutputFullPath =>
            Path.GetFullPath(Path.Combine(UiRootFullPath, GeneratedFolderName, OutputFileName));

        public static string OutputAssetPath =>
            $"{UiRootAssetPath}/{GeneratedFolderName}/{OutputFileName}".Replace("\\", "/");

        public static string UiRootAssetPathWithSlash =>
            UiRootAssetPath.TrimEnd('/') + "/";
    }

static class UiIdMenuItems
    {
        [MenuItem("PE/UI/Refresh UI Strings", priority = 2000)]
        static void RefreshUiStrings()
        {
            UiIdGenerator.GenerateAndImport();
        }

        [MenuItem("PE/UI/Tools/Refresh UI Strings", validate = true)]
        static bool RefreshUiStringsValidate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return false;

            var uiRootFullPath = UIIdsPaths.UiRootFullPath;
            return Directory.Exists(uiRootFullPath);
        }
    }

    static partial class UiIdGenerator
    {
        public static void GenerateAndImport()
        {
            try
            {
                // This won’t save UI Builder edits, but it ensures Unity’s view of disk is up to date.
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); 

                var uiRootFullPath = UIIdsPaths.UiRootFullPath;
                if (!Directory.Exists(uiRootFullPath))
                {
                    EditorUtility.DisplayDialog(
                        "Refresh UI Strings",
                        $"UI root folder does not exist:\n{uiRootFullPath}",
                        "OK"
                    );
                    return;
                }

                
                Debug.Log($"[UIIds] Scanning UI root:\n{uiRootFullPath}");

                var ussFileFullPaths = Directory.GetFiles(uiRootFullPath, "*.uss", SearchOption.AllDirectories);
                var uxmlFileFullPaths = Directory.GetFiles(uiRootFullPath, "*.uxml", SearchOption.AllDirectories);

                var generatedText = BuildGeneratedFileText(
                    ussFileFullPaths.Select(Path.GetFullPath).ToList(),
                    uxmlFileFullPaths.Select(Path.GetFullPath).ToList()
                );

                WriteAllTextAlways(UIIdsPaths.OutputFullPath, generatedText);

                AssetDatabase.ImportAsset(
                    UIIdsPaths.OutputAssetPath,
                    ImportAssetOptions.ForceUpdate
                    | ImportAssetOptions.ForceSynchronousImport
                );
                
                Debug.Log("[UIIds] UI Strings refreshed");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[UIIds] Generation failed:\n{exception}");
                EditorUtility.DisplayDialog(
                    "Refresh UI Strings FAILED",
                    "Generation failed. Check the Console for the exception.\n\n" +
                    "Common causes:\n" +
                    "- UXML is not saved (UI Builder)\n" +
                    "- UXML is invalid XML (e.g. an extra '>')",
                    "OK"
                );
            }
        }

        static void WriteAllTextAlways(string fullFilePath, string text)
        {
            var directoryPath = Path.GetDirectoryName(fullFilePath);
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllText(fullFilePath, text);
        }
    

        //
        // ***************************************
        // FILE TEXT
        // ***************************************
        static string BuildGeneratedFileText(List<string> ussFileFullPaths, List<string> uxmlFileFullPaths)
        {
            var stringBuilder = new StringBuilder(48 * 1024);

            stringBuilder.AppendLine("// GENERATED FILE - DO NOT EDIT");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace PixelEngine {");
            stringBuilder.AppendLine("    public static partial class UIStrings {");

            var wrapperToUssFiles = GroupByWrapperFolder(ussFileFullPaths, UssFolderName);
            var wrapperToUxmlFiles = GroupByWrapperFolder(uxmlFileFullPaths, UxmlFolderName);

            var allWrapperClassNames =
                wrapperToUssFiles.Keys
                    .Union(wrapperToUxmlFiles.Keys, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => string.IsNullOrEmpty(value) ? "" : value, StringComparer.Ordinal)
                    .ToList();

            var baseIndent = "        ";

            foreach (var wrapperClassName in allWrapperClassNames)
            {
                wrapperToUssFiles.TryGetValue(wrapperClassName, out var scopedUssFiles);
                wrapperToUxmlFiles.TryGetValue(wrapperClassName, out var scopedUxmlFiles);

                scopedUssFiles ??= new List<string>();
                scopedUxmlFiles ??= new List<string>();

                if (scopedUssFiles.Count == 0 && scopedUxmlFiles.Count == 0)
                    continue;

                if (!string.IsNullOrEmpty(wrapperClassName))
                {
                    stringBuilder.AppendLine($"{baseIndent}public static partial class {wrapperClassName} {{");

                    var nestedIndent = baseIndent + "    ";

                    AppendUss(stringBuilder, scopedUssFiles, nestedIndent);

                    if (scopedUxmlFiles.Count > 0)
                        stringBuilder.AppendLine();

                    AppendUxml(stringBuilder, scopedUxmlFiles, nestedIndent);

                    stringBuilder.AppendLine($"{baseIndent}}}");
                }
                else
                {
                    AppendUss(stringBuilder, scopedUssFiles, baseIndent);

                    if (scopedUxmlFiles.Count > 0)
                        stringBuilder.AppendLine();

                    AppendUxml(stringBuilder, scopedUxmlFiles, baseIndent);
                }

                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        //
        // ***************************************
        // NAME SANITIZATION
        // ***************************************
        static string MakeSafeClassStem(string rawStem)
        {
            if (string.IsNullOrWhiteSpace(rawStem))
                return "File";

            var safe = Regex.Replace(rawStem, @"[^A-Za-z0-9]+", "_");
            safe = Regex.Replace(safe, @"_+", "_").Trim('_');

            if (safe.Length == 0)
                safe = "File";

            if (char.IsDigit(safe[0]))
                safe = "_" + safe;

            return safe;
        }

        static string MakePerDirectoryClassName(string directoryFullPath)
        {
            if (string.IsNullOrWhiteSpace(directoryFullPath))
                return "Folder";

            var trimmed = directoryFullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var directoryName = Path.GetFileName(trimmed);

            if (string.IsNullOrWhiteSpace(directoryName))
                directoryName = "Folder";

            var safeStem = MakeSafeClassStem(directoryName);
            return ToClassName(safeStem);
        }

        static string NormalizeFullPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            return Path.GetFullPath(fullPath).Replace("\\", "/").TrimEnd('/');
        }

        static string ComputeRelativeToUiRoot(string directoryFullPathNormalized)
        {
            var uiRootFullPathNormalized = NormalizeFullPath(UIIdsPaths.UiRootFullPath);

            if (string.IsNullOrEmpty(uiRootFullPathNormalized))
                return directoryFullPathNormalized;

            if (!directoryFullPathNormalized.StartsWith(uiRootFullPathNormalized, StringComparison.OrdinalIgnoreCase))
                return directoryFullPathNormalized;

            var relative = directoryFullPathNormalized.Substring(uiRootFullPathNormalized.Length).TrimStart('/');
            return relative.Length == 0 ? "." : relative;
        }

        static string ComputeStableHexHash(string text)
        {
            unchecked
            {
                var hashValue = 2166136261u;
                for (var characterIndex = 0; characterIndex < text.Length; characterIndex++)
                {
                    hashValue ^= text[characterIndex];
                    hashValue *= 16777619u;
                }

                return hashValue.ToString("X8");
            }
        }

        static IEnumerable<(string constName, string rawValue)> BuildUniqueConstPairs(IEnumerable<string> rawValues)
        {
            var identifierToRawValue = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var rawValue in rawValues.OrderBy(value => value, StringComparer.Ordinal))
            {
                var baseIdentifier = ToIdentifier(rawValue);

                if (!identifierToRawValue.TryGetValue(baseIdentifier, out var existingRawValue))
                {
                    identifierToRawValue[baseIdentifier] = rawValue;
                    continue;
                }

                if (string.Equals(existingRawValue, rawValue, StringComparison.Ordinal))
                    continue;

                var hashSuffix = ComputeStableHexHash(rawValue).Substring(0, 6);
                var uniqueIdentifier = $"{baseIdentifier}_{hashSuffix}";

                while (identifierToRawValue.ContainsKey(uniqueIdentifier))
                    uniqueIdentifier += "_";

                identifierToRawValue[uniqueIdentifier] = rawValue;
            }

            foreach (var pair in identifierToRawValue.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                yield return (pair.Key, pair.Value);
        }

        //
        // ***************************************
        // USS (UNIFIED)
        // ***************************************
        static void AppendUss(StringBuilder stringBuilder, List<string> ussFileFullPaths, string baseIndent)
        {
            var selectorTokenRegex = new Regex(@"([.#])(?<name>[\w\-]+)", RegexOptions.Compiled);

            var seenClasses = new HashSet<string>(StringComparer.Ordinal);
            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var fileFullPath in ussFileFullPaths.OrderBy(value => value, StringComparer.Ordinal))
            {
                var fileText = File.ReadAllText(fileFullPath);

                foreach (var selectorPreludeRaw in EnumerateUssSelectorPreludes(fileText))
                {
                    var selectorPrelude = StripUssSelectorNoise(selectorPreludeRaw);

                    foreach (Match match in selectorTokenRegex.Matches(selectorPrelude))
                    {
                        var prefix = match.Groups[1].Value;
                        var rawName = match.Groups["name"].Value;

                        if (prefix == ".")
                            seenClasses.Add(rawName);
                        else if (prefix == "#")
                            seenNames.Add(rawName);
                    }
                }
            }

            if (seenClasses.Count == 0 && seenNames.Count == 0)
                return;

            baseIndent ??= string.Empty;

            var indent0 = baseIndent;
            var indent1 = indent0 + "    ";
            var indent2 = indent1 + "    ";

            stringBuilder.AppendLine($"{indent0}public static partial class Uss {{");

            if (seenClasses.Count > 0)
            {
                stringBuilder.AppendLine($"{indent1}public static class Classes {{");
                foreach (var (constName, rawValue) in BuildUniqueConstPairs(seenClasses))
                    stringBuilder.AppendLine($"{indent2}public const string {constName} = \"{rawValue}\";");
                stringBuilder.AppendLine($"{indent1}}}");
            }

            if (seenNames.Count > 0)
            {
                stringBuilder.AppendLine($"{indent1}public static class Names {{");
                foreach (var (constName, rawValue) in BuildUniqueConstPairs(seenNames))
                    stringBuilder.AppendLine($"{indent2}public const string {constName} = \"{rawValue}\";");
                stringBuilder.AppendLine($"{indent1}}}");
            }

            stringBuilder.AppendLine($"{indent0}}}");
        }

        static IEnumerable<string> EnumerateUssSelectorPreludes(string ussText)
        {
            var textWithoutComments =
                Regex.Replace(ussText, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var selectorPreludeBuilder = new StringBuilder();
            var isInsideDeclarations = false;
            var braceDepth = 0;
            var declarationsBraceDepth = 0;

            for (var characterIndex = 0; characterIndex < textWithoutComments.Length; characterIndex++)
            {
                var character = textWithoutComments[characterIndex];

                if (character == '{')
                {
                    if (!isInsideDeclarations)
                    {
                        var selectorPrelude = selectorPreludeBuilder.ToString().Trim();
                        selectorPreludeBuilder.Clear();

                        if (selectorPrelude.Length > 0 && selectorPrelude[0] != '@')
                            yield return selectorPrelude;

                        braceDepth++;

                        if (selectorPrelude.Length > 0 && selectorPrelude[0] != '@')
                        {
                            isInsideDeclarations = true;
                            declarationsBraceDepth = braceDepth;
                        }

                        continue;
                    }

                    braceDepth++;
                    continue;
                }

                if (character == '}')
                {
                    braceDepth = Math.Max(0, braceDepth - 1);

                    if (isInsideDeclarations && braceDepth < declarationsBraceDepth)
                    {
                        isInsideDeclarations = false;
                        declarationsBraceDepth = 0;
                    }

                    continue;
                }

                if (!isInsideDeclarations)
                    selectorPreludeBuilder.Append(character);
            }
        }

        static string StripUssSelectorNoise(string selectorPrelude)
        {
            if (string.IsNullOrEmpty(selectorPrelude))
                return selectorPrelude;

            var withoutAttributes = Regex.Replace(selectorPrelude, @"\[[^\]]*\]", " ");
            var withoutDoubleQuotes = Regex.Replace(withoutAttributes, "\"[^\"]*\"", " ");
            var withoutSingleQuotes = Regex.Replace(withoutDoubleQuotes, "'[^']*'", " ");

            return withoutSingleQuotes;
        }

        //
        // ***************************************
        // UXML (PER DIRECTORY, TRACKS name AND class)
        // ***************************************
        static void AppendUxml(StringBuilder stringBuilder, List<string> uxmlFileFullPaths, string baseIndent)
        {
            baseIndent ??= string.Empty;

            var indent0 = baseIndent;
            var indent1 = indent0 + "    ";
            var indent2 = indent1 + "    ";
            var indent3 = indent2 + "    ";

            stringBuilder.AppendLine($"{indent0}public static partial class Uxml {{");

            var directoryToFileFullPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileFullPathRaw in uxmlFileFullPaths)
            {
                if (string.IsNullOrEmpty(fileFullPathRaw))
                    continue;

                var fileFullPath = NormalizeFullPath(fileFullPathRaw);
                var directoryFullPath = Path.GetDirectoryName(fileFullPath);

                if (string.IsNullOrEmpty(directoryFullPath))
                    directoryFullPath = string.Empty;

                directoryFullPath = NormalizeFullPath(directoryFullPath);

                if (!directoryToFileFullPaths.TryGetValue(directoryFullPath, out var fileList))
                {
                    fileList = new List<string>();
                    directoryToFileFullPaths[directoryFullPath] = fileList;
                }

                fileList.Add(fileFullPath);
            }

            var groupedDirectories =
                directoryToFileFullPaths
                    .Select(pair =>
                    {
                        var directoryFullPath = pair.Key;
                        var directoryRelativePath = ComputeRelativeToUiRoot(directoryFullPath);
                        return (directoryFullPath, directoryRelativePath, fileFullPaths: pair.Value);
                    })
                    .OrderBy(group => group.directoryRelativePath, StringComparer.Ordinal)
                    .ThenBy(group => group.directoryFullPath, StringComparer.Ordinal)
                    .ToList();

            var usedDirectoryClassNames = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var group in groupedDirectories)
            {
                var seenNames = new HashSet<string>(StringComparer.Ordinal);
                var seenClasses = new HashSet<string>(StringComparer.Ordinal);

                foreach (var fileFullPath in group.fileFullPaths.OrderBy(value => value, StringComparer.Ordinal))
                {
                    var xmlDocument = XDocument.Load(fileFullPath);

                    foreach (var element in xmlDocument.Descendants())
                    {
                        var nameAttribute = element.Attribute("name");
                        if (nameAttribute != null)
                        {
                            var rawName = nameAttribute.Value.Trim();
                            if (rawName.Length > 0)
                                seenNames.Add(rawName);
                        }

                        var classAttribute = element.Attribute("class");
                        if (classAttribute != null)
                        {
                            var rawClassList = classAttribute.Value;
                            if (!string.IsNullOrWhiteSpace(rawClassList))
                            {
                                var classTokens = Regex.Split(rawClassList.Trim(), @"\s+");
                                foreach (var classToken in classTokens)
                                {
                                    if (!string.IsNullOrWhiteSpace(classToken))
                                        seenClasses.Add(classToken.Trim());
                                }
                            }
                        }
                    }
                }

                if (seenNames.Count == 0 && seenClasses.Count == 0)
                    continue;

                var baseDirectoryClassName = MakePerDirectoryClassName(group.directoryFullPath);
                var directoryClassName = baseDirectoryClassName;

                if (usedDirectoryClassNames.TryGetValue(directoryClassName, out var existingRelativePath) &&
                    !string.Equals(existingRelativePath, group.directoryRelativePath, StringComparison.Ordinal))
                {
                    var hashSuffix = ComputeStableHexHash(group.directoryRelativePath).Substring(0, 6);
                    directoryClassName = $"{baseDirectoryClassName}_{hashSuffix}";

                    while (usedDirectoryClassNames.ContainsKey(directoryClassName))
                        directoryClassName += "_";
                }

                usedDirectoryClassNames[directoryClassName] = group.directoryRelativePath;

                stringBuilder.AppendLine($"{indent1}public static class {directoryClassName} {{");

                if (seenNames.Count > 0)
                {
                    stringBuilder.AppendLine($"{indent2}public static class Names {{");
                    foreach (var (constName, rawValue) in BuildUniqueConstPairs(seenNames))
                        stringBuilder.AppendLine($"{indent3}public const string {constName} = \"{rawValue}\";");
                    stringBuilder.AppendLine($"{indent2}}}");
                }

                if (seenClasses.Count > 0)
                {
                    stringBuilder.AppendLine($"{indent2}public static class Classes {{");
                    foreach (var (constName, rawValue) in BuildUniqueConstPairs(seenClasses))
                        stringBuilder.AppendLine($"{indent3}public const string {constName} = \"{rawValue}\";");
                    stringBuilder.AppendLine($"{indent2}}}");
                }

                stringBuilder.AppendLine($"{indent1}}}");
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine($"{indent0}}}");
        }

        //
        // ***************************************
        // WRAPPER FOLDERS
        // ***************************************
        const string UssFolderName = "Uss";
        const string UxmlFolderName = "Uxml";

        static string GetWrapperFolderNameFromFilePath(string fileFullPathNormalized, string markerFolderName)
        {
            if (string.IsNullOrEmpty(fileFullPathNormalized))
                return string.Empty;

            var directoryFullPath = Path.GetDirectoryName(fileFullPathNormalized);
            if (string.IsNullOrEmpty(directoryFullPath))
                return string.Empty;

            directoryFullPath = NormalizeFullPath(directoryFullPath);
            var relativeDirectoryPath = ComputeRelativeToUiRoot(directoryFullPath);

            if (string.IsNullOrEmpty(relativeDirectoryPath) || relativeDirectoryPath == ".")
                return string.Empty;

            var pathSegments = relativeDirectoryPath
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (var segmentIndex = 0; segmentIndex < pathSegments.Length; segmentIndex++)
            {
                if (!pathSegments[segmentIndex].Equals(markerFolderName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (segmentIndex == 0)
                    return string.Empty;

                return pathSegments[segmentIndex - 1];
            }

            return string.Empty;
        }

        static string MakeWrapperClassName(string rawFolderName)
        {
            if (string.IsNullOrWhiteSpace(rawFolderName))
                return string.Empty;

            var safeStem = MakeSafeClassStem(rawFolderName);
            return ToClassName(safeStem);
        }

        static Dictionary<string, List<string>> GroupByWrapperFolder(List<string> fileFullPaths, string markerFolderName)
        {
            var wrapperToFiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileFullPathRaw in fileFullPaths)
            {
                if (string.IsNullOrEmpty(fileFullPathRaw))
                    continue;

                var fileFullPath = NormalizeFullPath(fileFullPathRaw);

                var wrapperFolderNameRaw = GetWrapperFolderNameFromFilePath(fileFullPath, markerFolderName);
                var wrapperClassName = MakeWrapperClassName(wrapperFolderNameRaw);

                if (!wrapperToFiles.TryGetValue(wrapperClassName, out var list))
                {
                    list = new List<string>();
                    wrapperToFiles[wrapperClassName] = list;
                }

                list.Add(fileFullPath);
            }

            return wrapperToFiles;
        }
    }


    public class TagGeneratorHelper
    {
        public static string ToIdentifier(string raw) {
            var parts = raw.Split(new[] {
                    '-', '_'
                }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => char.ToUpperInvariant(t[0]) + t.Substring(1));
            var name = string.Concat(parts);
            if (string.IsNullOrEmpty(name)) name = "_";
            if (char.IsDigit(name[0])) name = "_" + name;
            return name;
        }

        public static string ToClassName(string raw) {
            var n = ToIdentifier(raw);
            if (!char.IsLetter(n[0])) n = "S" + n;
            return n;
        }
    
    }

}
#endif
