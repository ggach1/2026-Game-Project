using UnityEditor;
using UnityEngine;

namespace KMJ.Code.Object
{
    [CustomEditor(typeof(TargetMoveObject))]
    public class TargetMoveObjectEditor : Editor
    {
        private SerializedProperty isSecondMove;
        private SerializedProperty isOnceMove;
        private SerializedProperty secondTarget;
        private SerializedProperty onceTarget;

        private void OnEnable()
        {
            isSecondMove = serializedObject.FindProperty("isSecondMove");
            secondTarget = serializedObject.FindProperty("secondTarget");
            isOnceMove = serializedObject.FindProperty("isOnceMove");
            onceTarget = serializedObject.FindProperty("onceTarget");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.name == "m_Script")
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(property, true);
                    EditorGUI.EndDisabledGroup();
                }
                else if (property.name == "isSecondMove")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                else if (property.name == "secondTarget")
                {
                    if (isSecondMove.boolValue)
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
                else if (property.name == "isOnceMove")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                else if (property.name == "onceTarget")
                {
                    if (isOnceMove.boolValue)
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}