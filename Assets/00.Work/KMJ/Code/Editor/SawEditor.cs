using KMJ.Code.Object;
using UnityEditor;

namespace KMJ.Code.Object
{
    [CustomEditor(typeof(Saw))]
    public class SawEditor : Editor
    {
        private SerializedProperty isOwnDirection;
        private SerializedProperty ownDirection;

        private void OnEnable()
        {
            isOwnDirection = serializedObject.FindProperty("isOwnDirection");
            ownDirection = serializedObject.FindProperty("ownDirection");
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
                else if (property.name == "isOwnDirection")
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                else if (property.name == "ownDirection")
                {
                    if (isOwnDirection.boolValue)
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