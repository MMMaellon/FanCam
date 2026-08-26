
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    public class TeleportUprightOnDrop : SmartObjectSyncListener
    {
        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {

        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            if (!sync.IsLocalOwner())
            {
                return;
            }
            if (oldState != SmartObjectSync.STATE_LEFT_HAND_HELD || oldState != SmartObjectSync.STATE_RIGHT_HAND_HELD || sync.pickup.IsHeld)
            {
                return;
            }
            if (sync.transform.forward == Vector3.up)
            {
                return;
            }
            sync.transform.SetPositionAndRotation(sync.transform.position, Quaternion.LookRotation(sync.transform.forward, Vector3.up));
        }
    }
}
