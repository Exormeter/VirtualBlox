using UnityEditor;

namespace Valve.VR.InteractionSystem
{
    [CustomEditor(typeof(UnityEngine.UI.ColorToggle))]
    public class ColorToggleEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            UnityEngine.UI.ColorToggle targetColorToggle = (UnityEngine.UI.ColorToggle)target;

            targetColorToggle.blockColor = (BLOCKCOLOR)EditorGUILayout.EnumPopup("Block Color", targetColorToggle.blockColor);

            // Show default inspector property editor
            base.OnInspectorGUI();
        }
    }
}
