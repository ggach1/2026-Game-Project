using UnityEditor;
using UnityEngine;

namespace KMJ.Code.Object
{
    [CustomEditor(typeof(GrowObject))]
    public class GrowObjectEditor : Editor
    {
        private SerializedProperty isReturnOwnScale;
        private SerializedProperty waitTime;

        private void OnEnable()
        {
            isReturnOwnScale = serializedObject.FindProperty("isReturnOwnScale");
            waitTime = serializedObject.FindProperty("waitTime");
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
                else if (property.name == "isReturnOwnScale")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                else if (property.name == "waitTime")
                {
                    if (isReturnOwnScale.boolValue)
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