
using Cinemachine;
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
        public RenderTexture fullResRenderTexture;
        public Camera realCamera;
        public Camera previewCamera;
        public Animator animator;
        private readonly int OwnerHash = Animator.StringToHash("owner");
        private readonly int camParameterHash = Animator.StringToHash("cam");
        CinemachineVirtualCameraBase switcher;
        public FanCam[] fanCams;
        public FanCamMenu menu;
        public FanCamCameraMenu camMenu;

        VRCPlayerApi owner;
        public void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
            animator.SetBool(OwnerHash, IsOwnerLocal());
            if (Utilities.IsValid(menu))
            {
                menu.animator.SetBool(OwnerHash, IsOwnerLocal());
            }
            ActiveCam = ActiveCam;
        }
        public VRCPlayerApi Owner
        {
            get => owner;
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            owner = player;
            animator.SetBool(OwnerHash, IsOwnerLocal());
            if (Utilities.IsValid(menu))
            {
                menu.animator.SetBool(OwnerHash, IsOwnerLocal());
            }
        }
        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(owner) && owner.isLocal;
        }
        [UdonSynced]
        [System.NonSerialized]
        [FieldChangeCallback(nameof(ActiveCam))]
        int _cam = 0;

        public int ActiveCam
        {
            get => _cam;
            set
            {
                if (_cam >= 0 && _cam < fanCams.Length && Utilities.IsValid(fanCams[_cam]))
                {
                    fanCams[_cam].Rec = false;
                }
                _cam = value;
                animator.SetInteger(camParameterHash, value);
                if (Utilities.IsValid(menu))
                {
                    menu.animator.SetInteger(camParameterHash, value);
                }
                if (_cam >= 0 && _cam < fanCams.Length && Utilities.IsValid(fanCams[_cam]))
                {
                    fanCams[_cam].Rec = true;
                }
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }

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
        }

        public void Cam1()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 1;
        }
        public void Cam2()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 2;
        }
        public void Cam3()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 3;
        }
        public void Cam4()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 4;
        }
        public void Cam5()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 5;
        }
        public void Cam6()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 6;
        }
        public void Cam7()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 7;
        }
        public void Cam8()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 8;
        }
        public void Cam9()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 9;
        }
        public void Cam0()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            ActiveCam = 0;
        }

        public void UpdateEdit(FanCam fanCam)
        {

        }

        public void TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void Setup()
        {
            for (int i = 0; i < fanCams.Length; i++)
            {
                fanCams[i].Id = i;
                fanCams[i].manager = this;
            }
        }

        int lastPreview = -1001;
        [System.NonSerialized]
        DataList previewList = new DataList();
        int previewCounter = 0;
        public void PreviewLoop()
        {
            if (lastPreview == Time.renderedFrameCount)
            {
                return;
            }
            if (!previewCamera.enabled)
            {
                return;
            }
            SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
            previewCounter = (previewCounter + 1) % previewList.Count;
            if (previewList.TryGetValue(previewCounter, TokenType.Reference, out var previewTargetRef))
            {
                var previewTarget = (FanCam)previewTargetRef.Reference;
                previewTarget.RenderPreview();
            }
        }

        public void AddToPreviewList(FanCam fanCam)
        {
            // if (previewList.Contains(fanCam))
            // {
            //     return;
            // }
            previewList.Add(fanCam);
            previewCamera.enabled = true;
            SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
        }

        public void RemoveFromPreviewList(FanCam fanCam)
        {
            // if (fanCam.Held)
            // {
            //     return;
            // }
            previewList.Remove(fanCam);
            if (previewList.Count == 0)
            {
                previewCamera.enabled = false;
            }
        }

        public void OnGridEnable()
        {
            foreach (var fanCam in fanCams)
            {
                previewList.Add(fanCam);
            }
            previewCamera.enabled = true;
            SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
        }

        public void OnGridDisable()
        {
            foreach (var fanCam in fanCams)
            {
                previewList.Remove(fanCam);
            }
            if (previewList.Count == 0)
            {
                previewCamera.enabled = false;
            }
        }
    }
}
