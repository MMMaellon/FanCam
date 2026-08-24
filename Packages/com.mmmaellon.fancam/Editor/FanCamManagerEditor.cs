#if !COMPILER_UDON && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace MMMaellon.FanCam
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FanCamManager), true)]
    public class FanCamManagerEditor : Editor
    {
        static bool fanCamFoldout = true;
        static bool managedFanCamFoldout = true;
        static bool otherFanCamFoldout = false;
        List<FanCam> managedFanCams = new List<FanCam>();
        List<FanCam> nullFanCams = new List<FanCam>();
        List<FanCam> otherFanCams = new List<FanCam>();
        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets);
            // if (Application.isPlaying || PrefabStageUtility.GetCurrentPrefabStage() != null)
            // {
            //     //TODO display fancam dictionary
            //     return;
            // }
            //
            // managedFanCams.Clear();
            // nullFanCams.Clear();
            // otherFanCams.Clear();

            EditorGUILayout.BeginHorizontal();
            FanCamSetup.AutoSetup = GUILayout.Toggle(FanCamSetup.AutoSetup, "Automatic Setup", GUILayout.Width(128));
            if (GUILayout.Button("Perform Manual Setup"))
            {
                FanCamSetup.Setup();
            }
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();

            // foreach (var fancam in FanCam.All)
            // {
            //     if (fancam.manager == target)
            //     {
            //         managedFanCams.Add(fancam);
            //     }
            //     else if (fancam.manager == null)
            //     {
            //         nullFanCams.Add(fancam);
            //     }
            //     else
            //     {
            //         otherFanCams.Add(fancam);
            //     }
            // }
            //
            // fanCamFoldout = EditorGUILayout.Foldout(fanCamFoldout, $"FanCams ({managedFanCams.Count + nullFanCams.Count + otherFanCams.Count})", true);
            // EditorGUI.indentLevel++;
            // if (fanCamFoldout)
            // {
            //     managedFanCamFoldout = EditorGUILayout.Foldout(managedFanCamFoldout, $"This manager ({managedFanCams.Count})", true);
            //     if (managedFanCamFoldout)
            //     {
            //         foreach (var fancam in managedFanCams)
            //         {
            //             if (fancam.manager == target)
            //             {
            //                 EditorGUILayout.BeginHorizontal();
            //                 EditorGUI.BeginDisabledGroup(true);
            //                 EditorGUILayout.LabelField(fancam.Id.ToString(), GUILayout.Width(42));
            //                 EditorGUILayout.ObjectField(fancam, typeof(FanCam), true);
            //                 EditorGUI.EndDisabledGroup();
            //                 if (GUILayout.Button("Remove FanCam manager"))
            //                 {
            //                     SerializedObject so = new(fancam);
            //                     Undo.SetCurrentGroupName("Remove FanCam");
            //                     so.FindProperty("manager").objectReferenceValue = null;
            //                     so.ApplyModifiedProperties();
            //                 }
            //                 EditorGUILayout.EndHorizontal();
            //             }
            //         }
            //     }
            //     otherFanCamFoldout = EditorGUILayout.Foldout(otherFanCamFoldout, $"Other ({nullFanCams.Count + otherFanCams.Count})", true);
            //     if (otherFanCamFoldout)
            //     {
            //         Color ogColor = GUI.backgroundColor;
            //         foreach (var fancam in nullFanCams.Concat(otherFanCams))
            //         {
            //             EditorGUILayout.BeginHorizontal();
            //             EditorGUI.BeginDisabledGroup(true);
            //             EditorGUILayout.LabelField(fancam.Id.ToString(), GUILayout.Width(42));
            //             EditorGUILayout.ObjectField(fancam, typeof(FanCam), true);
            //             if (fancam.manager == null)
            //             {
            //                 GUI.backgroundColor = Color.blue;
            //             }
            //             EditorGUILayout.ObjectField(fancam.manager, typeof(FanCamManager), true);
            //             EditorGUI.EndDisabledGroup();
            //             if (fancam.manager == null)
            //             {
            //                 GUI.backgroundColor = ogColor;
            //             }
            //             if (GUILayout.Button("Set manager"))
            //             {
            //                 SerializedObject so = new(fancam);
            //                 Undo.SetCurrentGroupName("Set FanCam Manager");
            //                 so.FindProperty("manager").objectReferenceValue = target;
            //                 so.ApplyModifiedProperties();
            //             }
            //             EditorGUILayout.EndHorizontal();
            //         }
            //     }
            // }
            // EditorGUI.indentLevel--;
        }
    }
}
#endif
