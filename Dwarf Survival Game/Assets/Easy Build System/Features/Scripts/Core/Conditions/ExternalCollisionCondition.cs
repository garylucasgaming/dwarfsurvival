using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Linq;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Conditions
{
    [Condition("External Collision Condition", "Allow the placement if the piece does not collide with another colliders.\n" +
        "You can find more information about this component in the documentation.", ConditionTarget.PieceBehaviour)]
    public class ExternalCollisionCondition : ConditionBehaviour
    {
        #region Fields

        public LayerMask CollisionLayer = 1 << 0;
        [Range(0f, 10f)]
        public float CollisionClippingTolerance = 1f;
        [Range(0f, 10f)]
        public float CollisionClippingSnappingTolerance = 0.99f;
        public string[] CollisionIgnoreCategory;
        public bool RequireBuildableSurface;
        public bool CollisionIgnoreWhenSnap;

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            if (Piece == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.cyan / 2f;
            Gizmos.DrawCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingTolerance * 1.001f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingTolerance * 1.001f);

            Gizmos.color = Color.yellow / 2f;
            Gizmos.DrawCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingSnappingTolerance * 1.001f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingSnappingTolerance * 1.001f);
        }

        public override bool CheckForPlacement()
        {
            bool HasBuildableSurface = false;
            bool CanBePlaced = true;

            Collider[] Colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.MeshBoundsToWorld.center,
                Piece.MeshBoundsToWorld.extents * CollisionClippingTolerance, transform.rotation, CollisionLayer).Where(x => !x.isTrigger).ToArray();

            if (RequireBuildableSurface)
            {
                for (int i = 0; i < Colliders.Length; i++)
                {
                    if (Colliders[i] != null)
                        if (BuildManager.Instance.IsBuildableSurface(Colliders[i]))
                            HasBuildableSurface = true;
                }

                if (!HasBuildableSurface)
                    return false;
            }
            else
                HasBuildableSurface = true;

            if (HasBuildableSurface && !CollisionIgnoreWhenSnap)
            {
                for (int i = 0; i < Colliders.Length; i++)
                {
                    if (Colliders[i] != null)
                    {
                        if (RequireBuildableSurface)
                        {
                            if (Colliders[i].GetComponentInParent<PieceBehaviour>() == null && !BuildManager.Instance.IsBuildableSurface(Colliders[i]))
                                CanBePlaced = false;
                        }
                        else
                        {
                            if (Colliders[i].GetComponentInParent<PieceBehaviour>() == null)
                                CanBePlaced = false;
                        }
                    }
                }
            }

            if (CanBePlaced && !CollisionIgnoreWhenSnap)
            {
                Colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.MeshBoundsToWorld.center,
                   Piece.MeshBoundsToWorld.extents * CollisionClippingSnappingTolerance, transform.rotation, CollisionLayer).Where(x => !x.isTrigger).ToArray();

                for (int i = 0; i < Colliders.Length; i++)
                {
                    if (Colliders[i] != null)
                    {
                        if (Colliders[i].GetComponentInParent<PieceBehaviour>() != null)
                        {
                            if (!CollisionIgnoreCategory.Contains(Colliders[i].GetComponentInParent<PieceBehaviour>().Category))
                            {
                                CanBePlaced = false;
                            }
                        }
                    }
                }
            }

            return HasBuildableSurface && CanBePlaced;
        }

        #endregion
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(ExternalCollisionCondition), true)]
    public class ExternalColliderConditionInspector : UnityEditor.Editor
    {
        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ExternalCollisionCondition.ShowGizmos = UnityEditor.EditorGUILayout.Toggle("Collision Show Gizmos :", ExternalCollisionCondition.ShowGizmos);

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionLayer"), new GUIContent("Collision Layer(s) :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionClippingTolerance"), new GUIContent("Collision Tolerance :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionClippingSnappingTolerance"), new GUIContent("Collision Snapping Tolerance :"));

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireBuildableSurface"), new GUIContent("Collision Require Buildable Surface :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionIgnoreWhenSnap"), new GUIContent("Collision Ignore When Snapped :"));

            GUILayout.BeginHorizontal();
            GUILayout.Space(13);
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionIgnoreCategory"), new GUIContent("Collision Ignore Categories :"));
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

#endif
}