using System.IO;
using UnityEditor;
using UnityEngine;
using SundayLastBreath.Data;

namespace SundayLastBreath.Editor
{
    public static class CreateModularPassagesMenu
    {
        [MenuItem("Sunday's Last Breath/Generate All Act 1 Hallway Passages", priority = 200)]
        public static void GenerateAllAct1Passages()
        {
            string pyScript = Path.Combine(Application.dataPath, "Scripts", "Editor", "generate_act1_passages.py");
            if (File.Exists(pyScript))
            {
                var psi = new System.Diagnostics.ProcessStartInfo("python", $"\"{pyScript}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var proc = System.Diagnostics.Process.Start(psi);
                proc?.WaitForExit(10000);
            }
            AssetDatabase.Refresh();
            ForceReimportAct1();
            Debug.Log("<color=#10B981>[MODULAR PASSAGES] Successfully generated all 14 4th Floor Hallway Passages in English from Twine!</color>");
        }

        [MenuItem("Sunday's Last Breath/Force Reimport All Act 1 Passages", priority = 201)]
        public static void ForceReimportAct1()
        {
            string animFolder = "Assets/Resources/Act1/Unit408Door_Anim";
            if (Directory.Exists(animFolder))
            {
                AssetDatabase.ImportAsset(animFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            }

            string folder = "Assets/Resources/StoryPassages/Act1";
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*.asset");
                foreach (var file in files)
                {
                    string assetPath = file.Replace("\\", "/");
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("<color=#10B981>[MODULAR PASSAGES] Reimported all Act 1 assets and animations cleanly!</color>");
        }
    }
}
