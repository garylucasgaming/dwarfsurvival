using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Data;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Socket
{
    [AddComponentMenu("Easy Build System/Features/Buildings Behaviour/Socket Behaviour")]
    public class SocketBehaviour : MonoBehaviour
    {
        #region Fields

        public float Radius = 0.25f;
        public List<Offset> PartOffsets = new List<Offset>();
        public List<Occupancy> BusySpaces = new List<Occupancy>();

        public PieceBehaviour ParentPiece;

        private Collider _CachedCollider;
        public Collider CachedCollider
        {
            get
            {
                if (_CachedCollider == null)
                    _CachedCollider = GetComponent<Collider>();

                return _CachedCollider;
            }
        }

        public bool IsDisabled;

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_SOCKET);
            ParentPiece = GetComponentInParent<PieceBehaviour>();
            gameObject.AddSphereCollider(Radius);
        }

        private void Start()
        {
            BuildManager.Instance.AddSocket(this);

            if (ParentPiece != null)
            {
                if (ParentPiece.CurrentState != Piece.Enums.StateType.Preview)
                {
                    UpdateOccupancy(true);
                }
            }
            else
            {
                UpdateOccupancy(true);
            }
        }

        private void OnDestroy()
        {
            BuildManager.Instance.RemoveSocket(this);

            if (ParentPiece != null)
            {
                if (ParentPiece.CurrentState != Piece.Enums.StateType.Preview)
                {
                    UpdateOccupancy(false);
                }
            }
            else
            {
                UpdateOccupancy(false);
            }
        }

        public void OnDrawGizmos()
        {
            if (!ShowGizmos)
                return;

            if (IsDisabled)
            {
                return;
            }

            if (BusySpaces.Count != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position, Vector3.one / 6);
            }
            else
            {
                Gizmos.color = Color.cyan / 2;
                Gizmos.DrawCube(transform.position, Vector3.one / 6);
                Gizmos.color = Color.cyan;
            }

            Gizmos.DrawWireCube(transform.position, Vector3.one / 6);
            Gizmos.DrawWireSphere(transform.position, 1f * Radius);
        }

        /// <summary>
        /// This method allows to disable the collider of the socket.
        /// </summary>
        public void DisableSocketCollider()
        {
            IsDisabled = true;

            if (CachedCollider != null)
            {
                CachedCollider.gameObject.layer = Physics.IgnoreRaycastLayer;
            }
        }

        /// <summary>
        /// This method allows to enable the collider of the socket.
        /// </summary>
        public void EnableSocketCollider()
        {
            IsDisabled = false;

            if (CachedCollider != null)
            {
                CachedCollider.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_SOCKET);
            }
        }

        /// <summary>
        /// This method allows to check if the piece is contains in the offset list of this socket.
        /// </summary>
        public bool AllowPiece(PieceBehaviour piece)
        {
            if (piece == null) return false;

            for (int i = 0; i < PartOffsets.Count; i++)
            {
                if (PartOffsets[i] != null && PartOffsets[i].Piece != null)
                {
                    if (PartOffsets[i].AllowSameCategory)
                    {
                        if (PartOffsets[i].Piece.Category == piece.Category)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (PartOffsets[i].Piece.Id == piece.Id)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method allows to check if the piece is placed on this socket.
        /// </summary>
        public bool CheckOccupancy(PieceBehaviour piece)
        {
            for (int i = 0; i < BusySpaces.Count; i++)
            {
                if (BusySpaces[i].Piece.Category == piece.Category)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method allows to add a occupancy on this socket.
        /// </summary>
        public void AddOccupancy(PieceBehaviour piece)
        {
            if (!CheckOccupancy(piece))
            {
                if (piece != ParentPiece)
                {
                    BusySpaces.Add(new Occupancy(piece));
                }
            }
        }

        /// <summary>
        /// This method allows to remove a occupancy from this socket.
        /// </summary>
        public void RemoveOccupancy(PieceBehaviour piece)
        {
            if (CheckOccupancy(piece))
            {
                BusySpaces.Remove(BusySpaces.Find(entry => entry.Piece == piece));
            }
        }

        /// <summary>
        /// This method allows to change the current socket state.
        /// </summary>
        public void ChangeOccupancy(bool busy, PieceBehaviour piece)
        {
            if (busy)
            {
                if (!CheckOccupancy(piece))
                {
                    AddOccupancy(piece);
                }
            }
            else
            {
                if (CheckOccupancy(piece))
                {
                    RemoveOccupancy(piece);
                }
            }

            BuildEvent.Instance.OnChangedSocketState.Invoke(this, busy);
        }

        /// <summary>
        /// This method allows to change the state of the socket if it touches a nearby mesh bounds of Piece Behaviour.
        /// </summary>
        private void UpdateOccupancy(bool busy)
        {
            PieceBehaviour[] Pieces = PhysicExtension.GetNeighborsTypeBySphere<PieceBehaviour>(transform.position, Radius, Physics.AllLayers);

            for (int i = 0; i < Pieces.Length; i++)
            {
                if (Pieces[i] != null && Pieces[i] != ParentPiece)
                {
                    if (AllowPiece(Pieces[i]))
                    {
                        ChangeOccupancy(busy, Pieces[i]);
                    }
                }
            }
        }

        /// <summary>
        /// This method allows to get the piece offset wich allowed on this socket.
        /// </summary>
        public Offset GetOffset(PieceBehaviour piece)
        {
            for (int i = 0; i < PartOffsets.Count; i++)
            {
                if (PartOffsets[i].AllowSameCategory)
                {
                    if (PartOffsets[i].Piece.Category == piece.Category)
                    {
                        return PartOffsets[i];
                    }
                }
                else
                {
                    if (PartOffsets[i].Piece != null && PartOffsets[i].Piece.Id == piece.Id)
                    {
                        return PartOffsets[i];
                    }
                }
            }

            return null;
        }

        #endregion Methods
    }
}