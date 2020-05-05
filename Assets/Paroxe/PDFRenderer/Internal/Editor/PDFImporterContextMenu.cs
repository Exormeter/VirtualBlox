using System.IO;
using UnityEditor;
using UnityEngine;

namespace Paroxe.PdfRenderer.Internal
{
    public class PDFImporterContextMenu
    {
        static string extension = ".pdf";
        static string newExtension = ".asset";

        [MenuItem("Assets/PDF Renderer/Convert to .asset")]
        public static void ConvertPDFToAsset()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string newPath = ConvertToInternalPath(path);

            PDFAsset numSeq = AssetDatabase.LoadAssetAtPath(newPath, typeof(PDFAsset)) as PDFAsset;
            bool loaded = (numSeq != null);

            if (!loaded)
            {
                numSeq = ScriptableObject.CreateInstance<PDFAsset>();
            }

            numSeq.Load(File.ReadAllBytes(path));

            if (!loaded)
            {
                AssetDatabase.CreateAsset(numSeq, newPath);
            }

            AssetDatabase.SaveAssets();
        }

        public static string ConvertToInternalPath(string asset)
        {
            string left = asset.Substring(0, asset.Length - extension.Length);
            return left + newExtension;
        }

        public static bool HasExtension(string asset)
        {
            return asset.EndsWith(extension, System.StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/PDF Renderer/Convert to .asset", true)]
        static bool ValidateConvertPDFToAsset()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return HasExtension(path);
        }
    }
}