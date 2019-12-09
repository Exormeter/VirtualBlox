using UnityEditor;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [CustomEditor(typeof(ColorToggle))]
    public class ColorToggleEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            ColorToggle targetColorToggle = (ColorToggle)target;

            targetColorToggle.blockColor = EditorGUILayout.ColorField("Block Color", targetColorToggle.blockColor);

            // Show default inspector property editor
            base.OnInspectorGUI();
        }
    }
}
