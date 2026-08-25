
using Cinemachine.Utility;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;


namespace MMMaellon.FanCam
{
    [RequireComponent(typeof(Animator))]
    public class FanCamMenu : UdonSharpBehaviour
    {
        private readonly int OwnerHash = Animator.StringToHash("owner");
        private readonly int EditHash = Animator.StringToHash("edit");
        private readonly int EditOwnerHash = Animator.StringToHash("edit owner");
        private readonly int EditDollyHash = Animator.StringToHash("edit dolly");
        public FanCamManager manager;
        public TMP_Dropdown editorDropdown;
        public TMP_Dropdown playerTrackingDropdown;
        public Animator animator;
        public TextMeshProUGUI ownerNameTMP;
        public RawImage editPreview;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif
        public void Start()
        {
            manager.menu = this;
            //just to force it to update
            editorDropdown.value = 1;
            if (Utilities.IsValid(manager.Owner))
            {
                ownerNameTMP.text = manager.Owner.displayName;
            }
        }

        public void OnEnable()
        {
            if (Utilities.IsValid(manager.Owner))
            {
                ownerNameTMP.text = manager.Owner.displayName;
            }
            UpdateEditor();
            animator.SetBool(OwnerHash, manager.IsOwnerLocal());
        }

        public void Cam0()
        {
            manager.Cam0();
        }
        public void Cam1()
        {
            manager.Cam1();
        }
        public void Cam2()
        {
            manager.Cam2();
        }
        public void Cam3()
        {
            manager.Cam3();
        }
        public void Cam4()
        {
            manager.Cam4();
        }
        public void Cam5()
        {
            manager.Cam5();
        }
        public void Cam6()
        {
            manager.Cam6();
        }
        public void Cam7()
        {
            manager.Cam7();
        }
        public void Cam8()
        {
            manager.Cam8();
        }
        public void Cam9()
        {
            manager.Cam9();
        }

        public string cameraControlsParameter = "camera controls";
        public void CameraSwitcherBtn()
        {
            if (animator.GetInteger(cameraControlsParameter) == 2)
            {
                animator.SetInteger(cameraControlsParameter, 0);
            }
            else
            {
                animator.SetInteger(cameraControlsParameter, 2);
            }
        }

        public void OperatorControlBtn()
        {
            if (animator.GetInteger(cameraControlsParameter) == 1)
            {
                animator.SetInteger(cameraControlsParameter, 0);
            }
            else
            {
                animator.SetInteger(cameraControlsParameter, 1);
            }
        }

        public void BecomeDirectorBtn()
        {
            manager.TakeOwnership();
        }

        public bool AreCameraPreviewsVisible()
        {
            return animator.GetInteger(cameraControlsParameter) == 2 && gameObject.activeInHierarchy && enabled;
        }

        public bool AreEditorControlsVisible()
        {
            return animator.GetInteger(cameraControlsParameter) == 1 && gameObject.activeInHierarchy && enabled;
        }

        [System.NonSerialized]
        public FanCam editorFanCam;
        public void OnChangeEditorDropdown()
        {
            if (editorDropdown.value >= 0 && editorDropdown.value < manager.fanCams.Length)
            {
                editorFanCam = manager.fanCams[editorDropdown.value];
            }
            else
            {
                editorFanCam = null;
            }
            UpdateEditor();
        }

        public TextMeshProUGUI editorOwnerTMP;
        public void UpdateEditor()
        {
            if (!Utilities.IsValid(editorFanCam))
            {
                animator.SetBool(EditHash, false);
                editPreview.texture = null;
                return;
            }
            if (editorFanCam.Edit)
            {
                manager.EditFanCam = editorFanCam;
            }
            editPreview.texture = editorFanCam.localPreview;
            animator.SetBool(EditHash, editorFanCam.Edit);
            animator.SetBool(EditOwnerHash, editorFanCam.IsOwnerLocal());
            animator.SetBool(EditDollyHash, editorFanCam.Dolly);
            if (Utilities.IsValid(editorFanCam.Owner))
            {
                editorOwnerTMP.text = editorFanCam.Owner.displayName;
            }
            SetPlayerTrackingDropdown();
        }

        public void SetPlayerTrackingDropdown()
        {
            if (Utilities.IsValid(editorFanCam) && Utilities.IsValid(editorFanCam.playerTarget.Target))
            {
                for (int i = 0; i < playerIdCache.Length; i++)
                {
                    if (Utilities.IsValid(playerIdCache[i]) && playerIdCache[i] == editorFanCam.TargetPlayerId)
                    {
                        playerTrackingDropdown.SetValueWithoutNotify(i);
                        return;
                    }
                }
            }
            playerTrackingDropdown.SetValueWithoutNotify(playerIdCache.Length + 1);
        }

        public Vector3 editorTeleportOffset = new Vector3(0, 1f, 1f);
        public void TeleportBtn()
        {
            if (Utilities.IsValid(editorFanCam) && editorFanCam.IsOwnerLocal() && editorFanCam.Edit)
            {
                var position = Networking.LocalPlayer.GetPosition();
                var rotation = Networking.LocalPlayer.GetRotation();
                if (editorFanCam.Dolly)
                {
                    var points = editorFanCam.dollyTrack.points;
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].sync.TeleportToWorldSpace(position + rotation * editorTeleportOffset + rotation * Vector3.right * (i - points.Length / 2f), rotation, Vector3.zero, Vector3.zero);
                    }
                    editorFanCam.dollyTrack.targetBall.Respawn();
                }
                else
                {
                    editorFanCam.pickupControllerPickup.TeleportToWorldSpace(position + rotation * editorTeleportOffset, rotation, Vector3.zero, Vector3.zero);
                }
            }
        }

        public void ResetBtn()
        {
            // if (Utilities.IsValid(editorFanCam) && editorFanCam.IsOwnerLocal() && editorFanCam.Edit)
            // {
            //     var position = Networking.LocalPlayer.GetPosition();
            //     var rotation = Networking.LocalPlayer.GetRotation();
            //     if (editorFanCam.Dolly)
            //     {
            //         var points = editorFanCam.dollyTrack.points;
            //         for (int i = 0; i < points.Length; i++)
            //         {
            //             points[i].sync.TeleportToWorldSpace(position + rotation * editorTeleportOffset + rotation * Vector3.right * (i - points.Length / 2f), rotation, Vector3.zero, Vector3.zero);
            //         }
            //     }
            //     else
            //     {
            //         editorFanCam.pickupControllerPickup.TeleportToWorldSpace(position + rotation * editorTeleportOffset, rotation, Vector3.zero, Vector3.zero);
            //     }
            // }
        }
        public void SwitchBtn()
        {
            if (!manager.IsOwnerLocal())
            {
                return;
            }
            if (!Utilities.IsValid(editorFanCam))
            {
                return;
            }
            manager.ActiveCam = editorFanCam.Id;
        }

        public void OnChangePlayerTrackingDropdown()
        {
            if (!Utilities.IsValid(editorFanCam) || !editorFanCam.IsOwnerLocal() || !editorFanCam.Edit)
            {
                return;
            }
            if (playerTrackingDropdown.value >= 0 && playerTrackingDropdown.value < playerIdCache.Length)
            {
                editorFanCam.TargetPlayerId = playerIdCache[playerTrackingDropdown.value];
            }
            else
            {
                editorFanCam.TargetPlayerId = -1001;
            }
        }

        int[] playerIdCache = { };
        string[] playerTargetOptions = { };
        public void PopulatePlayerTrackingDropdown()
        {
            playerTrackingDropdown.ClearOptions();
            var players = VRCPlayerApi.GetPlayers();
            playerIdCache = new int[players.Length];
            playerTargetOptions = new string[players.Length + 1];
            for (int i = 0; i < playerIdCache.Length; i++)
            {
                if (Utilities.IsValid(players[i]))
                {
                    playerIdCache[i] = players[i].playerId;
                    playerTargetOptions[i] = players[i].displayName;
                }
                else
                {
                    playerIdCache[i] = -1001;
                    playerTargetOptions[i] = "________";
                }
            }
            playerTargetOptions[playerTargetOptions.Length - 1] = "No Player Tracking";
            playerTrackingDropdown.AddOptions(playerTargetOptions);
            SetPlayerTrackingDropdown();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            PopulatePlayerTrackingDropdown();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            PopulatePlayerTrackingDropdown();
        }

        public void DollyBtn()
        {
            if (!Utilities.IsValid(editorFanCam) || !editorFanCam.IsOwnerLocal() || !editorFanCam.Edit)
            {
                return;
            }
            editorFanCam.ToggleDolly();
        }

        public void EditBtn()
        {
            if (!Utilities.IsValid(editorFanCam))
            {
                return;
            }
            if (!editorFanCam.IsOwnerLocal())
            {
                if (editorFanCam.pickupControllerPickup.IsHeld())
                {
                    return;
                }
                Networking.SetOwner(Networking.LocalPlayer, editorFanCam.gameObject);
            }
            editorFanCam.Edit = !editorFanCam.Edit;
        }
    }
}
