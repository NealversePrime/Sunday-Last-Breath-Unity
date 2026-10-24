using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System.Reflection;
using System.Linq;
using System;

[InitializeOnLoad]
public class ES3ScriptingDefineSymbols
{
    static ES3ScriptingDefineSymbols()
    {
        SetDefineSymbols();
    }

    static void SetDefineSymbols() 
    {
        if (Type.GetType("Unity.VisualScripting.IncludeInSettingsAttribute, Unity.VisualScripting.Core") != null)
            SetDefineSymbol("UNITY_VISUAL_SCRIPTING");

        if (Type.GetType("Ludiq.IncludeInSettingsAttribute, Ludiq.Core.Runtime") != null)
            SetDefineSymbol("BOLT_VISUAL_SCRIPTING");
    }

    static void SetDefineSymbol(string symbol)
    {
#if UNITY_2021_2_OR_NEWER
        foreach (var target in GetAllNamedBuildTargets())
        {
            try
            {
                string[] defines;
                PlayerSettings.GetScriptingDefineSymbols(target, out defines);
                if (!defines.Contains(symbol))
                {
                    ArrayUtility.Add(ref defines, symbol);
                    PlayerSettings.SetScriptingDefineSymbols(target, defines);
                }
            }
            catch (Exception)
            {
                // Ignore obsolete or unsupported build targets (e.g. Stadia in Unity 6)
            }
        }
#else
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var allDefines = new HashSet<string>(definesString.Split(';'));
        if (!allDefines.Contains(symbol))
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.Concat(new string[] { symbol }).ToArray()));
#endif
        return;
    }

#if UNITY_2021_2_OR_NEWER
    static List<NamedBuildTarget> GetAllNamedBuildTargets()
    {
        var staticFields = typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static);
        var buildTargets = new List<NamedBuildTarget>();

        foreach (var staticField in staticFields)
        {
            // Exclude 'Unknown' and deprecated platforms like 'Stadia' which throw ArgumentException in Unity 6
            if (staticField.Name == "Unknown" || staticField.Name == "Stadia")
                continue;

            if (staticField.FieldType == typeof(NamedBuildTarget))
                buildTargets.Add((NamedBuildTarget)staticField.GetValue(null));
        }

        return buildTargets;
    }
#endif
}
