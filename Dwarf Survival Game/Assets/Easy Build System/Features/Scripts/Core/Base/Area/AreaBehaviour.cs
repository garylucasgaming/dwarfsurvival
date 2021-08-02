using EasyBuildSystem.Features.Scripts.Core.Base.Area.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Area
{
    [AddComponentMenu("Easy Build System/Features/Buildings Behaviour/Area Behaviour")]
    public class AreaBehaviour : MonoBehaviour
    {
        #region Fields

        public AreaShape Shape;
        public float Radius = 5f;
        public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);

        public bool AllowAllPlacement = true;
        public List<PieceBehaviour> AllowPlacementSpecificPieces = new List<PieceBehaviour>();
        public bool AllowAllDestruction = true;
        public List<PieceBehaviour> AllowDestructionSpecificPieces = new List<PieceBehaviour>();
        public bool AllowAllEdition = true;
        public List<PieceBehaviour> AllowEditionSpecificPieces = new List<PieceBehaviour>();

        #endregion Fields

        #region Methods

        private void Start()
        {
            BuildManager.Instance.AddArea(this);
        }

        private void OnDestroy()
        {
            BuildManager.Instance.RemoveArea(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan / 2;

            if (Shape == AreaShape.Bounds)
            {
                Gizmos.DrawCube(transform.TransformPoint(Bounds.center), Bounds.size);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.TransformPoint(Bounds.center), Bounds.size);
            }
            else if (Shape == AreaShape.Sphere)
            {
                Gizmos.DrawSphere(transform.position, Radius);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, Radius);
            }
        }

        /// <summary>
        /// This method allows to check if the piece is allowed for placement by this area.
        /// </summary>
        public bool CheckAllowedPlacement(PieceBehaviour piece)
        {
            if (AllowPlacementSpecificPieces.Count == 0) return false;

            return AllowPlacementSpecificPieces.Find(entry => entry.Id == piece.Id);
        }

        /// <summary>
        /// This method allows to check if the piece is allowed for destruction by this area.
        /// </summary>
        public bool CheckAllowedDestruction(PieceBehaviour piece)
        {
            if (AllowDestructionSpecificPieces.Count == 0) return false;

            return AllowDestructionSpecificPieces.Find(entry => entry.Id == piece.Id);
        }

        /// <summary>
        /// This method allows to check if the piece is allowed for edition by this area.
        /// </summary>
        public bool CheckAllowedEdition(PieceBehaviour piece)
        {
            if (AllowEditionSpecificPieces.Count == 0) return false;

            return AllowEditionSpecificPieces.Find(entry => entry.Id == piece.Id);
        }

        #endregion Methods
    }
}