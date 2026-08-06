
using MMMaellon.FanCam;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamPlayerTarget : UdonSharpBehaviour
    {
        public FanCamManager manager;
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
        string username = "Staff-san";
        public string Username
        {
            get => username;
        }

        void Start()
        {
            owner = Networking.GetOwner(gameObject);
            username = owner.displayName;
            playerId = owner.playerId;
            manager.AddPlayerTarget(this);
        }

        void OnDestroy()
        {
            manager.RemovePlayerTarget(playerId);
        }
    }
}
