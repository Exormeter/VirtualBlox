using System.IO;
using UnityEditor;
using UnityEngine;

namespace Paroxe.PdfRenderer.Internal
{
    [CustomEditor(typeof(PDFAsset), true)]
    public class PDFAssetEditor : Editor
    {
        GUIStyle m_Background1;
        GUIStyle m_Background2;
        GUIStyle m_Background3;
        Texture2D m_Logo;
        PDFAsset pdfAsset = null;

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(pdfAsset, "PDFAsset");

            if (m_Logo != null)
            {
                Rect rect = GUILayoutUtility.GetRect(m_Logo.width, m_Logo.height);
                GUI.DrawTexture(rect, m_Logo, ScaleMode.ScaleToFit);
            }

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Password Options", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            pdfAsset.Password = EditorGUILayout.PasswordField("Password", pdfAsset.Password);
            EditorGUI.indentLevel--;

            GUILayout.Space(10.0f);
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(m_Background1.normal.background);
            DestroyImmediate(m_Background2.normal.background);
            DestroyImmediate(m_Background3.normal.background);
        }

        protected virtual void OnEnable()
        {
            pdfAsset = (PDFAsset)target;

            m_Background1 = new GUIStyle();
            m_Background1.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
            m_Background2 = new GUIStyle();
            m_Background2.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.0f));
            m_Background3 = new GUIStyle();
            m_Background3.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.05f));

            MonoScript script = MonoScript.FromScriptableObject(this);
            string path = AssetDatabase.GetAssetPath(script);
            string logoPath = Path.GetDirectoryName(path) + "/Icons/logo_pa.png";
            m_Logo = (Texture2D)AssetDatabase.LoadAssetAtPath(logoPath, typeof(Texture2D));
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.hideFlags = HideFlags.HideAndDontSave;
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}