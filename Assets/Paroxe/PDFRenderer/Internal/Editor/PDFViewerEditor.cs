using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    [CustomEditor(typeof(PDFViewer), true)]
    public class PDFViewerEditor : Editor
    {
        private Texture2D m_Logo;

        private SerializedProperty m_FileSource = null;
        private SerializedProperty m_Folder = null;
        private SerializedProperty m_FileName = null;
        private SerializedProperty m_FileURL = null;
        private SerializedProperty m_FilePath = null;
        private SerializedProperty m_BytesSupplierObject = null;
        private SerializedProperty m_BytesSupplierComponent = null;
        private SerializedProperty m_BytesSupplierFunctionName = null;
        private SerializedProperty m_PDFAsset = null;
        private SerializedProperty m_LoadOnEnable = null;
        private SerializedProperty m_UnloadOnDisable = null;
        private SerializedProperty m_Password = null;
        private SerializedProperty m_ZoomFactor = null;
        private SerializedProperty m_ZoomStep = null;
        private SerializedProperty m_PageFitting = null;
        private SerializedProperty m_VerticalMarginBetweenPages = null;
        private SerializedProperty m_MinZoomFactor = null;
        private SerializedProperty m_MaxZoomFactor = null;
        private SerializedProperty m_ScrollSensitivity = null;
        private SerializedProperty m_ShowTopBar = null;
        private SerializedProperty m_ShowVerticalScrollBar = null;
        private SerializedProperty m_ShowBookmarksViewer = null;
        private SerializedProperty m_ShowHorizontalScrollBar = null;
        private SerializedProperty m_ShowThumbnailsViewer = null;
        private SerializedProperty m_ChangeCursorWhenOverURL = null;
        private SerializedProperty m_AllowOpenURL = null;
        private SerializedProperty m_SearchResultColor = null;
        private SerializedProperty m_SearchResultPadding = null;
        private SerializedProperty m_SearchTimeBudgetPerFrame = null;
        private SerializedProperty m_ParagraphZoomFactor = null;
        private SerializedProperty m_ParagraphZoomingEnable = null;
        private SerializedProperty m_ParagraphDetectionThreshold = null;
        private SerializedProperty m_PageTileTexture = null;
        private SerializedProperty m_PageColor = null;
        private SerializedProperty m_MaxZoomFactorTextureQuality = null;
        private SerializedProperty m_RenderSettings = null;
        private SerializedProperty m_RenderAnnotations = null;
        private SerializedProperty m_RenderGrayscale = null;

        protected virtual void OnEnable()
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(script);

            m_Logo = (Texture2D) AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(scriptPath) + "/Icons/logo_pv.png", typeof(Texture2D));

            GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.MemberType == MemberTypes.Field && ((FieldInfo)x).FieldType == typeof(SerializedProperty))
                .Cast<FieldInfo>()
                .ToList().ForEach(prop =>
                {
                    SerializedProperty serializedProperty = serializedObject.FindProperty(prop.Name);

                    if (serializedProperty != null)
                        prop.SetValue(this, serializedProperty);
                });

            m_RenderAnnotations = m_RenderSettings.FindPropertyRelative("renderAnnotations");
            m_RenderGrayscale = m_RenderSettings.FindPropertyRelative("grayscale");
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "PDFViewer");

            serializedObject.Update();

            if (m_Logo != null)
            {
                Rect rect = GUILayoutUtility.GetRect(m_Logo.width, m_Logo.height);
                GUI.DrawTexture(rect, m_Logo, ScaleMode.ScaleToFit);
            }

            InspectLoadSettings();
            InspectSecuritySettings();
            InspectViewerSettings();
            InspectSearchSettings();
            InspectOtherSettings();
            InspectRenderingSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void InspectLoadSettings()
        {
            PDFViewer viewer = (PDFViewer)target;

            GUILayout.BeginVertical("Box");

            if (EnterGroup("Load Settings", "Paroxe.PdfRenderer.PDFViewer.ShowLoadSettings"))
            {
                EditorGUILayout.PropertyField(m_FileSource);

                PDFViewer.FileSourceType fileSourceType = (PDFViewer.FileSourceType)m_FileSource.enumValueIndex;

                if (fileSourceType == PDFViewer.FileSourceType.Resources
                    || fileSourceType == PDFViewer.FileSourceType.StreamingAssets
                    || fileSourceType == PDFViewer.FileSourceType.PersistentData)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(m_Folder);
                    EditorGUILayout.PropertyField(m_FileName);

                    if (fileSourceType == PDFViewer.FileSourceType.Resources)
                    {
                        if (File.Exists(Application.dataPath + "/Resources/" + viewer.GetFileLocation())
                            && !File.Exists(Application.dataPath + "/Resources/" + viewer.GetFileLocation().Replace(".bytes", "") + ".bytes"))
                        {
                            EditorGUILayout.HelpBox(
                                "PDF file in resources folder need to have .bytes extension to allow PDFViewer to access it correctly. \n\r    For example => pdf_sample.pdf.bytes",
                                MessageType.Warning);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                else if (fileSourceType == PDFViewer.FileSourceType.Web)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_FileURL, new GUIContent("Url"));
                    EditorGUI.indentLevel--;
                }
                else if (fileSourceType == PDFViewer.FileSourceType.FilePath)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_FilePath);
                    EditorGUI.indentLevel--;
                }

                if (fileSourceType != PDFViewer.FileSourceType.Bytes && fileSourceType != PDFViewer.FileSourceType.None)
                {
#if UNITY_IOS
                    if (fileSourceType == PDFViewer.FileSourceType.Web)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(" ");
                        EditorGUILayout.HelpBox(
                            "You may need to add NSAppTransportSecurity entry in info.plist of the XCode project to allow PDFViewer to download pdf from web:\n\r\n\r" +
                            "<key>NSAppTransportSecurity</key>\n\r" +
                            "    <dict>\n\r" +
                            "    <key>NSAllowsArbitraryLoads</key>\n\r" +
                            "        <true/>\n\r" +
                            "</dict>", MessageType.Info);
                        EditorGUILayout.EndHorizontal();
                    }
#endif
                    if (fileSourceType == PDFViewer.FileSourceType.StreamingAssets
                        || fileSourceType == PDFViewer.FileSourceType.PersistentData
                        || fileSourceType == PDFViewer.FileSourceType.Resources
                        || fileSourceType == PDFViewer.FileSourceType.FilePath)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(" ");

                        if (GUILayout.Button("Browse"))
                        {
                            string baseDirectory = "";

                            if (fileSourceType == PDFViewer.FileSourceType.PersistentData)
                                baseDirectory = Application.persistentDataPath;
                            else if (fileSourceType == PDFViewer.FileSourceType.StreamingAssets)
                                baseDirectory = Application.streamingAssetsPath;
                            else if (fileSourceType == PDFViewer.FileSourceType.Resources)
                                baseDirectory = "Assets/Resources";
                            else if (fileSourceType == PDFViewer.FileSourceType.FilePath)
                            {
                                string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
                                projectRoot = projectRoot.Replace('\\', '/');

                                baseDirectory = projectRoot;
                            }

                            if (!Directory.Exists(baseDirectory))
                            {
                                Directory.CreateDirectory(baseDirectory);
                                AssetDatabase.Refresh();
                            }

                            string folder = "";
                            string fileName = "";
                            string filePath = "";
                            bool usePersistentData = false;
                            bool useStreamingAssets = false;
                            bool useResources = false;
                            bool useFilePath = false;

                            if (Browse(baseDirectory, ref fileName, ref folder, ref filePath, ref useStreamingAssets, ref usePersistentData, ref useResources, ref useFilePath))
                            {
                                if (useStreamingAssets)
                                    fileSourceType = PDFViewer.FileSourceType.StreamingAssets;
                                else if (usePersistentData)
                                    fileSourceType = PDFViewer.FileSourceType.PersistentData;
                                else if (useResources)
                                    fileSourceType = PDFViewer.FileSourceType.Resources;
                                else if (useFilePath)
                                    fileSourceType = PDFViewer.FileSourceType.FilePath;

                                if (fileSourceType != PDFViewer.FileSourceType.FilePath)
                                {
                                    m_Folder.stringValue = folder;
                                    m_FileName.stringValue = fileName;
                                }
                                else
                                {
                                    m_FilePath.stringValue = filePath;
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

#if UNITY_EDITOR_WIN
                    if (fileSourceType != PDFViewer.FileSourceType.Asset && fileSourceType != PDFViewer.FileSourceType.DocumentObject)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(" ");

                        using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(m_FileName.stringValue.Trim())))
                        {
                            if (GUILayout.Button("Reveal File"))
                            {
                                string filePath = "";

                                if (fileSourceType == PDFViewer.FileSourceType.Resources)
                                {
                                    filePath = Application.dataPath + "/Resources/" + viewer.GetFileLocation();
                                }
                                else
                                {
                                    filePath = viewer.GetFileLocation();
                                }

                                if (fileSourceType != PDFViewer.FileSourceType.Web)
                                {
                                    if (!File.Exists(filePath))
                                    {
                                        if (fileSourceType == PDFViewer.FileSourceType.Resources &&
                                            File.Exists(filePath + ".bytes"))
                                        {
                                            ShowInExplorer(filePath + ".bytes");
                                        }
                                        else
                                        {
                                            EditorUtility.DisplayDialog("Error",
                                                "The file path is badly formed, contains invalid characters or doesn't exists:\r\n\r\n" +
                                                filePath, "Ok");
                                        }
                                    }
                                    else
                                    {
                                        ShowInExplorer(filePath);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        System.Diagnostics.Process.Start(filePath);
                                    }
                                    catch
                                    {
                                        EditorUtility.DisplayDialog("Error",
                                            "The URL is badly formed or contains invalid characters:\r\n\r\n" + filePath, "Ok");
                                    }
                                }
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
#endif
                }

                if (fileSourceType == PDFViewer.FileSourceType.Bytes)
                {
                    EditorGUI.indentLevel++;
                    GameObject oldObject = (GameObject)m_BytesSupplierObject.objectReferenceValue;
                    EditorGUILayout.PropertyField(m_BytesSupplierObject, new GUIContent("Supplier Object"));

                    int selectedIndex = 0;

                    if (m_BytesSupplierObject.objectReferenceValue != null)
                    {
                        try
                        {
                            List<BytesSupplierInfo> possibleSuppliers = new List<BytesSupplierInfo>();
                            possibleSuppliers.Add(new BytesSupplierInfo(null, null, ""));

                            Component[] components = ((GameObject)m_BytesSupplierObject.objectReferenceValue).GetComponents(typeof(Component));

                            foreach (Component component in components)
                            {
                                Type type = component.GetType();
                                MethodInfo[] methods = type.GetMethods();

                                if (methods.Length == 0)
                                    continue;

                                foreach (MethodInfo method in methods)
                                {
                                    if ((method.GetParameters() == null || method.GetParameters().Length == 0)
                                        && method.ReturnType == typeof(byte[]))
                                    {
                                        possibleSuppliers.Add(new BytesSupplierInfo(m_BytesSupplierObject.objectReferenceValue as GameObject, component,
                                            method.Name));

                                        if (oldObject == m_BytesSupplierObject.objectReferenceValue
                                            && method.Name == m_BytesSupplierFunctionName.stringValue
                                            && component == m_BytesSupplierComponent.objectReferenceValue)
                                        {
                                            selectedIndex = possibleSuppliers.Count - 1;
                                        }
                                    }
                                }
                            }

                            string[] supplierTitles = new string[possibleSuppliers.Count];

                            for (int i = 0; i < supplierTitles.Length; ++i)
                            {
                                if (i == 0)
                                {
                                    supplierTitles[0] = "No Function";
                                    continue;
                                }
                                supplierTitles[i] = possibleSuppliers[i].m_Behaviour.name + "." +
                                                    possibleSuppliers[i].m_MethodName;
                            }

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Supplier Function");

                            var choiceIndex = EditorGUILayout.Popup(selectedIndex, supplierTitles);

                            if (choiceIndex == 0)
                            {
                                m_BytesSupplierComponent.objectReferenceValue = null;
                                m_BytesSupplierFunctionName.stringValue = "";
                            }
                            else
                            {
                                m_BytesSupplierComponent.objectReferenceValue = possibleSuppliers[choiceIndex].m_Behaviour;
                                m_BytesSupplierFunctionName.stringValue = possibleSuppliers[choiceIndex].m_MethodName;
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        catch (Exception)
                        {
                            m_BytesSupplierComponent.objectReferenceValue = null;
                            m_BytesSupplierFunctionName.stringValue = "";
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (fileSourceType == PDFViewer.FileSourceType.Asset)
                {
                    EditorGUILayout.PropertyField(m_PDFAsset);
                }

                EditorGUILayout.PropertyField(m_LoadOnEnable);
                EditorGUILayout.PropertyField(m_UnloadOnDisable);

                if (fileSourceType == PDFViewer.FileSourceType.Asset)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("");
                    EditorGUILayout.HelpBox("To convert pdf file to .asset right click on pdf and select \"PDF Renderer\\Convert to .asset\"", MessageType.Info);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(4.0f);
            }
            GUILayout.EndVertical();
        }

        private void InspectSecuritySettings()
        {
            GUILayout.BeginVertical("Box");

            if (EnterGroup("Security Settings", "Paroxe.PdfRenderer.PDFViewer.ShowPasswordSettings"))
            {
                EditorGUILayout.PropertyField(m_Password);

                GUILayout.Space(4.0f);
            }

            GUILayout.EndVertical();
        }

        private void InspectViewerSettings()
        {
            PDFViewer viewer = (PDFViewer)target;

            GUILayout.BeginVertical("Box");

            if (EnterGroup("Viewer Settings", "Paroxe.PdfRenderer.PDFViewer.ShowViewerSettings"))
            {
                EditorGUILayout.PropertyField(m_PageFitting);

                PDFViewer.PageFittingType pageFitting = (PDFViewer.PageFittingType)m_PageFitting.enumValueIndex;

                if (pageFitting == PDFViewer.PageFittingType.Zoom || Application.isPlaying)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_ZoomFactor, GUILayout.ExpandWidth(false));
                    EditorGUI.indentLevel--;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_VerticalMarginBetweenPages, new GUIContent("Page Margins (px)"), GUILayout.ExpandWidth(false));

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_VerticalMarginBetweenPages.floatValue < 0.0f)
                        m_VerticalMarginBetweenPages.floatValue = 0.0f;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_MinZoomFactor, GUILayout.ExpandWidth(false));

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_MinZoomFactor.floatValue < 0.01f)
                        m_MinZoomFactor.floatValue = 0.01f;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_MaxZoomFactor, GUILayout.ExpandWidth(false));

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_MaxZoomFactor.floatValue < m_MinZoomFactor.floatValue)
                        m_MaxZoomFactor.floatValue = m_MinZoomFactor.floatValue;
                }

                EditorGUILayout.PropertyField(m_ZoomStep, GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(m_ScrollSensitivity, new GUIContent("Scroll Sensitivity (px)"), GUILayout.ExpandWidth(false));

                Rect controlRect = EditorGUILayout.GetControlRect(true);
                GUIContent guiLabel = new GUIContent("Show Top Bar");

                using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_ShowTopBar))
                {
                    bool showTopBar = EditorGUI.Toggle(controlRect, guiLabel, viewer.ShowTopBar);

                    if (showTopBar != viewer.ShowTopBar)
                    {
                        Undo.SetCurrentGroupName("PDFViewer.ShowTopBar");
                        int group = Undo.GetCurrentGroup();
                        Undo.RegisterFullObjectHierarchyUndo(viewer.m_Internal, "PDFViewer.ShowTopBar");

                        viewer.ShowTopBar = showTopBar;

                        Undo.CollapseUndoOperations(group);

                        m_ShowTopBar.boolValue = viewer.ShowTopBar;
                    }
                }

                controlRect = EditorGUILayout.GetControlRect(true);
                guiLabel = new GUIContent("Show VScrollBar");

                using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_ShowVerticalScrollBar))
                {
                    bool showVerticalScrollBar = EditorGUI.Toggle(controlRect, guiLabel, viewer.ShowVerticalScrollBar);

                    if (showVerticalScrollBar != viewer.ShowVerticalScrollBar)
                    {
                        Undo.SetCurrentGroupName("PDFViewer.VScrollBar");
                        int group = Undo.GetCurrentGroup();
                        Undo.RegisterFullObjectHierarchyUndo(viewer.m_Internal, "PDFViewer.VScrollBar");

                        viewer.ShowVerticalScrollBar = showVerticalScrollBar;

                        Undo.CollapseUndoOperations(group);

                        m_ShowVerticalScrollBar.boolValue = viewer.ShowVerticalScrollBar;
                    }
                }

                controlRect = EditorGUILayout.GetControlRect(true);
                guiLabel = new GUIContent("Show HScrollBar");

                using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_ShowHorizontalScrollBar))
                {
                    bool showHorizontalScrollBar = EditorGUI.Toggle(controlRect, "Show HScrollBar", viewer.ShowHorizontalScrollBar);

                    if (showHorizontalScrollBar != viewer.ShowHorizontalScrollBar)
                    {
                        Undo.SetCurrentGroupName("PDFViewer.HScrollBar");
                        int group = Undo.GetCurrentGroup();
                        Undo.RegisterFullObjectHierarchyUndo(viewer.m_Internal, "PDFViewer.HScrollBar");

                        viewer.ShowHorizontalScrollBar = showHorizontalScrollBar;

                        Undo.CollapseUndoOperations(group);

                        m_ShowHorizontalScrollBar.boolValue = viewer.ShowHorizontalScrollBar;
                    }
                }


                if (viewer.m_Internal.m_LeftPanel != null)
                {
                    controlRect = EditorGUILayout.GetControlRect(true);
                    guiLabel = new GUIContent("Show Bookmarks Viewer");

                    using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_ShowBookmarksViewer))
                    {
                        bool showBookmarksViewer = EditorGUI.Toggle(controlRect, guiLabel, viewer.ShowBookmarksViewer);

                        if (showBookmarksViewer != viewer.ShowBookmarksViewer)
                        {
                            Undo.SetCurrentGroupName("PDFViewer.ShowBookmarksViewer");
                            int group = Undo.GetCurrentGroup();
                            Undo.RegisterFullObjectHierarchyUndo(viewer.m_Internal, "PDFViewer.ShowBookmarksViewer");

                            viewer.ShowBookmarksViewer = showBookmarksViewer;

                            Undo.CollapseUndoOperations(group);

                            m_ShowBookmarksViewer.boolValue = viewer.ShowBookmarksViewer;
                        }
                    }

                    controlRect = EditorGUILayout.GetControlRect(true);
                    guiLabel = new GUIContent("Show Thumbnails Viewer");

                    using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_ShowThumbnailsViewer))
                    {
                        bool showThumbnailsViewer = EditorGUI.Toggle(controlRect, guiLabel, viewer.ShowThumbnailsViewer);

                        if (showThumbnailsViewer != viewer.ShowThumbnailsViewer)
                        {
                            Undo.SetCurrentGroupName("PDFViewer.ShowThumbnailsViewer");
                            int group = Undo.GetCurrentGroup();
                            Undo.RegisterFullObjectHierarchyUndo(viewer.m_Internal, "PDFViewer.ShowThumbnailsViewer");

                            viewer.ShowThumbnailsViewer = showThumbnailsViewer;

                            Undo.CollapseUndoOperations(group);

                            m_ShowThumbnailsViewer.boolValue = viewer.ShowThumbnailsViewer;
                        }
                    }
                }

                Color oldColor = viewer.BackgroundColor;
                Color newColor = EditorGUILayout.ColorField("Viewer BG Color", viewer.BackgroundColor);

                if (oldColor != newColor)
                {
                    Undo.RecordObject(viewer.m_Internal.m_Viewport.GetComponent<Image>(), "PDFViewerBackground");

                    viewer.BackgroundColor = newColor;
                }

                GUILayout.Space(4.0f);
            }
            GUILayout.EndVertical();
        }

        private void InspectLinksSettings()
        {
            GUILayout.BeginVertical("Box");

            if (EnterGroup("Links Settings", "Paroxe.PdfRenderer.PDFViewer.ShowLinksSettings"))
            {
                EditorGUILayout.PropertyField(m_ChangeCursorWhenOverURL, new GUIContent("Change Cursor"));
                EditorGUILayout.PropertyField(m_AllowOpenURL, new GUIContent("Allow Open URL"));

                GUILayout.Space(4.0f);
            }

            GUILayout.EndVertical();
        }

        private void InspectSearchSettings()
        {
            PDFViewer viewer = (PDFViewer)target;

            if (viewer.m_Internal.m_SearchPanel != null)
            {
                GUILayout.BeginVertical("Box");

                if (EnterGroup("Search Settings", "Paroxe.PdfRenderer.PDFViewer.ShowSearchSettings"))
                {
                    EditorGUILayout.PropertyField(m_SearchResultColor, new GUIContent("Result Color"));
                    EditorGUILayout.PropertyField(m_SearchResultPadding, new GUIContent("Result Padding"));
                    EditorGUILayout.PropertyField(m_SearchTimeBudgetPerFrame, new GUIContent("Time (% per frame)"));

                    GUILayout.Space(4.0f);
                }

                GUILayout.EndVertical();
            }
        }

        private void InspectOtherSettings()
        {
            GUILayout.BeginVertical("Box");

            if (EnterGroup("Other Settings", "Paroxe.PdfRenderer.PDFViewer.ShowOtherSettings"))
            {
                EditorGUILayout.PropertyField(m_ParagraphZoomingEnable, new GUIContent("Paragraph Zooming"), GUILayout.ExpandWidth(false));

                if (m_ParagraphZoomingEnable.boolValue)
                {
                    EditorGUILayout.PropertyField(m_ParagraphZoomFactor, new GUIContent("    Zoom Factor"), GUILayout.ExpandWidth(false));
                    EditorGUILayout.PropertyField(m_ParagraphDetectionThreshold, new GUIContent("    Detection Threshold (px)"), GUILayout.ExpandWidth(false));
                }

                EditorGUILayout.PropertyField(m_PageTileTexture);
                EditorGUILayout.PropertyField(m_PageColor);

                GUILayout.Space(4.0f);
            }

            GUILayout.EndVertical();
        }

        private void InspectRenderingSettings()
        {
            PDFViewer viewer = (PDFViewer)target;

            GUILayout.BeginVertical("Box");

            if (EnterGroup("Rendering Settings", "Paroxe.PdfRenderer.PDFViewer.ShowRenderSettings"))
            {
                Rect controlRect = EditorGUILayout.GetControlRect(true);
                GUIContent guiLabel = new GUIContent("Maximum Quality");

                using (new EditorGUI.PropertyScope(controlRect, guiLabel, m_MaxZoomFactorTextureQuality))
                {
                    float maxRenderingQuality = EditorGUI.FloatField(controlRect, guiLabel, viewer.MaxZoomFactorTextureQuality);

                    if (maxRenderingQuality != viewer.MaxZoomFactorTextureQuality)
                    {
                        viewer.MaxZoomFactorTextureQuality = maxRenderingQuality;

                        m_MaxZoomFactorTextureQuality.floatValue = viewer.MaxZoomFactorTextureQuality;
                    }
                }

                EditorGUILayout.PropertyField(m_RenderAnnotations, new GUIContent("Render Annotations"), GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(m_RenderGrayscale, new GUIContent("Render Grayscale"), GUILayout.ExpandWidth(false));

                GUILayout.Space(4.0f);
            }

            GUILayout.EndVertical();
        }

        private static bool EnterGroup(string title, string key)
        {
            bool enterGroup = EditorPrefs.GetBool(key, true);

            if (enterGroup != PRHelper.GroupHeader(title, enterGroup))
            {
                enterGroup = !enterGroup;

                EditorPrefs.SetBool(key, enterGroup);
            }

            return enterGroup;
        }

        private static void ShowInExplorer(string filePath)
        {
            filePath = Path.GetFullPath(filePath.Replace(@"/", @"\"));

            if (File.Exists(filePath))
                System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
        }

        private static bool Browse(string startPath, ref string filename, ref string folder, ref string filePath, ref bool isStreamingAsset, ref bool isPersistentData, ref bool isResourcesAsset, ref bool isFilePath)
        {
            bool result = false;
            string path = EditorUtility.OpenFilePanel("Browse video file", startPath, "*");

            if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                projectRoot = projectRoot.Replace('\\', '/');

                if (path.StartsWith(projectRoot))
                {
                    if (path.StartsWith(Application.streamingAssetsPath))
                    {
                        path = path.Remove(0, Application.streamingAssetsPath.Length);
                        filename = Path.GetFileName(path);
                        path = Path.GetDirectoryName(path);
                        if (path.StartsWith(Path.DirectorySeparatorChar.ToString()) || path.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                        {
                            path = path.Remove(0, 1);
                        }
                        folder = path;

                        isPersistentData = false;
                        isStreamingAsset = true;
                        isResourcesAsset = false;
                        isFilePath = false;
                    }
                    else if (path.StartsWith(Application.persistentDataPath))
                    {
                        path = path.Remove(0, Application.persistentDataPath.Length);
                        filename = Path.GetFileName(path);
                        path = Path.GetDirectoryName(path);
                        if (path.StartsWith(Path.DirectorySeparatorChar.ToString()) || path.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                        {
                            path = path.Remove(0, 1);
                        }
                        folder = path;

                        isPersistentData = true;
                        isStreamingAsset = false;
                        isResourcesAsset = false;
                        isFilePath = false;
                    }
                    else if (path.StartsWith(Application.dataPath + "/Resources"))
                    {
                        path = path.Remove(0, (Application.dataPath + "/Resources").Length);
                        filename = Path.GetFileName(path);
                        path = Path.GetDirectoryName(path);
                        if (path.StartsWith(Path.DirectorySeparatorChar.ToString()) || path.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
                        {
                            path = path.Remove(0, 1);
                        }
                        folder = path;

                        isPersistentData = false;
                        isStreamingAsset = false;
                        isResourcesAsset = true;
                        isFilePath = false;
                    }
                    else
                    {
                        path = path.Remove(0, projectRoot.Length + 1);
                        filePath = path;

                        isPersistentData = false;
                        isStreamingAsset = false;
                        isResourcesAsset = false;
                        isFilePath = true;
                    }
                }
                else
                {
                    filePath = path;

                    isPersistentData = false;
                    isStreamingAsset = false;
                    isResourcesAsset = false;
                    isFilePath = true;
                }

                result = true;
            }
            return result;
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

        private class BytesSupplierInfo
        {
            public Component m_Behaviour;
            public GameObject m_GameObject;
            public string m_MethodName;

            public BytesSupplierInfo(GameObject gameObject, Component component, string methodName)
            {
                m_GameObject = gameObject;
                m_Behaviour = component;
                m_MethodName = methodName;
            }
        }
    }
}