using EasyBuildSystem.Features.Scripts.Core;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBuildSystem.Features.Scripts.Editor.Inspector.Socket
{
    [CustomEditor(typeof(SocketBehaviour))]
    [CanEditMultipleObjects]
    public class SocketBehaviourInspector : UnityEditor.Editor
    {
        #region Enums

        public enum EditorHandleType
        {
            None,
            Position,
            Rotation,
            Scale
        }

        #endregion Enums

        #region Fields

        private SocketBehaviour Target;
        private Offset CurrentOffset;
        private GameObject PreviewPiece;

        private static bool GeneralFoldout;
        private static bool OffsetsFoldout;
        private static bool ConditionsFoldout;
        private static bool AddonsFoldout;
        private static bool DebuggingFoldout;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (SocketBehaviour)target;

            AddonInspector.LoadAddons(Target, AddonTarget.SocketBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.SocketBehaviour);
        }

        private void OnDisable()
        {
            ClearPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Socket Behaviour General

            InspectorStyles.DrawSectionLabel("Socket Behaviour - General");

            GeneralFoldout = EditorGUILayout.Foldout(GeneralFoldout, "General Settings", true);

            if (GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"), new GUIContent("Socket Radius :", "Define the socket radius point.\nYou can decrease the socket radius to improve the precision during the detection."));
            }

            #endregion Socket Behaviour Settings

            #region Socket Behaviour Offsets

            InspectorStyles.DrawSectionLabel("Socket Behaviour - Offsets");

            OffsetsFoldout = EditorGUILayout.Foldout(OffsetsFoldout, "Offsets Settings", true);

            if (OffsetsFoldout)
            {
                if (serializedObject.FindProperty("PartOffsets").arraySize == 0)
                {
                    GUI.color = Color.black / 4;
                    GUILayout.BeginVertical("helpBox");
                    GUI.color = Color.white;
                    GUILayout.Label("Offset list does not contains any transform child(s).");
                    GUILayout.EndVertical();
                }
                else
                {
                    int Index = 0;

                    foreach (Offset Offset in Target.PartOffsets.ToList())
                    {
                        if (Offset == null)
                        {
                            return;
                        }

                        if (CurrentOffset == Offset)
                            GUI.color = Color.yellow;
                        else
                            GUI.color = Color.white;

                        GUI.color = Color.black / 4;
                        GUILayout.BeginVertical("helpBox");
                        GUI.color = Color.white;

                        GUI.color = Color.white;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Piece"), new GUIContent("Offset Piece :", "Define the specific piece which can be snap on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Position"), new GUIContent("Offset Piece Position :", "Define the type of this piece."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Rotation"), new GUIContent("Offset Piece Rotation :", "This allows to set the rotation of piece that will snapped on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Scale"), new GUIContent("Offset Piece Scale :", "Define the specific scale of the piece that will be snapped on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("AllowSameCategory"), new GUIContent("Offset Piece Same Category :", "If checked, the pieces of the same category can will be snapped"));

                        if (PreviewPiece != null && int.Parse(PreviewPiece.name) == Offset.Piece.Id)
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Cancel Preview"))
                            {
                                ClearPreview();
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {
                            if (GUILayout.Button("Show Preview"))
                            {
                                ClearPreview();
                                CurrentOffset = Offset;
                                CreatePreview(Offset);
                            }
                        }

                        if (GUILayout.Button("Remove Offset"))
                        {
                            Undo.RecordObject(target, "Cancel remove offset");

                            Target.PartOffsets.Remove(Offset);

                            ClearPreview();

                            EditorUtility.SetDirty(target);

                            return;
                        }

                        GUILayout.EndVertical();

                        Index++;
                    }
                }

                GUI.color = Color.black / 4;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your pieces here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

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

                            if (!PrefabUtility.IsPartOfPrefabAsset(DraggedObject))
                            {
                                DraggedObject = PrefabUtility.GetCorrespondingObjectFromSource(DraggedObject);

                                if (DraggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> : Object have not PieceBehaviour component or the prefab is not the original.");
                                    return;
                                }
                            }

                            PieceBehaviour DraggedPiece = DraggedObject.GetComponent<PieceBehaviour>();

                            if (DraggedPiece == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : You object have not PieceBehaviour component!");
                                return;
                            }

                            if (Target.PartOffsets.Find(entry => entry.Piece.Id == DraggedPiece.Id) == null)
                            {
                                ClearPreview();
                                Offset Offset = new Offset(DraggedPiece);
                                Target.PartOffsets.Insert(Target.PartOffsets.Count, Offset);
                                Target.PartOffsets = Target.PartOffsets.OrderBy(x => i).ToList();
                                CurrentOffset = Offset;
                                CreatePreview(Offset);
                                Repaint();
                                EditorUtility.SetDirty(target);
                            }
                            else
                            {
                                Debug.LogError("<b>Easy Build System</b> : This piece is already exists in the list.");
                            }
                        }

                        UnityEngine.Event.current.Use();
                    }
                }

                GUILayout.EndVertical();
            }

            #endregion Socket Behaviour Offsets

            #region Socket Behaviour Conditions

            InspectorStyles.DrawSectionLabel("Socket Behaviour - Conditions");

            ConditionsFoldout = EditorGUILayout.Foldout(ConditionsFoldout, "Conditions Settings", true);

            if (ConditionsFoldout)
            {
                ConditionInspector.DrawConditions(Target, ConditionTarget.SocketBehaviour);
            }

            #endregion

            #region Socket Behaviour Add-Ons

            InspectorStyles.DrawSectionLabel("Socket Behaviour - Addons");

            AddonsFoldout = EditorGUILayout.Foldout(AddonsFoldout, "Addons Settings", true);

            if (AddonsFoldout)
            {
                AddonInspector.DrawAddons(Target, AddonTarget.SocketBehaviour);
            }

            #endregion Socket Behaviour Add-Ons

            #region Socket Behaviour Debugging

            InspectorStyles.DrawSectionLabel("Socket Behaviour - Debugging");

            DebuggingFoldout = EditorGUILayout.Foldout(DebuggingFoldout, "Debugging Settings", true);

            if (DebuggingFoldout)
            {
                SocketBehaviour.ShowGizmos = EditorGUILayout.Toggle("Socket Show Gizmos", SocketBehaviour.ShowGizmos);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDisabled"), new GUIContent("Socket Is Disabled :", "Socket is disabled?, when disable all the raycast will ignore this."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ParentPiece"), new GUIContent("Socket Parent Piece :", "Parent piece if the socket has a parent with the Piece Behaviour component."));
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BusySpaces"), new GUIContent("Socket Busy Spaces :", "Find here all the busy spaces of the socket, if a piece is here it's that you cannot place another same piece."), true);
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            GUI.color = Color.white;

            #endregion Socket Debugging Settings

            serializedObject.ApplyModifiedProperties();

            if (CurrentOffset != null)
            {
                if (PreviewPiece != null)
                {
                    PreviewPiece.transform.position = Target.transform.TransformPoint(CurrentOffset.Position);
                    PreviewPiece.transform.rotation = Target.transform.rotation * Quaternion.Euler(CurrentOffset.Rotation);

                    if (CurrentOffset.Scale != Vector3.one)
                    {
                        PreviewPiece.transform.localScale = CurrentOffset.Scale;
                    }
                    else
                        PreviewPiece.transform.localScale = Target.transform.parent != null ? Target.transform.parent.localScale : Target.transform.localScale;
                }
            }
        }

        private void CreatePreview(Offset offsetPiece)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (PreviewPiece == null)
            {
                PieceBehaviour PieceInstance = offsetPiece.Piece;

                if (PieceInstance == null) return;

                PreviewPiece = Instantiate(PieceInstance.gameObject, Target.transform);

                PreviewPiece.transform.position = Target.transform.TransformPoint(offsetPiece.Position);
                PreviewPiece.transform.rotation = Target.transform.rotation * Quaternion.Euler(offsetPiece.Rotation);

                if (offsetPiece.Scale != Vector3.one)
                {
                    PreviewPiece.transform.localScale = offsetPiece.Scale;
                }
                else
                    PreviewPiece.transform.localScale = Target.transform.parent != null ? Target.transform.parent.localScale : Target.transform.localScale;

                PreviewPiece.name = PieceInstance.Id.ToString();

                DestroyImmediate(PreviewPiece.GetComponent<PieceBehaviour>());

                foreach (SocketBehaviour Socket in PreviewPiece.GetComponentsInChildren<SocketBehaviour>())
                {
                    DestroyImmediate(Socket);
                }

                Material PreviewMaterial = new Material(Resources.Load<Material>("Materials/Default Transparent"));

                if (GraphicsSettings.currentRenderPipeline)
                {
                    if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                        PreviewMaterial = Resources.Load<Material>("Materials/HDRP Default Transparent");
                    else
                        PreviewMaterial = Resources.Load<Material>("Materials/URP Default Transparent");
                }

                PreviewMaterial.SetColor("_BaseColor", new Color(0, 1f, 1f, 0.5f));

                PreviewPiece.ChangeAllMaterialsInChildren(PreviewPiece.GetComponentsInChildren<MeshRenderer>(), PreviewMaterial);

                SceneView.FrameLastActiveSceneView();
            }
        }

        private void ClearPreview()
        {
            if (PreviewPiece != null)
            {
                DestroyImmediate(PreviewPiece);
                PreviewPiece = null;
                CurrentOffset = null;
            }
        }

        #endregion Methods
    }
}