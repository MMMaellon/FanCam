
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamPickupListener : UdonSharpBehaviour
    {
        public FanCam fanCam;
        public override void OnPickup()
        {
            fanCam.OnPickupListener(this);
        }

        public override void OnDrop()
        {
            fanCam.OnDropListener(this);
        }

        public void Reset()
        {
            fanCam = GetComponentInParent<FanCam>();
        }
    }
}
