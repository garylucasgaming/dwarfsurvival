using EasyBuildSystem.Features.Scripts.Core;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Editor.Helper;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Piece
{
    [CustomEditor(typeof(PieceBehaviour))]
    [CanEditMultipleObjects]
    public class PieceBehaviourInspector : UnityEditor.Editor
    {
        #region Fields

        public static Rect BoundsEditingWindowRect = new Rect(50, 50, 300, 145);

        private PieceBehaviour Target;

        private static bool[] AppearancesFolds = new bool[999];
        private readonly List<UnityEditor.Editor> AppearancePreviews = new List<UnityEditor.Editor>();
        private readonly List<UnityEditor.Editor> CachedEditors = new List<UnityEditor.Editor>();
        private List<GameObject> Previews = new List<GameObject>();

        private static bool GeneralFoldout;
        private static bool PreviewFoldout;
        private static bool BoundsFoldout;
        private static bool ConditionFoldout;
        private static bool AppearancesFoldout;
        private static bool AddonsFoldout;
        private static bool DebuggingFoldout;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (PieceBehaviour)target;

            AddonInspector.LoadAddons(Target, AddonTarget.PieceBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.PieceBehaviour);
        }

        private void OnSceneGUI()
        {
            if (SceneView.lastActiveSceneView.camera == null)
            {
                return;
            }

            if (Target.UseGroundUpper)
            {
                Handles.color = Color.green;
                Handles.DrawLine(Target.transform.position, Target.transform.position + Vector3.down * Target.GroundUpperHeight);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region General

            InspectorStyles.DrawSectionLabel("Piece Behaviour - General");

            GeneralFoldout = EditorGUILayout.Foldout(GeneralFoldout, "General Settings", true);

            if (GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"), new GUIContent("Piece Id :", "Define id of this piece."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"), new GUIContent("Piece Name :", "Define the name of this piece."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"), new GUIContent("Piece Description :", "Define the description of this piece."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"), new GUIContent("Piece Category :", "Define the category of this piece."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"), new GUIContent("Piece Icon :", "Define the icon of this piece."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsEditable"), new GUIContent("Piece Editable :", "Define if this piece can be edited via the edition mode."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDestructible"), new GUIContent("Piece Destructible :", "Define if this piece can be destroyed via the destruction mode."));
            }

            #endregion

            #region Preview

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Preview");

            PreviewFoldout = EditorGUILayout.Foldout(PreviewFoldout, "Preview Settings", true);

            if (PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireSocket"), new GUIContent("Piece Preview Require Sockets :", "Define if the piece require a socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("IgnoreSocket"), new GUIContent("Piece Preview Ignore Sockets :", "Define if the piece ignore the socket(s)."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("KeepToHeight"), new GUIContent("Piece Preview Keep To Height :", "Define if the piece keep to ground or can be moved on the Y axis."));

                if (serializedObject.FindProperty("KeepToHeight").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("KeepHeight"), new GUIContent("Piece Preview Keep Height :", "Define if the piece keep to ground or can be moved on the Y axis."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseGroundUpper"), new GUIContent("Piece Preview Ground Upper :", "This allows to to raise from ground the preview on the Y axis."));

                if (serializedObject.FindProperty("UseGroundUpper").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundUpperHeight"), new GUIContent("Piece Preview Ground Upper Height :", "Define the maximum limit not to exceed on the Y axis."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateOnSockets"), new GUIContent("Piece Preview Can Rotate On Socket :", "Define if the preview can be rotated on socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateAccordingSlope"), new GUIContent("Piece Preview Rotate According Slope :", "This allows to rotate the preview according to collider slope."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotationAxis"), new GUIContent("Piece Preview Rotate Axis :", "Define on what axis the preview will be rotated."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewOffset"), new GUIContent("Piece Preview Position Offset :", "Define the preview offset position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampPosition"), new GUIContent("Piece Preview Clamp Position :", "Define if the preview movement can be clamp."));

                if (serializedObject.FindProperty("PreviewClampPosition").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampMinPosition"), new GUIContent("Piece Preview Clamp Min Position :", "Define the preview clamp min position."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampMaxPosition"), new GUIContent("Piece Preview Clamp Max Position :", "Define the preview clamp max position."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewUseColorLerpTime"), new GUIContent("Piece Preview Color Lerp Time :", "This allows to lerp the preview color when the placement is possible or no."));

                if (serializedObject.FindProperty("PreviewUseColorLerpTime").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewColorLerpTime"), new GUIContent("Piece Preview Color Lerp Time :", "Define the transition speed on the preview color."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseCustomPreviewMaterial"), new GUIContent("Piece Preview Custom Material :", "This allows to use a custom material for the preview."));

                if (serializedObject.FindProperty("UseCustomPreviewMaterial").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomPreviewMaterial"), new GUIContent("Piece Preview Custom Material :", "Define here the custom material."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewAllowedColor"), new GUIContent("Piece Preview Allowed Color :", "This allows to show the allowed color when the preview can be placed."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDeniedColor"), new GUIContent("Piece Preview Denied Color :", "This allows to show the denied color when the preview can't be placed."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableObjects"), new GUIContent("Disable Object(s) In Preview State :", "This allows to disable some object(s) when the piece is in preview state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableBehaviours"), new GUIContent("Disable Mono Behaviour(s) In Preview State :", "This allows to disable some monobehaviour(s) when the piece is in preview/queue/remove/edit state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableColliders"), new GUIContent("Disable Collider(s) In Preview State :", "This allows to disable some collider(s) when the piece is in preview state."), true);
                GUILayout.EndHorizontal();
            }

            #endregion

            #region Skins

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Skins");

            AppearancesFoldout = EditorGUILayout.Foldout(AppearancesFoldout, "Skins Settings", true);

            if (AppearancesFoldout)
            {
                bool Flag = false;

                if (AppearancesFolds == null)
                {
                    AppearancesFolds = new bool[serializedObject.FindProperty("Appearances").arraySize];
                }

                for (int i = 0; i < serializedObject.FindProperty("Appearances").arraySize; i++)
                {
                    if (Target.Appearances[i] == null)
                    {
                        Flag = true;
                    }
                }

                if (Flag)
                {
                    Target.Appearances.Clear();
                }

                int Index = 0;

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                if (serializedObject.FindProperty("Appearances").arraySize == 0)
                {
                    GUILayout.Label("Skins list does not contains any transform child(s).");
                }
                else
                {
                    foreach (GameObject Appearance in Target.Appearances)
                    {
                        if (Appearance == null)
                        {
                            return;
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(13);

                        EditorGUI.BeginChangeCheck();

                        string Format = string.Format("[{0}] ", Index) + Appearance.name;

                        GUILayout.BeginHorizontal();

                        AppearancesFolds[Index] = EditorGUILayout.Foldout(AppearancesFolds[Index], Format, true);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        {
                            Undo.RecordObject(target, "Remove Appearance");
                            Target.Appearances.Remove(Appearance);
                            Repaint();
                            EditorUtility.SetDirty(target);
                            AppearancePreviews.Clear();
                            break;
                        }

                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (AppearancesFolds[Index] == true)
                            {
                                for (int i = 0; i < AppearancesFolds.Length; i++)
                                {
                                    if (i != Index)
                                    {
                                        AppearancesFolds[i] = false;
                                    }
                                }

                                for (int x = 0; x < Target.Appearances.Count; x++)
                                {
                                    if (x == Index)
                                    {
                                        Target.Appearances[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Appearances[x].SetActive(false);
                                    }
                                }
                            }
                            else
                            {
                                for (int x = 0; x < Target.Appearances.Count; x++)
                                {
                                    if (x == Target.AppearanceIndex)
                                    {
                                        Target.Appearances[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Appearances[x].SetActive(false);
                                    }
                                }
                            }

                            SceneHelper.Focus(Appearance, false);

                            AppearancePreviews.Clear();
                        }

                        if (Target.AppearanceIndex == Index)
                        {
                            GUI.enabled = false;
                        }

                        if (GUILayout.Button("Define As Default"))
                        {
                            for (int i = 0; i < AppearancesFolds.Length; i++)
                            {
                                AppearancesFolds[i] = false;
                            }

                            Target.ChangeSkin(Index);

                            for (int x = 0; x < Target.Appearances.Count; x++)
                            {
                                if (x == Target.AppearanceIndex)
                                {
                                    Target.Appearances[x].SetActive(true);
                                }
                                else
                                {
                                    Target.Appearances[x].SetActive(false);
                                }
                            }

                            Repaint();

                            EditorUtility.SetDirty(target);
                        }

                        GUI.enabled = true;

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();

                        if (AppearancesFolds[Index])
                        {
                            GUILayout.BeginHorizontal();

                            GUI.color = Color.black / 4f;
                            GUILayout.BeginVertical("helpBox");
                            GUI.color = Color.white;

                            if (Appearance != null)
                            {
                                GUILayout.BeginHorizontal();

                                UnityEditor.Editor PreviewEditor = null;

                                if (AppearancePreviews.Count > Index)
                                {
                                    PreviewEditor = AppearancePreviews[Index];
                                }

                                if (PreviewEditor == null)
                                {
                                    PreviewEditor = CreateEditor(Appearance);

                                    AppearancePreviews.Add(PreviewEditor);

                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);

                                    CachedEditors.Add(PreviewEditor);
                                }
                                else
                                {
                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);
                                }

                                GUILayout.EndHorizontal();

                                EditorGUILayout.ObjectField(serializedObject.FindProperty("Appearances").GetArrayElementAtIndex(Index), new GUIContent("Child Transform :"));
                            }

                            GUILayout.FlexibleSpace();

                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndHorizontal();

                        Index++;
                    }
                }

                EditorGUILayout.EndVertical();

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your transform children(s) here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

                if (DropRect.Contains(UnityEngine.Event.current.mousePosition))
                {
                    if (UnityEngine.Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        UnityEngine.Event.current.Use();
                    }
                    else if (UnityEngine.Event.current.type == EventType.DragPerform)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            GameObject DraggedObject = DragAndDrop.objectReferences[i] as GameObject;

                            if (DraggedObject == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Cannot add empty child!");
                                return;
                            }

                            if (!DraggedObject.transform.IsChildOf(Target.transform))
                            {
                                Debug.LogError("<b>Easy Build System</b> : This child does not exist in this transform!");
                                return;
                            }

                            if (Target.Appearances.Contains(DraggedObject) == false)
                            {
                                Target.Appearances.Add(DraggedObject);

                                for (int x = 0; x < Target.Appearances.Count; x++)
                                {
                                    if (x == Target.AppearanceIndex)
                                    {
                                        Target.Appearances[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Appearances[x].SetActive(false);
                                    }
                                }

                                EditorUtility.SetDirty(target);

                                Repaint();
                            }
                            else
                            {
                                Debug.LogError("<b>Easy Build System</b> : This child already exists in the list!");
                            }
                        }
                        UnityEngine.Event.current.Use();
                    }
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Bounds

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Bounds");

            BoundsFoldout = EditorGUILayout.Foldout(BoundsFoldout, "Bounds Settings", true);

            if (BoundsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MeshBounds"), new GUIContent("Piece Mesh Bounds :", "Define id of this piece."));

                if (GUILayout.Button("Generate Bounds"))
                {
                    Undo.RecordObject(target, "Cancel new bounds generation");
                    Target.MeshBounds = Target.gameObject.GetChildsBounds();
                    EditorUtility.SetDirty(target);
                }
            }

            #endregion Bounds

            #region Conditions

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Conditions");

            ConditionFoldout = EditorGUILayout.Foldout(ConditionFoldout, "Conditions Settings", true);

            if (ConditionFoldout)
            {
                ConditionInspector.DrawConditions(Target, ConditionTarget.PieceBehaviour);
            }

            #endregion

            #region Addons

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Addons");

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Addons Settings", true);

            if (AddonsFoldout)
            {
                AddonInspector.DrawAddons(Target, AddonTarget.PieceBehaviour);
            }

            #endregion

            #region Debugging

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Debugging");

            DebuggingFoldout = EditorGUILayout.Foldout(DebuggingFoldout, "Debugging Settings", true);

            if (DebuggingFoldout)
            {
                PieceBehaviour.ShowGizmos = EditorGUILayout.Toggle("Piece Show Gizmos", PieceBehaviour.ShowGizmos);
                GUI.enabled = false;
                EditorGUILayout.Toggle("Piece Has Group :", (Target.GetComponentInParent<Core.Base.Group.GroupBehaviour>() != null));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentState"), new GUIContent("Piece Current State", "Current state of the piece."));
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LinkedPieces"), new GUIContent("Piece Linked Pieces :", "All the linked pieces for the collapsing physics logic."), true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Sockets"), new GUIContent("Piece Sockets :", "All the sockets that the piece contains"), true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Colliders"), new GUIContent("Piece Colliders :", "All collider(s) of the piece."), true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Renderers"), new GUIContent("Piece Renderers :", "All renderer(s) of the piece."), true);
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }
}