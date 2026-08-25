
using MMMaellon.FanCam;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamPlayerTarget : UdonSharpBehaviour
    {
        public FanCamManager manager;
        // public Transform feet;
        VRCPlayerApi owner;
        public VRCPlayerApi Owner
        {
            get => owner;
        }
        int playerId = -1001;
        public int PlayerId
        {
            get => playerId;
        }
        string username = "target";
        public float speed = 4f;
        public string Username
        {
            get => username;
        }

        VRCTweenHandle headHandle;
        // VRCTweenHandle feetHandle;
        void Start()
        {
            owner = Networking.GetOwner(gameObject);
            username = owner.displayName;
            playerId = owner.playerId;
            manager.AddPlayerTarget(this);
            headHandle = VRCTween.TweenPosition(transform, owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, speed, VRCTweenEase.OutSine)
                .SetSpeedBased()
                .OnComplete(this, nameof(UpdatePos));
            // feetHandle = VRCTween.TweenPosition(feet, owner.GetPosition(), speed, VRCTweenEase.OutSine)
            //     .SetSpeedBased();
        }

        void OnDestroy()
        {
            manager.RemovePlayerTarget(playerId);
        }

        public void UpdatePos()
        {
            headHandle.ChangeEndValue(owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, true);
            // feetHandle.ChangeEndValue(owner.GetPosition(), true);
        }
    }
}
