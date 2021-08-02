using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Surface;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Blueprint;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Manager
{
    [ExecuteInEditMode, DefaultExecutionOrder(-998)]
    [AddComponentMenu("Easy Build System/Features/Build Manager")]
    [RequireComponent(typeof(BuildEvent))]
    public class BuildManager : MonoBehaviour
    {
        #region Fields

        public static BuildManager Instance;

        public LayerMask BuildableLayer = 1 << 0;

        public SupportType[] BuildableSurfaces = new SupportType[1] { SupportType.AnyCollider };

        public BlueprintTemplate[] Blueprints;

        public List<PieceBehaviour> Pieces = new List<PieceBehaviour>();

        public StateType DefaultState = StateType.Placed;

        public List<PieceBehaviour> CachedParts = new List<PieceBehaviour>();
        public List<SocketBehaviour> CachedSockets = new List<SocketBehaviour>();
        public List<AreaBehaviour> CachedAreas = new List<AreaBehaviour>();

        #endregion Fields

        #region Methods

        public void OnEnable()
        {
            Instance = this;
        }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// This method allows to add a piece from the manager cache.
        /// </summary>
        public void AddPiece(PieceBehaviour piece)
        {
            if (piece == null)
            {
                return;
            }

            CachedParts.Add(piece);
        }

        /// <summary>
        /// This method allows to remove a piece from the manager cache.
        /// </summary>
        public void RemovePiece(PieceBehaviour piece)
        {
            if (piece == null)
            {
                return;
            }

            CachedParts.Remove(piece);
        }

        /// <summary>
        /// This method allows to add a socket from the manager cache.
        /// </summary>
        public void AddSocket(SocketBehaviour socket)
        {
            if (socket == null)
            {
                return;
            }

            CachedSockets.Add(socket);
        }

        /// <summary>
        /// This method allows to remove a socket from the manager cache.
        /// </summary>
        public void RemoveSocket(SocketBehaviour socket)
        {
            if (socket == null)
            {
                return;
            }

            CachedSockets.Remove(socket);
        }

        /// <summary>
        /// This method allows to add a area from the manager cache.
        /// </summary>
        public void AddArea(AreaBehaviour area)
        {
            if (area == null)
            {
                return;
            }

            CachedAreas.Add(area);
        }

        /// <summary>
        /// This method allows to remove a area from the manager cache.
        /// </summary>
        public void RemoveArea(AreaBehaviour area)
        {
            if (area == null)
            {
                return;
            }

            CachedAreas.Remove(area);
        }

        /// <summary>
        /// This method allows to get a prefab by id.
        /// </summary>
        public PieceBehaviour GetPieceById(int id)
        {
            return Pieces.Find(entry => entry.Id == id);
        }

        /// <summary>
        /// This method allows to get a prefab by name.
        /// </summary>
        public PieceBehaviour GetPieceByName(string name)
        {
            return Pieces.Find(entry => entry.Name == name);
        }

        /// <summary>
        /// This method allows to get a prefab by category.
        /// </summary>
        public PieceBehaviour GetPieceByCategory(string category)
        {
            return Pieces.Find(entry => entry.Category == category);
        }

        /// <summary>
        /// This method allows to get all the nearest sockets from point according radius.
        /// </summary>
        public SocketBehaviour[] GetAllNearestSockets(Vector3 point, float radius)
        {
            List<SocketBehaviour> Result = new List<SocketBehaviour>();

            for (int i = 0; i < CachedSockets.Count; i++)
            {
                if (CachedSockets[i] != null)
                {
                    if (Vector3.Distance(CachedSockets[i].transform.position, point) < radius)
                    {
                        Result.Add(CachedSockets[i]);
                    }
                }
            }

            return Result.ToArray();
        }

        /// <summary>
        /// This method allows to place a piece.
        /// </summary>
        public PieceBehaviour PlacePrefab(PieceBehaviour piece, Vector3 position, Vector3 rotation, Vector3 scale, GroupBehaviour group = null, SocketBehaviour socket = null, bool createGroup = true)
        {
            GameObject PlacedObject = Instantiate(piece.gameObject, position, Quaternion.Euler(rotation));

            PlacedObject.transform.localScale = scale;

            PieceBehaviour PlacedPart = PlacedObject.GetComponent<PieceBehaviour>();

            if (group == null)
            {
                if (socket != null)
                {
                    if (socket.ParentPiece != null && socket.ParentPiece.HasGroup)
                    {
                        PlacedObject.transform.SetParent(socket.ParentPiece.transform.parent, true);
                    }
                }
                else
                {
                    if (createGroup)
                    {
                        GroupBehaviour InstancedGroup = new GameObject("Group (" + PlacedPart.GetInstanceID() + ")").AddComponent<GroupBehaviour>();
                        PlacedObject.transform.SetParent(InstancedGroup.transform, true);
                        BuildEvent.Instance.OnGroupInstantiated.Invoke(InstancedGroup);
                    }
                }
            }
            else
            {
                PlacedObject.transform.SetParent(group.transform, true);
                BuildEvent.Instance.OnGroupUpdated.Invoke(group);
            }

            PlacedPart.ChangeState(DefaultState);

            BuildEvent.Instance.OnPieceInstantiated.Invoke(PlacedPart, socket);

            return PlacedPart;
        }

        /// <summary>
        /// This method allows to destroy a piece.
        /// </summary>
        public void DestroyPrefab(PieceBehaviour piece)
        {
            Destroy(piece.gameObject);
        }

        /// <summary>
        /// This method allows to get the nearest area via an world position.
        /// </summary>
        public AreaBehaviour GetNearestArea(Vector3 position)
        {
            foreach (AreaBehaviour Area in CachedAreas)
            {
                if (Area != null)
                {
                    if (Area.gameObject.activeSelf == true)
                    {
                        if (Area.Shape == Base.Area.Enums.AreaShape.Bounds)
                        {
                            if (Area.transform.ConvertBoundsToWorld(Area.Bounds).Contains(position))
                            {
                                return Area;
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(position, Area.transform.position) <= Area.Radius)
                            {
                                return Area;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// This method allows to load a blueprint data in edit times.
        /// </summary>
        public void LoadBlueprintInEditor()
        {
            foreach (BlueprintTemplate blueprint in Blueprints)
            {
                LoadBlueprintInEditor(blueprint);
            }
        }

        /// <summary>
        /// This method allows to load a blueprint data in runtimes.
        /// </summary>
        public void LoadBlueprintInRuntimeEditor()
        {
            foreach (BlueprintTemplate blueprint in Blueprints)
            {
                LoadBlueprintInRuntimeEditor(blueprint);
            }
        }

        /// <summary>
        /// This method allows to load a blueprint data in edit times.
        /// </summary>
        public void LoadBlueprintInEditor(BlueprintTemplate blueprint)
        {
            BuildManager Manager = FindObjectOfType<BuildManager>();

            if (Manager == null)
            {
                Debug.LogError("<b>Easy Build System</b> : The build manager does not exists.");

                return;
            }

            PieceData.SerializedPiece[] Parts = blueprint.Model.DecodeToStr(blueprint.Data);

            GroupBehaviour Group = new GameObject("(Editor) Blueprint").AddComponent<GroupBehaviour>();

            for (int i = 0; i < Parts.Length; i++)
            {
                PieceBehaviour Part = Manager.GetPieceById(Parts[i].Id);

                PieceBehaviour PlacedPart = Manager.PlacePrefab(Part, PieceData.ParseToVector3(Parts[i].Position),
                    PieceData.ParseToVector3(Parts[i].Rotation), PieceData.ParseToVector3(Parts[i].Scale), Group);

                PlacedPart.ChangeSkin(Parts[i].AppearanceIndex);
            }
        }

        /// <summary>
        /// This method allows to load a blueprint data in runtimes.
        /// </summary>
        public void LoadBlueprintInRuntimeEditor(BlueprintTemplate blueprint)
        {
            BuildManager Manager = FindObjectOfType<BuildManager>();

            if (Manager == null)
            {
                Debug.LogError("<b>Easy Build System</b> : The build manager does not exists.");

                return;
            }

            PieceData.SerializedPiece[] Parts = blueprint.Model.DecodeToStr(blueprint.Data);

            GroupBehaviour Group = new GameObject("(Runtime) Blueprint").AddComponent<GroupBehaviour>();

            for (int i = 0; i < Parts.Length; i++)
            {
                PieceBehaviour Part = Manager.GetPieceById(Parts[i].Id);

                PieceBehaviour PlacedPart = Manager.PlacePrefab(Part, PieceData.ParseToVector3(Parts[i].Position),
                    PieceData.ParseToVector3(Parts[i].Rotation), PieceData.ParseToVector3(Parts[i].Scale), Group);

                PlacedPart.ChangeSkin(Parts[i].AppearanceIndex);
            }
        }

        /// <summary>
        /// This method allows to check if the collider is a buildable surface.
        /// </summary>
        public bool IsBuildableSurface(Collider collider)
        {
            if (BuildableSurfaces.Contains(SupportType.AnyCollider))
                return true;

            for (int i = 0; i < BuildableSurfaces.Length; i++)
            {
                if (collider.GetComponent<SurfaceCollider>())
                {
                    return BuildableSurfaces.Contains(SupportType.SurfaceCollider);
                }

                if (collider.GetComponent<TerrainCollider>())
                {
                    return BuildableSurfaces.Contains(SupportType.TerrainCollider);
                }
            }

            return false;
        }

        #endregion Methods
    }
}