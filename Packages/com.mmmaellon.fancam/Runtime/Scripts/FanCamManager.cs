
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
        public readonly int DirectorHash = Animator.StringToHash("director");
        public readonly int camParameterHash = Animator.StringToHash("cam");
        CinemachineVirtualCameraBase switcher;
        public FanCam[] fanCams;
        public FanCamMenu menu;

        VRCPlayerApi owner;
        public void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
            animator.SetBool(DirectorHash, IsDirector());
            if (Utilities.IsValid(menu))
            {
                menu.animator.SetBool(DirectorHash, IsDirector());
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
            animator.SetBool(DirectorHash, IsDirector());
            if (Utilities.IsValid(menu))
            {
                menu.animator.SetBool(DirectorHash, IsDirector());
                menu.ownerNameTMP.text = owner.displayName;
            }
        }
        public bool IsDirector()
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
                if (IsDirector())
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
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 1;
        }
        public void Cam2()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 2;
        }
        public void Cam3()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 3;
        }
        public void Cam4()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 4;
        }
        public void Cam5()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 5;
        }
        public void Cam6()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 6;
        }
        public void Cam7()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 7;
        }
        public void Cam8()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 8;
        }
        public void Cam9()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 9;
        }
        public void Cam0()
        {
            if (!IsDirector())
            {
                return;
            }
            ActiveCam = 0;
        }

        public void Setup()
        {
            for (int i = 0; i < fanCams.Length; i++)
            {
                fanCams[i].Id = i;
                fanCams[i].manager = this;
            }
        }

        int previewCounter = 0;
        public void PreviewLoop()
        {
            if (Utilities.IsValid(menu) && menu.AreCameraPreviewsVisible())
            {
                previewCamera.enabled = true;
                previewCounter = (previewCounter + 1) % fanCams.Length;
                fanCams[previewCounter].RenderPreview();
            }
            else if (Utilities.IsValid(HeldFanCam))
            {
                Debug.LogWarning("Rendering held cam");
                previewCamera.enabled = true;
                HeldFanCam.RenderPreview();
                if (Utilities.IsValid(menu) && menu.EditorFanCam != HeldFanCam)
                {
                    Debug.LogWarning("disabling edit preview");
                    menu.editPreview.enabled = false;
                }
            }
            else if (Utilities.IsValid(menu) && Utilities.IsValid(menu.EditorFanCam) && menu.AreEditorControlsVisible())
            {
                previewCamera.enabled = true;
                menu.editPreview.enabled = true;
                menu.EditorFanCam.RenderPreview();
            }
            else
            {
                previewCamera.enabled = false;
                return;
            }
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
                //update preview handled by fancam's Held
                // if (Utilities.IsValid(menu) && Utilities.IsValid(menu.EditorFanCam))
                // {
                //     menu.EditorFanCam.UpdatePreviewMesh();
                // }
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
                else if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
                {
                    HeldFanCam.StopZoom();
                }
            }
            else
            if (Utilities.IsValid(menu) && Utilities.IsValid(menu.EditorFanCam) && menu.EditorFanCam.Edit && menu.EditorFanCam.IsOwnerLocal())
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    menu.EditorFanCam.ZoomIn();
                    return;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    menu.EditorFanCam.ZoomOut();
                    return;
                }
                else if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
                {
                    menu.EditorFanCam.StopZoom();
                    return;
                }
            }
            if (IsDirector())
            {
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
                    else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        fanCams[_cam].SpeedUp();
                    }
                    else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                    {
                        fanCams[_cam].SpeedDown();
                    }
                }
            }
        }

        // [UdonSynced]
        // [FieldChangeCallback(nameof(Speed))]
        // public float _speed = 4f;
        // public float Speed
        // {
        //     get => _speed;
        //     set
        //     {
        //         _speed = value;
        //         foreach (var fanCam in fanCams)
        //         {
        //             fanCam.dollyTrack.Speed = value;
        //         }
        //         if (IsDirector())
        //         {
        //             RequestSerialization();
        //         }
        //     }
        // }

        public override void InputLookVertical(float value, VRC.Udon.Common.UdonInputEventArgs args)
        {
            if (!Networking.LocalPlayer.IsUserInVR())
            {
                return;
            }
            if (Utilities.IsValid(HeldFanCam))
            {
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
            else if (Utilities.IsValid(menu) && Utilities.IsValid(menu.EditorFanCam))
            {
                if (value > 0.5f)
                {
                    menu.EditorFanCam.ZoomIn();
                }
                else if (value < -0.5f)
                {
                    menu.EditorFanCam.ZoomOut();
                }
                else
                {
                    menu.EditorFanCam.StopZoom();
                }
            }
        }
    }
}
