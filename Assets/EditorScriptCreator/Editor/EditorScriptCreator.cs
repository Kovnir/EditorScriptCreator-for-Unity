using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class EditorScriptCreator : Editor
{
    private const string MANU_NAME = "Assets/Create/== Create Editor Script ==";


    [MenuItem(MANU_NAME)]
    private static void DoSomething()
    {
        string assetName = Selection.activeObject.name;
        string assetFullPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string assetDirectory = Path.GetDirectoryName(assetFullPath);

        string assetNamespace = GetNamespace(assetFullPath);
        
        string editorName = assetName + "Editor.cs";
        string editorDirectory = Path.Combine(assetDirectory, "Editor");
        string editorFullPath = Path.Combine(editorDirectory, editorName);

        
        if (!Directory.Exists(editorDirectory))
        {
            Directory.CreateDirectory(editorDirectory);
        }

        if (!File.Exists(editorFullPath))
        {
            File.Create(editorFullPath).Dispose();
            List<string> lines = GenerateClassContent(assetName, editorName, assetNamespace);
            File.WriteAllLines(editorFullPath, lines);
            AssetDatabase.Refresh();
            InternalEditorUtility.OpenFileAtLineExternal(editorFullPath, 0);
        }
        else
        {
            EditorUtility.DisplayDialog("Editor Script Creator", "The file with path 'editorFullPath' is exists.", "Fuck");
        }
    }

    private static string GetNamespace(string assetFullPath)
    {
        string content = File.ReadAllText(assetFullPath);
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("Source file is empty.");
            return null;
        }

        string[] strings = content.Split(' ', '\n');

        int namespaceIndex = Array.IndexOf(strings, "namespace");
        if (namespaceIndex != -1)
        {
            return strings[namespaceIndex+1];
        }
            
        return null;
    }

    private static List<string> GenerateClassContent(string assetName, string editorName, string assetNamespace)
    {
        var lines = new List<string>
        {
            "using UnityEngine;",
            "using UnityEditor;",
            "",
            $"[CustomEditor(typeof({assetName}))]",
            "[CanEditMultipleObjects]",
            "public class NewBehaviourScriptEditor : Editor",
            "{",
            $"\tprivate {assetName} {GenerateVariableName(assetName)} => target as {assetName};",
            "",
            "\tpublic override void OnInspectorGUI()",
            "\t{",
            $"\t\tEditorGUILayout.LabelField(\"Label From Generated Editor ({editorName})\");",
            "\t\tbase.OnInspectorGUI();",
            "\t}",
            "}"
        };
        if (!string.IsNullOrEmpty(assetNamespace))
        {
            lines.Insert(0, $"using {assetNamespace};");
        }
        return lines;
    }

    private static string GenerateVariableName(string classname)
    {
        return classname.First().ToString().ToUpper() + classname.Substring(1);
    }
    
    [MenuItem(MANU_NAME, true)]
    private static bool DoSomethingValidation() {
        return Selection.activeObject.GetType() == typeof(MonoScript);
    }

}
