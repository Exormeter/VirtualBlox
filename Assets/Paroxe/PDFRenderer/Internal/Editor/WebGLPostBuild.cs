using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;

namespace Paroxe.PdfRenderer.Internal
{
    public class WebGLPostBuild : ScriptableObject
    {
        private const string PdfJsPath = @"Plugins\WebGL\pdf.bytes";
        private const string PdfWorkerJsPath = @"Plugins\WebGL\pdf.worker.bytes";

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.WebGL)
            {
                ScriptableObject scriptableObject = CreateInstance<WebGLPostBuild>();
                MonoScript script = MonoScript.FromScriptableObject(scriptableObject);

                string scriptPath = AssetDatabase.GetAssetPath(script);
                DestroyImmediate(scriptableObject);

                DirectoryInfo directoryInfo = new DirectoryInfo(scriptPath);

                string pdfJsFullPath = Path.Combine(directoryInfo.Parent.Parent.Parent.FullName, PdfJsPath);
                string pdfWorkerJsFullPath = Path.Combine(directoryInfo.Parent.Parent.Parent.FullName, PdfWorkerJsPath);

                File.Copy(pdfJsFullPath, Path.Combine(pathToBuiltProject, "pdf.js"), true);
                File.Copy(pdfWorkerJsFullPath, Path.Combine(pathToBuiltProject, "pdf.worker.js"), true);
            }
        }
    }
}