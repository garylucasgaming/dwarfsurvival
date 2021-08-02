using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Extensions;
using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Builder
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(999)]
    [AddComponentMenu("Easy Build System/Features/Builders Behaviour/Builder Behaviour")]
    public class BuilderBehaviour : MonoBehaviour
    {
        #region Fields

        public static BuilderBehaviour Instance;

        public float ActionDistance = 10f;
        public float SnapThreshold = 5f;
        public float OutOfRangeDistance = 0f;

        public float OverlapAngles = 35f;
        public bool LockRotation;
        public DetectionType RayDetection = DetectionType.Overlap;
#if ENABLE_INPUT_SYSTEM
        public XRRayInteractor RayInteractor;
#endif
        public RayType CameraType;
        public Vector3 RaycastOffset = new Vector3(0, 0, 1);
        public Transform RaycastOriginTransform;
        public Transform RaycastAnchorTransform;

        public MovementType PreviewMovementType;
        public bool PreviewMovementOnlyAllowed;
        public float PreviewGridSize = 1.0f;
        public float PreviewGridOffset;
        public float PreviewSmoothTime = 5.0f;

        public bool UsePlacementMode = true;
        public bool ResetModeAfterPlacement = false;
        public bool RequireAreaForPlacement = false;
        public bool UseDestructionMode = true;
        public bool ResetModeAfterDestruction = false;
        public bool RequireAreaForDestruction = false;
        public bool UseEditionMode = true;
        public bool ResetModeAfterEdition = false;
        public bool RequireAreaForEdition = false;

        public AudioSource Source;
        public AudioClip[] PlacementClips;
        public AudioClip[] DestructionClips;
        public AudioClip[] EditionClips;

        public virtual Ray GetRay
        {
            get
            {
                if (CameraType == RayType.TopDown)
                {
#if ENABLE_LEGACY_INPUT_MANAGER
                    return BuilderCamera.ScreenPointToRay(Input.mousePosition + RaycastOffset); 
#else
                    return BuilderCamera.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f) + RaycastOffset);
#endif
                }
                else if (CameraType == RayType.FirstPerson)
                {
                    return new Ray(BuilderCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f) + RaycastOffset), BuilderCamera.transform.forward);
                }
                else if (CameraType == RayType.ThirdPerson)
                {
                    if (RaycastOriginTransform != null && BuilderCamera != null)
                    {
                        return new Ray(RaycastOriginTransform.position + RaycastOriginTransform.TransformDirection(RaycastOffset), BuilderCamera.transform.forward);
                    }
                }
#if ENABLE_INPUT_SYSTEM
                else if (CameraType == RayType.VirtualRealityPerson)
                {
                    return new Ray(RayInteractor.transform.position, RayInteractor.transform.forward);      
                }
#endif
                return new Ray();
            }
        }

        private Transform _Caster;
        public virtual Transform GetCaster
        {
            get
            {
                if (_Caster == null)
                {
                    if (CameraType == RayType.TopDown)
                    {
                        _Caster = RaycastAnchorTransform != null ? RaycastAnchorTransform : transform;
                    }
#if ENABLE_INPUT_SYSTEM
                    else if (CameraType == RayType.VirtualRealityPerson)
                    {
                        _Caster = RayInteractor.transform;
                    }
#endif
                    else
                    {
                        _Caster = transform;
                    }
                }

                return _Caster;
            }
        }

        public BuildMode CurrentMode { get; set; }
        public BuildMode LastMode { get; set; }

        public PieceBehaviour SelectedPrefab { get; set; }

        public PieceBehaviour CurrentPreview { get; set; }
        public PieceBehaviour CurrentEditionPreview { get; set; }
        public PieceBehaviour CurrentRemovePreview { get; set; }

        public Vector3 CurrentRotationOffset { get; set; }

        public Vector3 InitialScale { get; set; }

        public bool AllowPlacement { get; set; }
        public bool AllowDestruction { get; set; }
        public bool AllowEdition { get; set; }

        public bool HasSocket { get; set; }

        public SocketBehaviour CurrentSocket { get; set; }
        public SocketBehaviour LastSocket { get; set; }

        private Camera BuilderCamera;

        private Vector3 LastAllowedPoint;

        private RaycastHit TopDownHit;

        private readonly RaycastHit[] Hits = new RaycastHit[PhysicExtension.MAX_ALLOC_COUNT];

        #endregion Fields

        #region Methods

        public virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BuilderCamera = GetComponent<Camera>();

            if (BuilderCamera == null)
            {
                Debug.LogWarning("<b>Easy Build System</b> : The Builder Behaviour require a camera!");
            }
        }

        public virtual void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateModes();
        }

        #region Placement

        /// <summary>
        /// This method allows to update the placement preview.
        /// </summary>
        public void UpdatePreview()
        {
            HasSocket = false;

            if (CameraType == RayType.TopDown)
            {
                Physics.Raycast(GetRay, out TopDownHit, Mathf.Infinity, LayerMask.GetMask(Constants.LAYER_SOCKET), QueryTriggerInteraction.Ignore);
            }

            if (RayDetection == DetectionType.Overlap)
            {
                SocketBehaviour[] NeighboursSockets =
                    BuildManager.Instance.GetAllNearestSockets(GetCaster.position, ActionDistance);

                SocketBehaviour[] ApplicableSockets = new SocketBehaviour[NeighboursSockets.Length];

                for (int i = 0; i < NeighboursSockets.Length; i++)
                {
                    if (NeighboursSockets[i] == null)
                    {
                        continue;
                    }

                    foreach (SocketBehaviour Socket in NeighboursSockets)
                    {
                        if (NeighboursSockets[i].gameObject.activeInHierarchy && !Socket.IsDisabled && Socket.AllowPiece(CurrentPreview))
                        {
                            ApplicableSockets[i] = NeighboursSockets[i];
                            break;
                        }
                    }
                }

                if (ApplicableSockets.Length > 0)
                {
                    UpdateMultipleSocket(ApplicableSockets);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }
            else if (RayDetection == DetectionType.Raycast)
            {
                SocketBehaviour Socket = null;

#if ENABLE_INPUT_SYSTEM
                if (CameraType == RayType.VirtualRealityPerson)
                {
                    RayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit Hit);
                }
                else
                {
                    int ColliderCount = Physics.RaycastNonAlloc(GetRay, Hits, ActionDistance, LayerMask.GetMask(Constants.LAYER_SOCKET));

                    for (int i = 0; i < ColliderCount; i++)
                    {
                        if (Hits[i].collider.GetComponent<SocketBehaviour>() != null)
                        {
                            Socket = Hits[i].collider.GetComponent<SocketBehaviour>();
                        }
                    }
                }
#else
                int ColliderCount = Physics.RaycastNonAlloc(GetRay, Hits, ActionDistance, LayerMask.GetMask(Constants.LAYER_SOCKET));

                for (int i = 0; i < ColliderCount; i++)
                {
                    if (Hits[i].collider.GetComponent<SocketBehaviour>() != null)
                    {
                        Socket = Hits[i].collider.GetComponent<SocketBehaviour>();
                    }
                }
#endif

                if (Socket != null)
                {
                    UpdateSingleSocket(Socket);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }          

            CurrentPreview.gameObject.ChangeAllMaterialsColorInChildren(CurrentPreview.Renderers.ToArray(),
                CheckPlacementConditions() ? CurrentPreview.PreviewAllowedColor : CurrentPreview.PreviewDeniedColor, SelectedPrefab.PreviewColorLerpTime, SelectedPrefab.PreviewUseColorLerpTime);
        }

        /// <summary>
        /// This method allows to check the internal placement conditions.
        /// </summary>
        public bool CheckPlacementConditions()
        {
            if (CurrentPreview == null)
            {
                return false;
            }

            if (CurrentPreview.RequireSocket && !HasSocket)
            {
                return false;
            }

            if (OutOfRangeDistance != 0)
            {
                if (Vector3.Distance(GetCaster.position, CurrentPreview.transform.position) > ActionDistance)
                {
                    return false;
                }
            }

            AreaBehaviour NearestArea = BuildManager.Instance.GetNearestArea(CurrentPreview.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllPlacement)
                {
                    if (!NearestArea.CheckAllowedPlacement(CurrentPreview))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForPlacement)
                {
                    return false;
                }
            }

            if (!CurrentPreview.CheckExternalPlacementConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to rotate the current preview.
        /// </summary>
        public void RotatePreview(Vector3 rotateAxis)
        {
            if (CurrentPreview == null)
            {
                return;
            }

            CurrentRotationOffset += rotateAxis;
        }

        /// <summary>
        /// This method allows to move the preview in free movement.
        /// </summary>
        public void UpdateFreeMovement()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            RaycastHit Hit;

#if ENABLE_INPUT_SYSTEM
            if (CameraType == RayType.VirtualRealityPerson)
            {
                RayInteractor.TryGetCurrent3DRaycastHit(out Hit);
            }
            else
            {
                Physics.Raycast(GetRay, out Hit, Distance, BuildManager.Instance.BuildableLayer);
            }
#else
            Physics.Raycast(GetRay, out Hit, Distance, BuildManager.Instance.BuildableLayer);
#endif

            if (Hit.collider != null)
            {
                Vector3 TargetPoint = Hit.point + CurrentPreview.PreviewOffset;
                Vector3 NextPoint = TargetPoint;

                if (CurrentPreview.PreviewClampPosition)
                    NextPoint = MathExtension.Clamp(NextPoint, CurrentPreview.PreviewClampMinPosition, CurrentPreview.PreviewClampMaxPosition);

                if (CurrentPreview.KeepToHeight)
                    NextPoint = new Vector3(NextPoint.x, CurrentPreview.KeepHeight, NextPoint.z);

                if (PreviewMovementType == MovementType.Smooth)
                    NextPoint = Vector3.Lerp(CurrentPreview.transform.position, NextPoint, PreviewSmoothTime * Time.deltaTime);
                else if (PreviewMovementType == MovementType.Grid)
                    NextPoint = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, NextPoint);

                if (PreviewMovementOnlyAllowed)
                {
                    CurrentPreview.transform.position = NextPoint;

                    if (CurrentPreview.CheckExternalPlacementConditions() && CheckPlacementConditions())
                    {
                        LastAllowedPoint = CurrentPreview.transform.position;
                    }
                    else
                    {
                        CurrentPreview.transform.position = LastAllowedPoint;
                    }
                }
                else
                    CurrentPreview.transform.position = NextPoint;

                if (!CurrentPreview.RotateAccordingSlope)
                {
                    if (LockRotation)
                    {
                        CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                    }
                    else
                    {
                        CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);
                    }
                }
                else
                {
                    if (Hit.collider is TerrainCollider)
                    {
                        float SampleHeight = Hit.collider.GetComponent<Terrain>().SampleHeight(Hit.point);

                        if (Hit.point.y - .1f < SampleHeight)
                        {
                            CurrentPreview.transform.rotation = Quaternion.FromToRotation(GetCaster.up, Hit.normal) * Quaternion.Euler(CurrentRotationOffset)
                                * GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        }
                        else
                        {
                            CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                        }
                    }
                    else
                    {
                        CurrentPreview.transform.rotation = Quaternion.FromToRotation(GetCaster.up, Hit.normal) * Quaternion.Euler(CurrentRotationOffset) *
                            GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
                    }
                }

                return;
            }

            if (LockRotation)
            {
                CurrentPreview.transform.rotation = GetCaster.rotation * SelectedPrefab.transform.localRotation * Quaternion.Euler(CurrentRotationOffset);
            }
            else
            {
                CurrentPreview.transform.rotation = Quaternion.Euler(CurrentRotationOffset);
            }

            Transform StartTransform = (CurrentPreview.GroundUpperHeight == 0) ? GetCaster : BuilderCamera.transform;

            Vector3 LookDistance = StartTransform.position + StartTransform.forward * Distance;

            if (CurrentPreview.UseGroundUpper)
            {
                LookDistance.y = Mathf.Clamp(LookDistance.y, GetCaster.position.y - CurrentPreview.GroundUpperHeight,
                    GetCaster.position.y + CurrentPreview.GroundUpperHeight);
            }
            else
            {
                if (Physics.Raycast(CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                    Vector3.down, out RaycastHit HitLook, Mathf.Infinity, BuildManager.Instance.BuildableLayer, QueryTriggerInteraction.Ignore))
                {
                    LookDistance.y = HitLook.point.y;
                }
            }

            if (PreviewMovementType == MovementType.Normal)
            {
                CurrentPreview.transform.position = LookDistance;
            }
            else if (PreviewMovementType == MovementType.Grid)
            {
                CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, LookDistance + CurrentPreview.PreviewOffset);
            }
            else if (PreviewMovementType == MovementType.Smooth)
            {
                CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, LookDistance, PreviewSmoothTime * Time.deltaTime);
            }

            CurrentSocket = null;

            LastSocket = null;

            HasSocket = false;
        }

        /// <summary>
        /// This method allows to move the preview only on available socket(s).
        /// </summary>
        public void UpdateMultipleSocket(SocketBehaviour[] sockets)
        {
            if (CurrentPreview == null || sockets == null)
            {
                return;
            }

            float ClosestAngle = Mathf.Infinity;

            CurrentSocket = null;

            foreach (SocketBehaviour Socket in sockets)
            {
                if (Socket != null)
                {
                    if (Socket.gameObject.activeSelf && !Socket.IsDisabled)
                    {
                        if (!Socket.CheckOccupancy(SelectedPrefab) && Socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                        {
                            if ((Socket.transform.position - (CameraType != RayType.TopDown ? GetCaster.position : TopDownHit.point)).sqrMagnitude <
                                Mathf.Pow(CameraType != RayType.TopDown ? ActionDistance : SnapThreshold, 2))
                            {
                                float Angle = Vector3.Angle(GetRay.direction, Socket.transform.position - GetRay.origin);

                                if (Angle < ClosestAngle && Angle < OverlapAngles)
                                {
                                    ClosestAngle = Angle;

                                    if (CameraType != RayType.TopDown && CurrentSocket == null)
                                    {
                                        CurrentSocket = Socket;
                                    }
                                    else
                                    {
                                        CurrentSocket = Socket;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (CurrentSocket != null)
            {
                Offset OffsetPiece = CurrentSocket.GetOffset(CurrentPreview);

                if (CurrentSocket.CheckOccupancy(CurrentPreview))
                {
                    return;
                }

                if (OffsetPiece != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(OffsetPiece.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation *
                        (CurrentPreview.RotateOnSockets ? Quaternion.Euler(OffsetPiece.Rotation + CurrentRotationOffset) : Quaternion.Euler(OffsetPiece.Rotation));

                    if (OffsetPiece.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = OffsetPiece.Scale;
                    }
                    else
                    {
                        CurrentPreview.transform.localScale = CurrentSocket.transform.parent != null ? CurrentSocket.transform.parent.localScale : transform.localScale;
                    }

                    LastSocket = CurrentSocket;

                    HasSocket = true;

                    return;
                }
            }
            else
                CurrentPreview.transform.localScale = InitialScale;

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to move the preview only on available socket.
        /// </summary>
        public void UpdateSingleSocket(SocketBehaviour socket)
        {
            if (CurrentPreview == null || socket == null)
            {
                return;
            }

            CurrentSocket = null;

            if (socket != null)
            {
                if (socket.gameObject.activeSelf && !socket.IsDisabled)
                {
                    if (socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                    {
                        CurrentSocket = socket;
                    }
                }
            }

            if (CurrentSocket != null)
            {
                Offset Offset = CurrentSocket.GetOffset(CurrentPreview);

                if (CurrentSocket.CheckOccupancy(CurrentPreview))
                {
                    return;
                }

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation
                        * (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = Offset.Scale;
                    }

                    LastSocket = CurrentSocket;

                    HasSocket = true;

                    return;
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to place the current preview.
        /// </summary>
        public virtual void PlacePrefab(GroupBehaviour group = null)
        {
            AllowPlacement = CheckPlacementConditions();

            if (!AllowPlacement)
            {
                return;
            }

            if (CurrentEditionPreview != null)
            {
                Destroy(CurrentEditionPreview.gameObject);
            }

            BuildManager.Instance.PlacePrefab(SelectedPrefab,
                CurrentPreview.transform.position,
                CurrentPreview.transform.eulerAngles,
                CurrentPreview.transform.localScale,
                group,
                CurrentSocket);

            if (Source != null)
            {
                if (PlacementClips.Length != 0)
                {
                    Source.PlayOneShot(PlacementClips[UnityEngine.Random.Range(0, PlacementClips.Length)]);
                }
            }

            CurrentRotationOffset = Vector3.zero;
            CurrentSocket = null;
            LastSocket = null;
            AllowPlacement = false;
            HasSocket = false;

            if (LastMode == BuildMode.Edition && ResetModeAfterEdition)
            {
                ChangeMode(BuildMode.None);
            }

            if (CurrentMode == BuildMode.Placement && ResetModeAfterPlacement)
            {
                ChangeMode(BuildMode.None);
            }

            if (CurrentPreview != null)
            {
                Destroy(CurrentPreview.gameObject);
            }
        }

        /// <summary>
        /// This method allows to create a preview.
        /// </summary>
        public virtual PieceBehaviour CreatePreview(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            CurrentPreview = Instantiate(prefab).GetComponent<PieceBehaviour>();
            CurrentPreview.transform.eulerAngles = Vector3.zero;
            CurrentRotationOffset = Vector3.zero;

            InitialScale = CurrentPreview.transform.localScale;

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Mathf.Infinity, BuildManager.Instance.BuildableLayer, QueryTriggerInteraction.Ignore))
            {
                CurrentPreview.transform.position = Hit.point;
            }

            CurrentPreview.ChangeState(StateType.Preview);

            SelectedPrefab = prefab.GetComponent<PieceBehaviour>();

            BuildEvent.Instance.OnPieceInstantiated.Invoke(CurrentPreview, null);

            CurrentSocket = null;

            LastSocket = null;

            AllowPlacement = false;

            HasSocket = false;

            return CurrentPreview;
        }

        /// <summary>
        /// This method allows to clear the current preview.
        /// </summary>
        public virtual void ClearPreview()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            BuildEvent.Instance.OnPieceDestroyed.Invoke(CurrentPreview);

            Destroy(CurrentPreview.gameObject);

            AllowPlacement = false;

            CurrentPreview = null;
        }

#endregion Placement

        #region Destruction

        /// <summary>
        /// This method allows to update the destruction preview.
        /// </summary>
        public void UpdateRemovePreview()
        {
            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            if (CurrentRemovePreview != null)
            {
                CurrentRemovePreview.ChangeState(StateType.Remove);

                AllowPlacement = false;
            }

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Distance, BuildManager.Instance.BuildableLayer))
            {
                PieceBehaviour Part = Hit.collider.GetComponentInParent<PieceBehaviour>();

                if (Part != null)
                {
                    if (CurrentRemovePreview != null)
                    {
                        if (CurrentRemovePreview.GetInstanceID() != Part.GetInstanceID())
                        {
                            ClearRemovePreview();

                            CurrentRemovePreview = Part;
                        }
                    }
                    else
                    {
                        CurrentRemovePreview = Part;
                    }
                }
                else
                {
                    ClearRemovePreview();
                }
            }
            else
            {
                ClearRemovePreview();
            }
        }

        /// <summary>
        /// This method allows to check the internal destruction conditions.
        /// </summary>
        public bool CheckDestructionConditions()
        {
            if (CurrentRemovePreview == null)
            {
                return false;
            }

            if (!CurrentRemovePreview.IsDestructible)
            {
                return false;
            }

            AreaBehaviour NearestArea = BuildManager.Instance.GetNearestArea(CurrentRemovePreview.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllDestruction)
                {
                    if (!NearestArea.CheckAllowedDestruction(CurrentRemovePreview))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForDestruction)
                {
                    return false;
                }
            }

            if (!CurrentRemovePreview.CheckExternalDestructionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to remove the current preview.
        /// </summary>
        public virtual void DestroyPrefab()
        {
            AllowDestruction = CheckDestructionConditions();

            if (!AllowDestruction)
            {
                return;
            }

            Destroy(CurrentRemovePreview.gameObject);

            if (Source != null)
            {
                if (DestructionClips.Length != 0)
                {
                    Source.PlayOneShot(DestructionClips[UnityEngine.Random.Range(0, DestructionClips.Length)]);
                }
            }

            CurrentSocket = null;

            LastSocket = null;

            AllowDestruction = false;

            HasSocket = false;

            if (ResetModeAfterDestruction)
            {
                ChangeMode(BuildMode.None);
            }
        }

        /// <summary>
        /// This method allows to clear the current remove preview.
        /// </summary>
        public virtual void ClearRemovePreview()
        {
            if (CurrentRemovePreview == null)
            {
                return;
            }

            CurrentRemovePreview.ChangeState(CurrentRemovePreview.LastState);

            AllowDestruction = false;

            CurrentRemovePreview = null;
        }

#endregion Destruction

        #region Edition

        /// <summary>
        /// This method allows to update the edition mode.
        /// </summary>
        public void UpdateEditionPreview()
        {
            AllowEdition = CurrentEditionPreview;

            if (CurrentEditionPreview != null && AllowEdition)
            {
                CurrentEditionPreview.ChangeState(StateType.Edit);
            }

            float Distance = OutOfRangeDistance == 0 ? ActionDistance : OutOfRangeDistance;

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Distance, BuildManager.Instance.BuildableLayer))
            {
                PieceBehaviour Piece = Hit.collider.GetComponentInParent<PieceBehaviour>();

                if (Piece != null)
                {
                    if (CurrentEditionPreview != null)
                    {
                        if (CurrentEditionPreview.GetInstanceID() != Piece.GetInstanceID())
                        {
                            ClearEditionPreview();

                            CurrentEditionPreview = Piece;
                        }
                    }
                    else
                    {
                        CurrentEditionPreview = Piece;
                    }
                }
                else
                {
                    ClearEditionPreview();
                }
            }
            else
            {
                ClearEditionPreview();
            }
        }

        /// <summary>
        /// This method allows to check the internal edition conditions.
        /// </summary>
        public bool CheckEditionConditions()
        {
            if (CurrentEditionPreview == null)
            {
                return false;
            }

            if (!CurrentEditionPreview.IsEditable)
            {
                return false;
            }

            AreaBehaviour NearestArea = BuildManager.Instance.GetNearestArea(CurrentEditionPreview.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllEdition)
                {
                    if (!NearestArea.CheckAllowedEdition(CurrentEditionPreview))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForEdition)
                {
                    return false;
                }
            }

            if (!CurrentEditionPreview.CheckExternalEditionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to edit the current preview.
        /// </summary>
        public virtual void EditPrefab()
        {
            AllowEdition = CheckEditionConditions();

            if (!AllowEdition)
            {
                return;
            }

            PieceBehaviour EditingPiece = CurrentEditionPreview;

            EditingPiece.ChangeState(StateType.Edit);

            SelectPrefab(EditingPiece);

            SelectedPrefab.AppearanceIndex = EditingPiece.AppearanceIndex;

            ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to clear the current edition preview.
        /// </summary>
        public void ClearEditionPreview()
        {
            if (CurrentEditionPreview == null)
            {
                return;
            }

            CurrentEditionPreview.ChangeState(CurrentEditionPreview.LastState);

            AllowEdition = false;

            CurrentEditionPreview = null;
        }

#endregion Edition

        /// <summary>
        /// This method allows to update all the builder (Placement, Destruction, Edition).
        /// </summary>
        public virtual void UpdateModes()
        {
            if (BuildManager.Instance == null)
            {
                return;
            }

            if (BuildManager.Instance.Pieces == null)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                if (SelectedPrefab == null)
                {
                    return;
                }

                if (CurrentPreview == null)
                {
                    CreatePreview(SelectedPrefab.gameObject);
                    return;
                }
                else
                {
                    UpdatePreview();
                }
            }
            else if (CurrentMode == BuildMode.Destruction)
            {
                UpdateRemovePreview();
            }
            else if (CurrentMode == BuildMode.Edition)
            {
                UpdateEditionPreview();
            }
            else if (CurrentMode == BuildMode.None)
            {
                ClearPreview();
            }
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(BuildMode mode)
        {
            if (CurrentMode == mode)
            {
                return;
            }

            if (mode == BuildMode.Placement && !UsePlacementMode)
            {
                return;
            }

            if (mode == BuildMode.Destruction && !UseDestructionMode)
            {
                return;
            }

            if (mode == BuildMode.Edition && !UseEditionMode)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (mode == BuildMode.None)
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditionPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = mode;

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(string modeName)
        {
            if (CurrentMode.ToString() == modeName)
            {
                return;
            }

            if (modeName == BuildMode.Placement.ToString() && !UsePlacementMode)
            {
                return;
            }

            if (modeName == BuildMode.Destruction.ToString() && !UseDestructionMode)
            {
                return;
            }

            if (modeName == BuildMode.Edition.ToString() && !UseEditionMode)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (modeName == BuildMode.None.ToString())
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditionPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = (BuildMode)Enum.Parse(typeof(BuildMode), modeName);

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to select a prefab.
        /// </summary>
        public void SelectPrefab(PieceBehaviour prefab)
        {
            if (prefab == null)
            {
                return;
            }

            SelectedPrefab = BuildManager.Instance.GetPieceById(prefab.Id);
        }

        private void OnDrawGizmosSelected()
        {
            if (BuilderCamera == null)
            {
                BuilderCamera = GetComponent<Camera>();
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(GetRay.origin, GetRay.direction * ActionDistance);
        }

        #endregion Methods
    }
}