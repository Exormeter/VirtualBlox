using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ColorToggle), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the Toggle Component.
    /// Extend this class to write a custom editor for a component derived from Toggle.
    /// </summary>
    public class ColorToggleEditor : SelectableEditor
    {
        SerializedProperty m_OnValueChangedProperty;
        SerializedProperty m_TransitionProperty;
        SerializedProperty m_GraphicProperty;
        SerializedProperty m_GroupProperty;
        SerializedProperty m_IsOnProperty;
        SerializedProperty m_BlockColor;
        protected override void OnEnable()
        {
            base.OnEnable();

            m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
            m_GraphicProperty = serializedObject.FindProperty("graphic");
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
            m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
            m_BlockColor = serializedObject.FindProperty("blockColor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            ColorToggle toggle = serializedObject.targetObject as ColorToggle;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsOnProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
                ColorToggleGroup group = m_GroupProperty.objectReferenceValue as ColorToggleGroup;

                toggle.isOn = m_IsOnProperty.boolValue;

                if (group != null && toggle.IsActive())
                {
                    if (toggle.isOn || (!group.AnyTogglesOn() && !group.allowSwitchOff))
                    {
                        toggle.isOn = true;
                        group.NotifyToggleOn(toggle);
                    }
                }
            }
            EditorGUILayout.PropertyField(m_TransitionProperty);
            EditorGUILayout.PropertyField(m_GraphicProperty);
            EditorGUILayout.PropertyField(m_BlockColor);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_GroupProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
                ColorToggleGroup group = m_GroupProperty.objectReferenceValue as ColorToggleGroup;
                toggle.group = group;
            }

            EditorGUILayout.Space();

            // Draw the event notification options
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
