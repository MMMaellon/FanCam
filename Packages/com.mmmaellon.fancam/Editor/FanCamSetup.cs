#if !COMPILER_UDON && UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace MMMaellon.FanCam
{
    [InitializeOnLoad]
    class FanCamSetup : IVRCSDKBuildRequestedCallback
    {
        static FanCamSetup()
        {
            ObjectChangeEvents.changesPublished += OnChangesPublished;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && AutoSetup)
            {
                Setup();
            }
        }

        public static void Setup()
        {
            if (Application.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }

            int counter = 0;
            foreach (var fancam in GetAll<FanCam>())
            {
                counter++;
                var so = new SerializedObject(fancam);
                Undo.SetCurrentGroupName("FanCam Setup");
                so.FindProperty("_id").intValue = counter;
                so.ApplyModifiedProperties();
            }
            //
            //     foreach (var menu in FanCamMenu.All)
            //     {
            //         if (Utilities.IsValid(menu.manager))
            //         {
            //             menu.manager.AddMenu(menu);
            //         }
            //     }
            //
            //     foreach (var picBtn in GetAll<FanCamPictureButton>())
            //     {
            //         picBtn.Setup();
            //     }
            foreach (var switcher in GetAll<FanCamSwitcher>())
            {
                switcher.Setup();
            }
        }

        public static T[] GetAll<T>() where T : Object
        {
            if (Application.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return new T[0];
            }
            return Object.FindObjectsOfType<T>(true);
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType == VRCSDKRequestedBuildType.Scene && AutoSetup)
            {
                Setup();
            }
            return true;
        }

        public static bool AutoSetup = true;

        public int callbackOrder => 0;
    }
}
#endif
