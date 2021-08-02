using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Blueprint
{
    [CustomEditor(typeof(Core.Scriptables.Blueprint.BlueprintTemplate), true)]
    public class BlueprintInspector : UnityEditor.Editor
    {
        #region Fields

        private Core.Scriptables.Blueprint.BlueprintTemplate Target;

        private Vector2 TextScrollPosition;

        #endregion Fields

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Blueprint Data General

            InspectorStyles.DrawSectionLabel("Blueprint Template - General");

            if (Target.Model == null)
            {
                GUILayout.BeginHorizontal("box");

                GUILayout.Label("The list does not contains of piece(s).");

                GUILayout.EndHorizontal();
            }
            else
            {
                PieceData.SerializedPiece[] Pieces = new PieceData.SerializedPiece[0];

                if (Target.Data != null)
                {
                    if (Target.Data != string.Empty && Target.Data.Length > 0)
                    {
                        Pieces = Target.Model.DecodeToStr(Target.Data);
                    }
                }

                GUILayout.Label("Pieces Count : " + Pieces.Length);

                GUILayout.Label("Blueprint Data :");

                TextScrollPosition = GUILayout.BeginScrollView(TextScrollPosition, GUILayout.Height(200));

                EditorGUI.BeginChangeCheck();

                GUI.enabled = false;
                Target.Data = EditorGUILayout.TextArea(Target.Data, GUILayout.ExpandHeight(true));
                GUI.enabled = true;

                if (EditorGUI.EndChangeCheck())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }

                GUILayout.EndScrollView();

                GUI.enabled = Target.Model != null && Target.Data != string.Empty;

                if (GUILayout.Button("Load Blueprint In Editor"))
                {
                    BuildManager Manager = FindObjectOfType<BuildManager>();

                    if (Manager == null)
                    {
                        Debug.LogError("<b>Easy Build System</b> : The Build Manager does not exists.");

                        return;
                    }

                    PieceData.SerializedPiece[] Parts = Target.Model.DecodeToStr(Target.Data);

                    GroupBehaviour Parent = new GameObject("(Editor) Blueprint").AddComponent<GroupBehaviour>();

                    for (int i = 0; i < Parts.Length; i++)
                    {
                        PieceBehaviour Part = Manager.GetPieceById(Parts[i].Id);

                        PieceBehaviour PlacedPart = Manager.PlacePrefab(Part, PieceData.ParseToVector3(Parts[i].Position),
                            PieceData.ParseToVector3(Parts[i].Rotation), PieceData.ParseToVector3(Parts[i].Scale), Parent);

                        PlacedPart.ChangeSkin(Parts[i].AppearanceIndex);
                    }
                }

                GUI.enabled = true;

                GUI.enabled = Application.isPlaying;

                if (GUILayout.Button("Load Blueprint In Runtime Editor"))
                {
                    BuildManager Manager = FindObjectOfType<BuildManager>();

                    if (Manager == null)
                    {
                        Debug.LogError("<b>Easy Build System</b> : The Build Manager does not exists.");

                        return;
                    }

                    PieceData.SerializedPiece[] Parts = Target.Model.DecodeToStr(Target.Data);

                    GroupBehaviour Group = new GameObject("(Runtime) Blueprint").AddComponent<GroupBehaviour>();

                    for (int i = 0; i < Parts.Length; i++)
                    {
                        PieceBehaviour Part = Manager.GetPieceById(Parts[i].Id);

                        PieceBehaviour PlacedPart = Manager.PlacePrefab(Part, PieceData.ParseToVector3(Parts[i].Position),
                            PieceData.ParseToVector3(Parts[i].Rotation), PieceData.ParseToVector3(Parts[i].Scale), Group);

                        PlacedPart.ChangeSkin(Parts[i].AppearanceIndex);
                    }
                }

                GUI.enabled = true;
            }

            #endregion Blueprint Data General

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            Target = (Core.Scriptables.Blueprint.BlueprintTemplate)target;
        }

        #endregion Methods
    }
}