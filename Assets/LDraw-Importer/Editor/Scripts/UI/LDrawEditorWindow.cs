using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LDraw
{
    public class LDrawEditorWindow : EditorWindow
    {
        [MenuItem("Window/LDrawImporter/Open Importer")]
        public static void Create()
        {
            var window = GetWindow<LDrawEditorWindow>("LDrawImporter");
            window.position = new Rect(100, 100, 400, 400);
            window.Show();
        }

        private string[] _PartNames;
        private string _CurrentPart;
        private int _CurrentIndex;
        private GeneratingType _CurrentType;
        private LDrawModelConverter lDrawModelConverter = new LDrawModelConverter();

        private void OnEnable()
        {
            _PartNames = LDrawConfig.Instance.PartFileNames;
        }

        private void OnGUI()
        {
            GUILayout.Label("This is LDraw model importer for file format v1.0.2");
            if (GUILayout.Button("Update blueprints"))
            {
                LDrawConfig.Instance.InitParts();
                _PartNames = LDrawConfig.Instance.PartFileNames;
            }
            _CurrentType = (GeneratingType) EditorGUILayout.EnumPopup("Blueprint Type", _CurrentType);
            switch (_CurrentType)
            {
                    case GeneratingType.ByName:
                        _CurrentPart = EditorGUILayout.TextField("Name", _CurrentPart);
                        break;
                    case GeneratingType.Models:
                        _CurrentIndex = EditorGUILayout.Popup("Parts", _CurrentIndex, _PartNames);
                        break;
            }
      
            GenerateModelButton();
        }

        private void GenerateModelButton()
        {
            if (GUILayout.Button("Generate"))
            {
                var model = LDrawModel.Create(_PartNames[_CurrentIndex], LDrawConfig.Instance.GetSerializedPart(_PartNames[_CurrentIndex]));
                lDrawModelConverter.ConvertLDrawModel(model);
            }
        }

        private enum GeneratingType
        {
            ByName,
            Models
        }
        private const string PathToModels = "Assets/LDraw-Importer/Editor/base-parts/";
    }
}