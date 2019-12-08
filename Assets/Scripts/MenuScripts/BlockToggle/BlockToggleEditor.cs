using UnityEditor;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [CustomEditor(typeof(UnityEngine.UI.ColorToggle))]
    public class BlockColorEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            UnityEngine.UI.BlockToggle targetBlockToggle = (UnityEngine.UI.BlockToggle)target;

            targetBlockToggle.Rows = EditorGUILayout.IntField("Block Rows", targetBlockToggle.Rows);
            targetBlockToggle.Columns = EditorGUILayout.IntField("Block Columns", targetBlockToggle.Columns);

            // Show default inspector property editor
            base.OnInspectorGUI();
        }
    }
}
