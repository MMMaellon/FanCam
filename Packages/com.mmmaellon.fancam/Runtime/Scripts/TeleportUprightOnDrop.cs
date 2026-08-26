
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

        SmartObjectSync lastSync;
        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            if (!sync.IsLocalOwner())
            {
                return;
            }
            if (oldState != SmartObjectSync.STATE_LEFT_HAND_HELD || oldState != SmartObjectSync.STATE_RIGHT_HAND_HELD || oldState != SmartObjectSync.STATE_NO_HAND_HELD || newState == SmartObjectSync.STATE_LEFT_HAND_HELD || newState == SmartObjectSync.STATE_RIGHT_HAND_HELD || newState == SmartObjectSync.STATE_NO_HAND_HELD)
            {
                return;
            }
            if (sync.transform.forward == Vector3.up)
            {
                return;
            }
            lastSync = sync;
            SendCustomEventDelayedFrames(nameof(StandUpright), 0);
        }

        public void StandUpright()
        {
            if (lastSync.IsLocalOwner() && lastSync.state == SmartObjectSync.STATE_INTERPOLATING)
            {
                lastSync.TeleportToWorldSpace(lastSync.transform.position, Quaternion.LookRotation(lastSync.transform.forward, Vector3.up), Vector3.zero, Vector3.zero);
            }
            // lastSync.transform.SetPositionAndRotation(lastSync.transform.position, Quaternion.LookRotation(lastSync.transform.forward, Vector3.up));
        }
    }
}
