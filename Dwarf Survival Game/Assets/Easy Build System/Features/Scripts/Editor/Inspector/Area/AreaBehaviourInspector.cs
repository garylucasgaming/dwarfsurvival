using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Area.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Area
{
    [CustomEditor(typeof(AreaBehaviour))]
    public class AreaBehaviourInspector : UnityEditor.Editor
    {
        #region Fields

        public static Rect OffsetEditingWindow = new Rect(50, 50, 300, 280);

        private AreaBehaviour Target;

        private static bool GeneralFoldout;
        private static bool ConditionsFoldout;
        private static bool AddonsFoldout;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (AreaBehaviour)target;

            AddonInspector.LoadAddons(Target, AddonTarget.AreaBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.AreaBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Area Behaviour General

            InspectorStyles.DrawSectionLabel("Area Behaviour - General");

            GeneralFoldout = EditorGUILayout.Foldout(GeneralFoldout, "General Settings", true);

            if (GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Shape"), new GUIContent("Area Shape Type :", "Define the shape of area."));

                if (Target.Shape == AreaShape.Bounds)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Bounds"), new GUIContent("Area Shape Size :", "Define the bounds of area."));
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"), new GUIContent("Area Shape Radius :", "Define the radius of area."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllPlacement"), new GUIContent("Area Allow All Placement :", "Allow all the placement."));

                if (!Target.AllowAllPlacement)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowPlacementSpecificPieces"), new GUIContent("Area Allow Specific Piece(s) In Placement :", "Allow specific piece(s) to allow the placement."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllDestruction"), new GUIContent("Area Allow All Destruction :", "Allow all the destruction."));

                if (!Target.AllowAllDestruction)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowDestructionSpecificPieces"), new GUIContent("Area Allow Specific Piece(s) In Destruction :", "Allow specific piece(s) to allow the destruction."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllEdition"), new GUIContent("Area Allow All Edition :", "Allow all the edition."));

                if (!Target.AllowAllEdition)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowEditionSpecificPieces"), new GUIContent("Area Allow Specific Piece(s) In Edition :", "Allow specific piece(s) to allow the edition."), true);
            }

            #endregion Area Behaviour General

            #region Area Behaviour Conditions

            InspectorStyles.DrawSectionLabel("Area Behaviour - Conditions");

            ConditionsFoldout = EditorGUILayout.Foldout(ConditionsFoldout, "Conditions Settings", true);

            if (ConditionsFoldout)
            {
                ConditionInspector.DrawConditions(Target, ConditionTarget.AreaBehaviour);
            }

            #endregion

            #region Area Behaviour Addons

            InspectorStyles.DrawSectionLabel("Area Behaviour - Addons");

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Addons Settings", true);

            if (AddonsFoldout)
            {
                AddonInspector.DrawAddons(Target, AddonTarget.AreaBehaviour);
            }

            #endregion Area Behaviour Addons

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }
}