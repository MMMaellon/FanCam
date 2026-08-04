
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamManager : UdonSharpBehaviour
    {
        [HideInInspector]
        public DataDictionary fancams;
        [UdonSynced, FieldChangeCallback(nameof(ActiveCamIndex))]
        int _activeCamIndex = 0;
        public int ActiveCamIndex
        {
            get => _activeCamIndex;
            set
            {
                _activeCamIndex = value;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }

                if (fancams.TryGetValue(value, TokenType.Reference, out DataToken activeToken))
                {
                    var newCam = (FanCam)activeToken.Reference;
                    if (Utilities.IsValid(_activeCam))
                    {
                        if (newCam == _activeCam && _activeCam.CamActive)
                        {
                            //Skip disabling and re-enabling camera.
                            return;
                        }
                        _activeCam.DisableCam();
                    }
                    _activeCam = newCam;
                    if (Utilities.IsValid(_activeCam))
                    {
                        _activeCam.EnableCam();
                    }
                }
                else
                {
                    Debug.LogWarning($"Warning: FanCam failed to set the active camera to {value} from manager. Assuming it's a timing issue, we'll wait for the FanCam's network update");
                }
            }
        }
        FanCam _activeCam = null;
        public FanCam ActiveCam
        {
            get => _activeCam;
        }

        VRCPlayerApi owner;
        public void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
        }
        public VRCPlayerApi Owner
        {
            get => owner;
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            owner = player;
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(owner) && owner.isLocal;
        }

        public void AddFanCam(FanCam newCam)
        {
            if (fancams.ContainsKey(newCam.Id))
            {
                Debug.LogWarning($"[FANCAM] Fan cam with conflicting Id {newCam.Id} tried to add itself to the manager");
                return;
            }
            fancams.Add(newCam.Id, newCam);
            ActiveCamIndex = ActiveCamIndex;
        }

        public void RemoveFanCam(FanCam oldCam)
        {
            if (oldCam == ActiveCam && IsOwnerLocal())
            {
                ActiveCamIndex = 0;
            }
            fancams.Remove(oldCam.Id);
            ActiveCamIndex = ActiveCamIndex;
        }
    }
}
