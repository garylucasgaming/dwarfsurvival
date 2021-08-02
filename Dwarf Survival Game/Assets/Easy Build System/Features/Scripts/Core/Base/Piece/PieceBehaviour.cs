using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Piece
{
    [AddComponentMenu("Easy Build System/Features/Buildings Behaviour/Piece Behaviour")]
    public class PieceBehaviour : MonoBehaviour
    {
        #region Fields

        public int Id;
        public Sprite Icon;
        public string Name = "New Part";
        public string Description = "";
        public string Category = "Default";
        public bool IsEditable = true;
        public bool IsDestructible = true;

        public bool RequireSocket;
        public bool IgnoreSocket;

        public bool KeepToHeight;
        public float KeepHeight = 0f;
        public bool UseGroundUpper;
        public float GroundUpperHeight = 1f;

        public bool RotateOnSockets = true;
        public bool RotateAccordingSlope;
        public Vector3 RotationAxis = Vector3.up * 90;

        public bool PreviewClampPosition = false;
        public Vector3 PreviewClampMinPosition;
        public Vector3 PreviewClampMaxPosition;

        public Vector3 PreviewOffset = new Vector3(0, 0, 0);
        public GameObject[] PreviewDisableObjects;
        public MonoBehaviour[] PreviewDisableBehaviours;
        public Collider[] PreviewDisableColliders;

        public bool PreviewUseColorLerpTime = false;
        public float PreviewColorLerpTime = 15.0f;

        public bool UseCustomPreviewMaterial = false;
        public Material CustomPreviewMaterial;
        public Material PreviewMaterial { get; set; }

        public Color PreviewAllowedColor = new Color(0.0f, 1.0f, 0, 0.5f);
        public Color PreviewDeniedColor = new Color(1.0f, 0, 0, 0.5f);

        public List<ConditionBehaviour> Conditions = new List<ConditionBehaviour>();

        public List<GameObject> Appearances = new List<GameObject>();
        public int AppearanceIndex = 0;

        public Bounds MeshBounds;
        public Bounds MeshBoundsToWorld { get { return transform.ConvertBoundsToWorld(MeshBounds); } }

        public StateType CurrentState = StateType.Placed;
        public StateType LastState { get; set; }

        public bool HasGroup => (GetComponentInParent<GroupBehaviour>() != null);

        public SocketBehaviour[] Sockets;

        public List<PieceBehaviour> LinkedPieces = new List<PieceBehaviour>();

        public Dictionary<Renderer, Material[]> InitialRenderers = new Dictionary<Renderer, Material[]>();
        public List<Collider> Colliders;
        public List<Renderer> Renderers;

        public List<string> ExtraProperties = new List<string>();

        public int EntityInstanceId { get; set; }

        private Material DefaultPreviewMaterial;

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void Awake()
        {
            Conditions.AddRange(GetComponents<ConditionBehaviour>());

            Sockets = GetComponentsInChildren<SocketBehaviour>();

            Renderers = GetComponentsInChildren<Renderer>(true).ToList();

            for (int i = 0; i < Renderers.Count; i++)
                InitialRenderers.Add(Renderers[i], Renderers[i].sharedMaterials);

            Colliders = GetComponentsInChildren<Collider>(true).ToList();

            for (int i = 0; i < Colliders.Count; i++)
                if (Colliders[i] != Colliders[i])
                    Physics.IgnoreCollision(Colliders[i], Colliders[i]);

            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    DefaultPreviewMaterial = Resources.Load<Material>("Materials/HDRP Default Transparent");
                else
                    DefaultPreviewMaterial = Resources.Load<Material>("Materials/URP Default Transparent");
            }
            else
            {
                DefaultPreviewMaterial = new Material(Resources.Load<Material>("Materials/Default Transparent"));
            }

            if (!UseCustomPreviewMaterial)
            {
                PreviewMaterial = new Material(DefaultPreviewMaterial);
            }
            else
            {
                PreviewMaterial = new Material(CustomPreviewMaterial);
            }
        }

        private void Start()
        {
            if (CurrentState != StateType.Preview)
            {
                BuildManager.Instance.AddPiece(this);
                UpdateOccupancy(true);
            }
        }

        private void Reset()
        {
            if (MeshBounds.size == Vector3.zero)
                MeshBounds = gameObject.GetChildsBounds();
        }

        private void Update()
        {
            bool Placed = CurrentState == StateType.Placed;

            if (!Placed)
            {
                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(Placed);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = Placed;
                    }
                }

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = Placed;
                    }
                }

                return;
            }

            for (int i = 0; i < Appearances.Count; i++)
            {
                if (Appearances[i] == null)
                {
                    return;
                }

                if (i == AppearanceIndex)
                {
                    Appearances[i].SetActive(true);
                }
                else
                {
                    Appearances[i].SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (CurrentState == StateType.Preview)
            {
                return;
            }

            UpdateOccupancy(false);

            BuildEvent.Instance.OnPieceDestroyed.Invoke(this);

            BuildManager.Instance.RemovePiece(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            for (int i = 0; i < LinkedPieces.Count; i++)
            {
                if (LinkedPieces[i] != null)
                {
                    Gizmos.DrawLine(transform.position, LinkedPieces[i].transform.position);
                }
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
            Gizmos.color = Color.cyan / 2;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(MeshBounds.center, MeshBounds.size * 1.001f);
        }

        /// <summary>
        /// This method allows to change the piece state (Queue, Preview, Edit, Remove, Placed).
        /// </summary>
        public void ChangeState(StateType state)
        {
            if (BuilderBehaviour.Instance == null)
            {
                return;
            }

            if (CurrentState == state)
            {
                return;
            }

            LastState = CurrentState;

            if (state == StateType.Queue)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(), Color.clear);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }
            else if (state == StateType.Preview)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(),
                    BuilderBehaviour.Instance.AllowPlacement ? PreviewAllowedColor : PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                DisableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].DisableSocketCollider();
                    Sockets[i].gameObject.SetActive(false);
                }
            }
            else if (state == StateType.Edit)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(),
                    BuilderBehaviour.Instance.AllowEdition ? PreviewAllowedColor : PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }
            else if (state == StateType.Remove)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(), PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].DisableSocketCollider();
                    Sockets[i].gameObject.SetActive(false);
                }
            }
            else if (state == StateType.Placed)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), InitialRenderers);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(true);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = true;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = true;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }

            CurrentState = state;

            BuildEvent.Instance.OnPieceChangedState.Invoke(this, state);
        }

        /// <summary>
        /// This method allows to change the state of the sockets that collide the mesh bounds to avoid multiple placement.
        /// </summary>
        public void UpdateOccupancy(bool busy)
        {
            SocketBehaviour[] Sockets = PhysicExtension.GetNeighborsTypeByBox<SocketBehaviour>(MeshBoundsToWorld.center, MeshBoundsToWorld.extents,
                transform.rotation, LayerMask.GetMask(Constants.LAYER_SOCKET));

            for (int i = 0; i < Sockets.Length; i++)
            {
                if (Sockets[i] != null)
                {
                    if (Sockets[i].ParentPiece != this)
                    {
                        if (Sockets[i].AllowPiece(this))
                        {
                            if (Sockets[i].ParentPiece != null)
                            {
                                LinkPart(Sockets[i].ParentPiece);
                                Sockets[i].ParentPiece.LinkPart(this);
                            }

                            Sockets[i].ChangeOccupancy(busy, this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method allows to enable all the colliders of this piece.
        /// </summary>
        public void EnableAllColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].enabled = true;
            }
        }

        /// <summary>
        /// This method allows to disable all the colliders of this piece.
        /// </summary>
        public void DisableAllColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].enabled = false;
            }
        }

        /// <summary>
        /// This method allows to enable all the colliders of this piece.
        /// </summary>
        public void EnableAllCollidersTrigger()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].isTrigger = true;
            }
        }

        /// <summary>
        /// This method allows to disable all the colliders of this piece.
        /// </summary>
        public void DisableAllCollidersTrigger()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].isTrigger = false;
            }
        }

        /// <summary>
        /// This method allows check all the external condition(s) before placement.
        /// </summary>
        public bool CheckExternalPlacementConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForPlacement())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allows check all the external condition(s) before destruction.
        /// </summary>
        public bool CheckExternalDestructionConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForDestruction())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allow check all the external condition(s) before edition.
        /// </summary>
        public bool CheckExternalEditionConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForEdition())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allows to link a piece at this piece.
        /// </summary>
        public void LinkPart(PieceBehaviour piece)
        {
            if (!LinkedPieces.Contains(piece))
            {
                LinkedPieces.Add(piece);
            }
        }

        /// <summary>
        /// This method allows to unlink a piece at this piece.
        /// </summary>
        public void UnlinkPart(PieceBehaviour piece)
        {
            LinkedPieces.Remove(piece);
        }

        /// <summary>
        /// This method allows to change the piece appearance.
        /// </summary>
        public void ChangeSkin(int skinIndex)
        {
            if (AppearanceIndex == skinIndex)
            {
                return;
            }

            if (Appearances.Count < skinIndex)
            {
                return;
            }

            AppearanceIndex = skinIndex;

            if (BuildEvent.Instance != null)
                BuildEvent.Instance.OnPieceChangedApperance.Invoke(this, skinIndex);
        }

        #endregion Methods
    }
}