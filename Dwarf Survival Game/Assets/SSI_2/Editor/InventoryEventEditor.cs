using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(InventoryEvent))]
public class InventoryEventEditor : PropertyDrawer
{
    private float height;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height + (property.isExpanded ? (property.FindPropertyRelative("dataToSend").enumValueIndex != 4 ? EditorGUIUtility.singleLineHeight * 8 + 12 : EditorGUIUtility.singleLineHeight * 7 + 10) : EditorGUIUtility.singleLineHeight);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        height = 0;
        EditorGUI.BeginProperty(position, label, property);

        var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        position.y += EditorGUIUtility.singleLineHeight;

        
        if (property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, property.displayName, true))
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 2;

            var nameRect = new Rect(position.x, position.y, 300, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            var messageRect = new Rect(position.x, position.y, 300, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            var activateRect = new Rect(position.x, position.y, 300, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            var transformRect = new Rect(position.x, position.y, 300, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            var quantityRect = new Rect(position.x, position.y, 300, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("eventName"), new GUIContent("Event Name"));
            EditorGUI.PropertyField(messageRect, property.FindPropertyRelative("eventMessage"), new GUIContent("Event Message"));
            EditorGUI.PropertyField(activateRect, property.FindPropertyRelative("eventActivated"), new GUIContent("Event Activated?"));
            EditorGUI.PropertyField(transformRect, property.FindPropertyRelative("obj"), new GUIContent("Send To"));
            EditorGUI.PropertyField(quantityRect, property.FindPropertyRelative("includeQuantityOfItem"), new GUIContent("Include Quantity?"));
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("dataToSend"), new GUIContent("Data Type"));

            if (property.FindPropertyRelative("dataToSend").enumValueIndex == 0)
            {
                var paramRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(paramRect, property.FindPropertyRelative("stringParam"), new GUIContent("Event Parameter"));

                position.y += EditorGUIUtility.singleLineHeight + 2;
            } else if (property.FindPropertyRelative("dataToSend").enumValueIndex == 1)
            {
                var paramRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(paramRect, property.FindPropertyRelative("intParam"), new GUIContent("Event Parameter"));

                position.y += EditorGUIUtility.singleLineHeight + 2;
            }
            if (property.FindPropertyRelative("dataToSend").enumValueIndex == 2)
            {
                var paramRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(paramRect, property.FindPropertyRelative("floatParam"), new GUIContent("Event Parameter"));

                position.y += EditorGUIUtility.singleLineHeight + 2;
            }
            if (property.FindPropertyRelative("dataToSend").enumValueIndex == 3)
            {
                var paramRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(paramRect, property.FindPropertyRelative("boolParam"), new GUIContent("Event Parameter"));

                position.y += EditorGUIUtility.singleLineHeight + 2;
            }

            EditorGUI.indentLevel = indent;
        }

        EditorGUI.EndProperty();
    }
}