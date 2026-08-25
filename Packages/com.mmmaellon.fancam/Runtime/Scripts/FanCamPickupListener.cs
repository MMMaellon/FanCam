
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRC_Pickup))]
    public class FanCamPickupListener : UdonSharpBehaviour
    {
        public FanCam fanCam;
        public override void OnPickup()
        {
            fanCam.OnPickupListener(this);
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                pickupGun = pickup.ExactGun;
                pickup.ExactGun = null;
            }
        }

        public override void OnDrop()
        {
            fanCam.OnDropListener(this);
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                pickup.ExactGun = pickupGun;
            }
        }

        public void Reset()
        {
            fanCam = GetComponentInParent<FanCam>();
        }

        Transform pickupGun;
        VRC_Pickup pickup;
        public void Start()
        {
            pickup = GetComponent<VRC_Pickup>();
            if (Networking.LocalPlayer.IsUserInVR())
            {
                pickupGun = pickup.ExactGun;
                pickup.ExactGun = null;
            }
        }
    }
}
