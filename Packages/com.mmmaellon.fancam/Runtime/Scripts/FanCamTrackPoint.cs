
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    public class FanCamTrackPoint : SmartObjectSyncListener
    {
        public FanCamTrackFollower track;
        public SmartObjectSync sync;

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {

        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            if (!Utilities.IsValid(track))
            {
                return;
            }
            if (sync.IsHeld())
            {
                track.ForcePreview(sync);
                return;
            }

            // if (oldState == SmartObjectSync.STATE_LEFT_HAND_HELD || oldState == SmartObjectSync.STATE_RIGHT_HAND_HELD || oldState == SmartObjectSync.STATE_NO_HAND_HELD)
            // {
            // }
            track.PopulateWaypoints();
        }
    }
}
