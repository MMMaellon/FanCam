
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

        VRCPlayerApi owner;
        public void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
            animator.SetBool(OwnerHash, IsOwnerLocal());
            if (Utilities.IsValid(menu))
            {
                menu.animator.SetBool(OwnerHash, IsOwnerLocal());
                menu.ownerNameTMP.text = owner.displayName;
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
                menu.ownerNameTMP.text = owner.displayName;
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

        // DataDictionary playerTargets = new DataDictionary();
        // public DataDictionary PlayerTargets
        // {
        //     get => playerTargets;
        // }

        // public void AddPlayerTarget(FanCamPlayerTarget target)
        // {
        //     playerTargets.Add(target.playerId, target);
        //     if (Utilities.IsValid(menu))
        //     {
        //         menu.PopulatePlayerTrackingDropdown();
        //     }
        // }
        //
        // public void RemovePlayerTarget(int playerId)
        // {
        //     if (playerId < 0)
        //     {
        //         return;
        //     }
        //     foreach (var fanCam in fanCams)
        //     {
        //         if (fanCam.IsOwnerLocal() && fanCam.TargetPlayerId == playerId)
        //         {
        //             fanCam.TargetPlayerId = -1001;
        //         }
        //     }
        //     playerTargets.Remove(playerId);
        //     if (Utilities.IsValid(menu))
        //     {
        //         menu.PopulatePlayerTrackingDropdown();
        //     }
        // }

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
            if (fanCam != EditFanCam)
            {
                return;
            }
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

        // int lastPreview = -1001;
        // [System.NonSerialized]
        // DataList previewList = new DataList();
        int previewCounter = 0;
        public void PreviewLoop()
        {
            // if (lastPreview == Time.renderedFrameCount)
            // {
            //     return;
            // }
            // if (!previewCamera.enabled || previewList.Count == 0)
            // {
            //     return;
            // }
            if (Utilities.IsValid(menu) && menu.AreCameraPreviewsVisible())
            {
                previewCamera.enabled = true;
                previewCounter = (previewCounter + 1) % fanCams.Length;
                fanCams[previewCounter].RenderPreview();
            }
            else
            if (Utilities.IsValid(HeldFanCam))
            {
                previewCamera.enabled = true;
                HeldFanCam.RenderPreview();
            }
            else if (Utilities.IsValid(EditFanCam) && menu.AreEditorControlsVisible())
            {
                previewCamera.enabled = true;
                EditFanCam.RenderPreview();
            }
            else
            {
                previewCamera.enabled = false;
                return;
            }
            // SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
            // previewCounter = (previewCounter + 1) % previewList.Count;
            // if (previewList.TryGetValue(previewCounter, TokenType.Reference, out var previewTargetRef))
            // {
            //     var previewTarget = (FanCam)previewTargetRef.Reference;
            //     previewTarget.RenderPreview();
            // }
            // previewCounter = (previewCounter + 1) % fanCams.Length;
            // fanCams[previewCounter].RenderPreview();
        }

        // public void AddToPreviewList(FanCam fanCam)
        // {
        //     // if (previewList.Contains(fanCam))
        //     // {
        //     //     return;
        //     // }
        //     previewList.Add(fanCam);
        //     previewCamera.enabled = true;
        //     SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
        // }

        // public void RemoveFromPreviewList(FanCam fanCam)
        // {
        //     // if (fanCam.Held)
        //     // {
        //     //     return;
        //     // }
        //     previewList.Remove(fanCam);
        //     if (previewList.Count == 0)
        //     {
        //         previewCamera.enabled = false;
        //     }
        // }

        // public void ClearPreviewList()
        // {
        //     previewList.Clear();
        // }

        // bool gridEnabled = false;
        // public void OnGridEnable()
        // {
        //     // foreach (var fanCam in fanCams)
        //     // {
        //     //     previewList.Add(fanCam);
        //     // }
        //     gridEnabled = true;
        //     // previewCamera.enabled = true;
        //     SendCustomEventDelayedFrames(nameof(PreviewLoop), 0);
        // }
        //
        // public void OnGridDisable()
        // {
        //     // foreach (var fanCam in fanCams)
        //     // {
        //     //     previewList.Remove(fanCam);
        //     // }
        //     // if (previewList.Count == 0)
        //     // {
        //     //     previewCamera.enabled = false;
        //     // }
        //     // previewList.Clear();
        //     gridEnabled = false;
        // }

        FanCam _heldFanCam;
        public FanCam HeldFanCam
        {
            get => _heldFanCam;
            set
            {
                if (Utilities.IsValid(_heldFanCam))
                {
                    _heldFanCam.StopZoom();
                }
                _heldFanCam = value;
                if (Utilities.IsValid(EditFanCam))
                {
                    EditFanCam.UpdatePreview();
                }
            }
        }
        FanCam _editFanCam;
        public FanCam EditFanCam
        {
            get => _editFanCam;
            set
            {
                if (Utilities.IsValid(_editFanCam))
                {
                    _editFanCam.StopZoom();
                }
                _editFanCam = value;
                if (Utilities.IsValid(value))
                {
                    value.UpdatePreview();
                }
            }
        }

        public void Update()
        {
            PreviewLoop();
            if (Utilities.IsValid(HeldFanCam))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    HeldFanCam.ZoomIn();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    HeldFanCam.ZoomOut();
                }
                else if (Input.GetKeyUp(KeyCode.Q))
                {
                    HeldFanCam.StopZoom();
                }
                else if (Input.GetKeyUp(KeyCode.E))
                {
                    HeldFanCam.StopZoom();
                }
            }
            if (Utilities.IsValid(EditFanCam) && EditFanCam.Edit)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.PageUp))
                {
                    EditFanCam.ZoomIn();
                    return;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.PageDown))
                {
                    EditFanCam.ZoomOut();
                    return;
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.PageUp))
                {
                    EditFanCam.StopZoom();
                    return;
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.PageDown))
                {
                    EditFanCam.StopZoom();
                    return;
                }
            }
            if (!IsOwnerLocal())
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                ActiveCam = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ActiveCam = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ActiveCam = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ActiveCam = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                ActiveCam = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                ActiveCam = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                ActiveCam = 6;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            {
                ActiveCam = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                ActiveCam = 8;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
            {
                ActiveCam = 9;
            }
            else if (_cam >= 0 && _cam < fanCams.Length)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.PageUp))
                {
                    fanCams[_cam].ZoomIn();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.PageDown))
                {
                    fanCams[_cam].ZoomOut();
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.PageUp))
                {
                    fanCams[_cam].StopZoom();
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.PageDown))
                {
                    fanCams[_cam].StopZoom();
                }
            }

            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                SpeedUp();
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SpeedDown();
            }
        }

        [UdonSynced]
        [FieldChangeCallback(nameof(Speed))]
        public float _speed = 120;
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                foreach (var fanCam in fanCams)
                {
                    fanCam.dollyTrack.Speed = value;
                }
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        public void SpeedUp()
        {
            Speed = Mathf.Min(10f, Speed + 0.25f);
        }

        public void SpeedDown()
        {
            Speed = Mathf.Max(0.25f, Speed - 0.25f);
        }


        public override void InputLookVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (!Networking.LocalPlayer.IsUserInVR() || !Utilities.IsValid(HeldFanCam))
            {
                return;
            }
            if (value > 0.5f)
            {
                HeldFanCam.ZoomIn();
            }
            else if (value < -0.5f)
            {
                HeldFanCam.ZoomOut();
            }
            else
            {
                HeldFanCam.StopZoom();
            }
        }
    }
}
