
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    public class FanCamGridVisibility : UdonSharpBehaviour
    {
        public FanCamMenu menu;
        FanCamManager manager;
        // public void Start()
        // {
        //     manager = menu.manager;
        //     Debug.LogWarning("START ASDFADFADSF");
        //     manager.OnGridEnable();
        // }

        // public void OnEnable()
        // {
        //     if (Time.timeSinceLevelLoad < 1)
        //     {
        //         SendCustomEventDelayedSeconds(nameof(EnabledCheck), 1);
        //         return;
        //     }
        //     if (!Utilities.IsValid(manager))
        //     {
        //         manager = menu.manager;
        //     }
        //     // Debug.LogWarning("ASDFASDFASDFASDFAFD ON ENABLE");
        //     manager.OnGridEnable();
        // }
        //
        // public void OnDisable()
        // {
        //     if (!Utilities.IsValid(manager))
        //     {
        //         manager = menu.manager;
        //     }
        //     // Debug.LogWarning("ASDFASDFASDFASDFAFD ON DISABLE");
        //     manager.OnGridDisable();
        // }
        //
        // bool checkComplete = false;
        // public void EnabledCheck()
        // {
        //     if (checkComplete)
        //     {
        //         return;
        //     }
        //     if (gameObject.activeInHierarchy && enabled)
        //     {
        //         OnEnable();
        //     }
        //     checkComplete = true;
        // }
    }
}
