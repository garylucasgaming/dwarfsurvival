using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Extensions;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Group
{
    [CustomEditor(typeof(GroupBehaviour))]
    public class GroupBehaviourInspector : UnityEditor.Editor
    {
        #region Fields

        public static Rect OffsetEditingWindow = new Rect(50, 50, 300, 280);

        private GroupBehaviour Target;

        private static bool GeneralFoldout;
        private static bool AddonsFoldout;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (GroupBehaviour)target;

            AddonInspector.LoadAddons(Target, AddonTarget.GroupBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region General

            InspectorStyles.DrawSectionLabel("Group Behaviour - General");

            GeneralFoldout = EditorGUILayout.Foldout(GeneralFoldout, "General Settings", true);

            if (GeneralFoldout)
            {
                if (GUILayout.Button("Save As Blueprint Template"))
                {
                    if (Target.transform.GetComponentsInChildren<PieceBehaviour>().Length > 0)
                    {
                        Core.Scriptables.Blueprint.BlueprintTemplate Data = ScriptableObjectExtension.CreateAsset<Core.Scriptables.Blueprint.BlueprintTemplate>("New Empty Blueprint");
                        Data.name = Target.name;
                        Data.Model = Target.GetModel();
                        Data.Data = Target.GetModel().ToJson();
                    }
                }
            }

            #endregion

            InspectorStyles.DrawSectionLabel("Group Behaviour - Addons");

            #region Group Add-Ons Settings

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Addons Settings", true);

            if (AddonsFoldout)
            {
                AddonInspector.DrawAddons(Target, AddonTarget.GroupBehaviour);
            }

            #endregion Group Add-Ons Settings

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }
}