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
            FanCam.MarkDirty();
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
            foreach (var fancam in FanCam.All)
            {
                counter++;
                var so = new SerializedObject(fancam);
                Undo.SetCurrentGroupName("FanCam Setup");
                so.FindProperty("_id").intValue = counter;
                so.FindProperty("_isPlayerObject").boolValue = fancam.CheckForPlayerObj();
                so.ApplyModifiedProperties();
            }

            foreach (var menu in FanCamMenu.All)
            {
                if (Utilities.IsValid(menu.manager))
                {
                    menu.manager.AddMenu(menu);
                }
            }
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            // throw new System.NotImplementedException();
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
