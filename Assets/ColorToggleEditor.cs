using UnityEditor;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [CustomEditor(typeof(UnityEngine.UI.ColorToggle))]
    public class ColorToggleEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            UnityEngine.UI.ColorToggle targetColorToggle = (UnityEngine.UI.ColorToggle)target;

            targetColorToggle.blockColor = EditorGUILayout.ColorField("Block Color", targetColorToggle.blockColor);

            // Show default inspector property editor
            base.OnInspectorGUI();
        }
    }
}
