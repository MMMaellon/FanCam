
using MMMaellon.FanCam;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    public class FanCamPlayerTarget : UdonSharpBehaviour
    {
        public FanCam fanCam;
        VRCPlayerApi _target;
        public VRCPlayerApi Target
        {
            get => _target;
            set
            {
                _target = value;
                if (Utilities.IsValid(value))
                {
                    tracking = true;
                    SendCustomEventDelayedFrames(nameof(TrackPlayer), 2);
                }
                else
                {
                    if (fanCam.TargetPlayerId >= 0)
                    {
                        fanCam.TargetPlayerId = -1001;
                    }
                    tracking = false;
                }
            }
        }

        bool tracking = false;
        public void TrackPlayer()
        {
            if (!tracking || !Utilities.IsValid(_target))
            {
                if (fanCam.TargetPlayerId >= 0)
                {
                    fanCam.TargetPlayerId = -1001;
                }
                return;
            }
            SendCustomEventDelayedFrames(nameof(TrackPlayer), 0);
            transform.position = _target.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        }
    }
}
