#if UNITY_EDITOR

using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEditor;

namespace Cavrnus.SpatialConnector.Editor.Setup
{
    [InitializeOnLoad]
    public static class CavrnusOnLoadSetup
    {
        static CavrnusOnLoadSetup()
        {
            LoadTmPro();
        }

        private static readonly string TMProLoaded = "K_CavrnusTmProLoaded" + GetProjectHash();
        private static void LoadTmPro()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(TMProLoaded))
                    return;
                
                if (TMP_Settings.instance == null)
                {
                    var import = EditorUtility.DisplayDialog(
                        "Cavrnus - TextMeshPro Essentials Required",
                        "Cavrnus UI relies on TextMeshPro. Would you like to import TMP Essentials now?",
                        "Import",
                        "Skip");
                    
                    if (import)
                        TMP_PackageResourceImporter.ImportResources(true,false,false);
                }

                EditorPrefs.SetBool(TMProLoaded, true);
            };
        }

        private static string GetProjectHash()
        {
            var projectPath = System.IO.Path.GetFullPath(".");
            using (var sha = SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(projectPath);
                var hashBytes = sha.ComputeHash(bytes);
                return System.BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }
    }
}

#endif