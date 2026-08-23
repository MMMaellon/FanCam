
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace MMMaellon.FanCam
{
    [RequireComponent(typeof(Animator))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FanCamManager : UdonSharpBehaviour
    {
        private readonly int CamHash = Animator.StringToHash("cam");
        public Animator animator;
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
                animator.SetInteger(CamHash, value);
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }

                // if (fancams.TryGetValue(value, TokenType.Reference, out DataToken activeToken))
                // {
                //     var newCam = (FanCam)activeToken.Reference;
                //     if (Utilities.IsValid(_activeCam))
                //     {
                //         if (newCam == _activeCam && _activeCam.Rec)
                //         {
                //             //Skip disabling and re-enabling camera.
                //             return;
                //         }
                //         _activeCam.Rec = false;
                //     }
                //     _activeCam = newCam;
                //     if (Utilities.IsValid(_activeCam))
                //     {
                //         Debug.LogWarning($"Active Cam set to {_activeCam.name}");
                //         _activeCam.Rec = true;
                //     }
                // }
                // else
                // {
                //     Debug.LogWarning($"Warning: FanCam failed to set the active camera to {value} from manager. Assuming it's a timing issue, we'll wait for the FanCam's network update");
                //     Debug.LogWarning($"There are {fancams.Count} fan cams.");
                // }
            }
        }
        // FanCam _activeCam = null;
        // public FanCam ActiveCam
        // {
        //     get => _activeCam;
        // }

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

        // public void AddFanCam(FanCam newCam)
        // {
        //     if (fancams.ContainsKey(newCam.Id))
        //     {
        //         Debug.LogWarning($"[FANCAM] Fan cam with conflicting Id {newCam.Id} tried to add itself to the manager");
        //         return;
        //     }
        //     fancams.Add(newCam.Id, newCam);
        //     ActiveCamIndex = ActiveCamIndex;
        // }
        //
        // public void RemoveFanCam(FanCam oldCam)
        // {
        //     if (oldCam == ActiveCam && IsOwnerLocal())
        //     {
        //         ActiveCamIndex = 0;
        //     }
        //     fancams.Remove(oldCam.Id);
        //     ActiveCamIndex = ActiveCamIndex;
        // }

        DataDictionary playerTargets = new DataDictionary();
        public DataDictionary PlayerTargets
        {
            get => playerTargets;
        }

        public void AddPlayerTarget(FanCamPlayerTarget target)
        {
            playerTargets.Add(target.PlayerId, target);
        }

        public void RemovePlayerTarget(int playerId)
        {
            if (playerId < 0)
            {
                return;
            }
            playerTargets.Remove(playerId);
            var fanCamlist = fancams.GetValues();
            for (int i = 0; i < fanCamlist.Count; i++)
            {
                var fanCam = (FanCam)fanCamlist[i].Reference;
                if (Utilities.IsValid(fanCam) && fanCam.IsOwnerLocal() && fanCam.TargetPlayerId == playerId)
                {
                    fanCam.TargetPlayerId = -1001;
                }
            }
        }

        // DataList menus;
        // public void AddMenu(FanCamMenu menu)
        // {
        //     menus.Add(menu);
        // }
        // public void RemoveMenu(FanCamMenu menu)
        // {
        //     menus.Remove(menu);
        // }
        //
        // public override void OnDeserialization(VRC.Udon.Common.DeserializationResult result)
        // {
        //     for (int i = 0; i < menus.Count; i++)
        //     {
        //         if (menus.TryGetValue(i, TokenType.Reference, out DataToken menuToken))
        //         {
        //             ((FanCamMenu)menuToken.Reference).OnManagerUpdate();
        //         }
        //     }
        // }
        //
        // public override void OnPreSerialization()
        // {
        //     for (int i = 0; i < menus.Count; i++)
        //     {
        //         if (menus.TryGetValue(i, TokenType.Reference, out DataToken menuToken))
        //         {
        //             ((FanCamMenu)menuToken.Reference).OnManagerUpdate();
        //         }
        //     }
        // }
    }
}
