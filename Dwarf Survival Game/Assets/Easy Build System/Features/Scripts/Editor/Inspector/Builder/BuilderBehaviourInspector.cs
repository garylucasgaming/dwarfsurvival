using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Builder
{
    [CustomEditor(typeof(BuilderBehaviour), true)]
    public class BuilderBehaviourInspector : UnityEditor.Editor
    {
        #region Fields

        private BuilderBehaviour Target;

        private static bool GeneralFoldout;
        private static bool PreviewFoldout;
        private static bool ModesFoldout;
        private static bool AudioFoldout;
        private static bool AddonsFoldout;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (BuilderBehaviour)target;

            AddonInspector.LoadAddons(Target, AddonTarget.BuilderBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Builder Behaviour General

            InspectorStyles.DrawSectionLabel("Builder Behaviour - General");

            GeneralFoldout = EditorGUILayout.Foldout(GeneralFoldout, "General Settings", true);

            if (GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraType"), new GUIContent("Builder View Type :", "Define camera type.\n" +
                    "First person : The raycast origin come from camera center.\n" +
                    "Top down : The raycast origin come from mouse position.\n" +
                    "Third person : The raycast origin come from defined reference Third Person Origin Transform."));

                #if ENABLE_INPUT_SYSTEM
                if (((RayType)serializedObject.FindProperty("CameraType").enumValueIndex) == RayType.VirtualRealityPerson)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RayInteractor"), new GUIContent("Builder Ray Interactor :", ""));
                }
                #endif

                if (((RayType)serializedObject.FindProperty("CameraType").enumValueIndex) == RayType.TopDown)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastAnchorTransform"), new GUIContent("Builder Top Down Anchor Transform :", "Define the origin transform where the ray will be sent (Not required)."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SnapThreshold"), new GUIContent("Builder Top Down Snap Threshold :", "Define the threshold distance on which the preview can will be moved in allowed status."));
                }
                else if (((RayType)serializedObject.FindProperty("CameraType").enumValueIndex) == RayType.ThirdPerson)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOriginTransform"), new GUIContent("Builder Third Person Transform :", "Define the origin transform where the ray will be sent."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RayDetection"), new GUIContent("Builder Detection Type :", "Define the ray of detection.\nIt is recommended to use the following types :\nRaycast: if you've sockets with the type Attachment.\nOverlap Sphere: if you've sockets with the type (Point)."));

                if (serializedObject.FindProperty("RayDetection").enumValueIndex == (int)DetectionType.Overlap)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OverlapAngles"), new GUIContent("Builder Overlap Max Angles :", "Define the maximum angles to detect the sockets."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionDistance"), new GUIContent("Builder Max Distance :", "Define the maximum distance on which the preview can will be moved in allowed status."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("OutOfRangeDistance"), new GUIContent("Builder Out Of Range Distance :", "Define the maximum out of range distance on which the preview can will be moved in denied status. (0 = Use only Placement Distance)"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOffset"), new GUIContent("Builder Detection Origin Offset :", "Define the origin offset."));
            }

            #endregion

            #region Builder Preview Settings

            InspectorStyles.DrawSectionLabel("Builder Behaviour - Preview");

            PreviewFoldout = EditorGUILayout.Foldout(PreviewFoldout, "Preview Settings", true);

            if (PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewMovementType"), new GUIContent("Builder Preview Movement Type :", "Define the preview movement type."));

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Smooth)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewSmoothTime"), new GUIContent("Builder Preview Movement Smooth Time :", "Define the smooth time movement."));
                }

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Grid)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridSize"), new GUIContent("Builder Preview Grid Size :", "Define the grid size on which the preview will be moved."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridOffset"), new GUIContent("Builder Preview Grid Offset :", "Define the grid offset for the preview position."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewMovementOnlyAllowed"), new GUIContent("Builder Preview Movement Only Allowed :", "This allows to move the preview only if the placement is possible."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("LockRotation"), new GUIContent("Builder Preview Movement Lock Rotation :", "This allows to make that the preview look the camera rotation."));
            }

            #endregion Builder Preview Settings

            #region Builder Modes Settings

            InspectorStyles.DrawSectionLabel("Builder Behaviour - Modes");

            ModesFoldout = EditorGUILayout.Foldout(ModesFoldout, "Modes Settings", true);

            if (ModesFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UsePlacementMode"), new GUIContent("Builder Use Placement Mode :", "This allows to allow the placement mode."));

                if (serializedObject.FindProperty("UsePlacementMode").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterPlacement"), new GUIContent("Builder Reset Mode After Placement :", "This allows to reset the mode to (None) after the placement."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForPlacement"), new GUIContent("Builder Require Area For Placement :", "This allows to require a nearest Area Behaviour for allow the placement."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseDestructionMode"), new GUIContent("Builder Use Destruction Mode :", "This allows to allow the destruction mode."));

                if (serializedObject.FindProperty("UseDestructionMode").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterDestruction"), new GUIContent("Builder Reset Mode After Destruction :", "This allows to reset the mode to (None) after the destruction."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForDestruction"), new GUIContent("Builder Require Area For Destruction :", "This allows to require a nearest Area Behaviour for allow the destruction."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseEditionMode"), new GUIContent("Builder Use Edition Mode :", "This allows to allow the edition mode."));

                if (serializedObject.FindProperty("UseEditionMode").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterEdition"), new GUIContent("Builder Reset Mode After Edition :", "This allows to reset the mode to (None) after the destruction."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForEdition"), new GUIContent("Builder Require Area For Edition :", "This allows to require a nearest Area Behaviour for allow the edition."));
                }
            }

            #endregion Builder Modes Settings

            #region Builder Audio Settings

            InspectorStyles.DrawSectionLabel("Builder Behaviour - Audio");

            AudioFoldout = EditorGUILayout.Foldout(AudioFoldout, "Audio Settings", true);

            if (AudioFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Source"), new GUIContent("Builder Audio Source :", "This source is the source on which the sounds will be played."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlacementClips"), new GUIContent("Builder Audio Placement Clip(s) :", "Placement clips at play when a preview is placed (Randomly played)."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestructionClips"), new GUIContent("Builder Audio Destruction Clip(s) :", "Destruction clips at play when a piece is destroyed (Randomly played)."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EditionClips"), new GUIContent("Builder Audio Edition Clip(s) :", "Destruction clips at play when a piece is destroyed (Randomly played)."), true);
                GUILayout.EndHorizontal();
            }

            #endregion Builder Audio Settings

            #region Builder Add-Ons Settings

            InspectorStyles.DrawSectionLabel("Builder Behaviour - Addons");

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Addons Settings", true);

            if (AddonsFoldout)
            {
                AddonInspector.DrawAddons(Target, AddonTarget.BuilderBehaviour);
            }

            #endregion Builder Add-Ons Settings

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}