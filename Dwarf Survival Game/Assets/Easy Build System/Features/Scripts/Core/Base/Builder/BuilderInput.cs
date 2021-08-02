using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Builder
{
    [RequireComponent(typeof(BuilderBehaviour))]
    [AddComponentMenu("Easy Build System/Features/Builders Behaviour/Inputs/Builder Input")]
    public class BuilderInput : MonoBehaviour
    {
        #if ENABLE_INPUT_SYSTEM

        #region Fields

        public static BuilderInput Instance;

        public bool UIBlocking = false;

        public bool UsePlacementModeShortcut = true;
        public bool UseDestructionModeShortcut = true;
        public bool UseEditionModeShortcut = true;

        public DemoInputActions.BuildingActions building;
        public DemoInputActions.UIActions userInteraface;

        private DemoInputActions Inputs;

        public int SelectedIndex { get; set; }

        private bool WheelRotationReleased;
        private bool WheelSelectionReleased;

        #endregion

        #region Methods

        public virtual void OnEnable()
        {
            Inputs.Building.Enable();
            Inputs.UI.Enable();
        }

        public virtual void OnDisable()
        {
            Inputs.Building.Disable();
            Inputs.UI.Disable();
        }

        public virtual void OnDestroy()
        {
            Inputs.Building.Disable();
            Inputs.UI.Disable();
        }

        public virtual void Awake()
        {
            Instance = this;

            Inputs = new DemoInputActions();
            building = Inputs.Building;
            userInteraface = Inputs.UI;
        }

        public virtual void Update()
        {
            if (UsePlacementModeShortcut && building.Placement.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
            }

            if (UseDestructionModeShortcut && building.Destruction.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Destruction);
            }

            if (UseEditionModeShortcut && building.Edition.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Edition);
            }

            if (BuilderBehaviour.Instance.CurrentMode != BuildMode.Placement)
            {
                UpdatePrefabSelection();
            }

            if (building.Cancel.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            }

            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (building.Validate.triggered)
                {
                    BuilderBehaviour.Instance.PlacePrefab();
                }

                float WheelAxis = building.Rotate.ReadValue<float>();

                if (WheelAxis > 0 && !WheelRotationReleased)
                {
                    WheelRotationReleased = true;
                    BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPrefab.RotationAxis);
                }
                else if (WheelAxis < 0 && !WheelRotationReleased)
                {
                    WheelRotationReleased = true;
                    BuilderBehaviour.Instance.RotatePreview(-BuilderBehaviour.Instance.SelectedPrefab.RotationAxis);
                }
                else if (WheelAxis == 0)
                {
                    WheelRotationReleased = false;
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Edition)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (building.Validate.triggered)
                {
                    BuilderBehaviour.Instance.EditPrefab();
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Destruction)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (building.Validate.triggered)
                {
                    if (BuilderBehaviour.Instance.CurrentRemovePreview != null)
                    {
                        BuilderBehaviour.Instance.DestroyPrefab();
                    }
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
        }

        public virtual void UpdatePrefabSelection()
        {
            float WheelAxis = building.Switch.ReadValue<float>();

            if (WheelAxis > 0 && !WheelSelectionReleased)
            {
                WheelSelectionReleased = true;

                if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                {
                    SelectedIndex++;
                }
                else
                {
                    SelectedIndex = 0;
                }
            }
            else if (WheelAxis < 0 && !WheelSelectionReleased)
            {
                WheelSelectionReleased = true;

                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                else
                {
                    SelectedIndex = BuildManager.Instance.Pieces.Count - 1;
                }
            }
            else if (WheelAxis == 0)
            {
                WheelSelectionReleased = false;
            }

            if (SelectedIndex == -1)
            {
                return;
            }

            if (BuildManager.Instance.Pieces.Count != 0)
            {
                BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            }
        }

        /// <summary>
        /// Check if the cursor is above a UI element or if the ciruclar menu is open.
        /// </summary>
        private bool IsPointerOverUIElement()
        {
            if (!UIBlocking)
            {
                return false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return false;
            }

            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData EventData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };

            List<RaycastResult> Results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(EventData, Results);
            return Results.Count > 0;
        }

        #endregion

        #else

        #region Fields

        public bool UseShortcuts = true;
        public bool UIBlocking = false;

        public KeyCode BuilderPlacementModeKey = KeyCode.E;
        public KeyCode BuilderDestructionModeKey = KeyCode.R;
        public KeyCode BuilderEditionModeKey = KeyCode.T;

        public KeyCode BuilderValidateModeKey = KeyCode.Mouse0;
        public KeyCode BuilderCancelModeKey = KeyCode.Mouse1;

        public int SelectedIndex { get; set; }

        #endregion

        #region Methods

        private void Update()
        {
            if (UseShortcuts)
            {
                if (Input.GetKeyDown(BuilderPlacementModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
                }

                if (Input.GetKeyDown(BuilderDestructionModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.Destruction);
                }

                if (Input.GetKeyDown(BuilderEditionModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.Edition);
                }

                if (BuilderBehaviour.Instance.CurrentMode != BuildMode.Placement)
                {
                    UpdatePrefabSelection();
                }

                if (Input.GetKeyDown(BuilderCancelModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }

            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (Input.GetKeyDown(BuilderValidateModeKey))
                {
                    BuilderBehaviour.Instance.PlacePrefab();
                }

                float WheelAxis = Input.GetAxis("Mouse ScrollWheel");

                if (WheelAxis > 0)
                {
                    BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPrefab.RotationAxis);
                }
                else if (WheelAxis < 0)
                {
                    BuilderBehaviour.Instance.RotatePreview(-BuilderBehaviour.Instance.SelectedPrefab.RotationAxis);
                }

                if (Input.GetKeyDown(BuilderCancelModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Edition)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (Input.GetKeyDown(BuilderValidateModeKey))
                {
                    BuilderBehaviour.Instance.EditPrefab();
                }

                if (Input.GetKeyDown(BuilderCancelModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Destruction)
            {
                if (IsPointerOverUIElement())
                {
                    return;
                }

                if (Input.GetKeyDown(BuilderValidateModeKey))
                {
                    if (BuilderBehaviour.Instance.CurrentRemovePreview != null)
                    {
                        BuilderBehaviour.Instance.DestroyPrefab();
                    }
                }

                if (Input.GetKeyDown(BuilderCancelModeKey))
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
        }

        private void UpdatePrefabSelection()
        {
            float WheelAxis = Input.GetAxis("Mouse ScrollWheel");

            if (WheelAxis > 0)
            {
                if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                {
                    SelectedIndex++;
                }
                else
                {
                    SelectedIndex = 0;
                }
            }
            else if (WheelAxis < 0)
            {
                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                else
                {
                    SelectedIndex = BuildManager.Instance.Pieces.Count - 1;
                }
            }

            if (SelectedIndex == -1)
            {
                return;
            }

            if (BuildManager.Instance.Pieces.Count != 0)
            {
                BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            }
        }

        /// <summary>
        /// Check if the cursor is above a UI element or if the ciruclar menu is open.
        /// </summary>
        private bool IsPointerOverUIElement()
        {
            if (!UIBlocking)
            {
                return false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return false;
            }

            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData EventData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };

            List<RaycastResult> Results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(EventData, Results);
            return Results.Count > 0;
        }

        #endregion

        #endif
    }
}