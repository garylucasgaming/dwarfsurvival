using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Collection;
using EasyBuildSystem.Features.Scripts.Editor.Window;
using System;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor
{
    public class QuickStart : MonoBehaviour
    {
        #region Methods

        [MenuItem("GameObject/Easy Build System/Quick Start...", false, priority = -700)]
        [MenuItem("Tools/Easy Build System/Quick Start...", priority = -700)]
        public static void Init()
        {
            try
            {
                if (!EditorUtility.DisplayDialog("Easy Build System - Quick Start",
                    "This will install in your scene the components required to make work the system by default.\n\n" +
                    "Make you sure to have a camera in your scene with the tag “Main Camera”.\n\n" +
                    "You can will find more information about Quick Start feature in the documentation.\n\n" +
                    "Do you want run the Quick Start?", "Yes", "Cancel"))
                {
                    return;
                }

                ImporterEditor.AddMissingLayers(new string[1] { "Socket" });

                if (Camera.main != null)
                {
                    if (FindObjectOfType<BuilderBehaviour>() == null)
                    {
                        Camera.main.gameObject.AddComponent<BuilderBehaviour>();
                        Camera.main.gameObject.AddComponent<BuilderInput>();
                    }
                }

                if (FindObjectOfType<BuildManager>() != null)
                {
                    Debug.LogWarning("<b>Easy Build System</b> : This scene was already setup!");
                    return;
                }

                BuildManager Manager = new GameObject("Easy Build System - Build Manager").AddComponent<BuildManager>();

                Manager.Pieces.AddRange(Resources.Load<PieceCollection>("Default - Modular Building Collection").Pieces);

                Debug.Log("<b>Easy Build System</b> : You can now use the system on this scene!");
            }
            catch (Exception ex)
            {
                Debug.LogError("<b>Easy Build System</b> : " + ex.Message);
            }
        }

        #endregion Methods
    }
}