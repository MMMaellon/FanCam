
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
        public FanCamManager manager;
        public TMP_Dropdown editorDropdown;
        public TMP_Dropdown playerTrackingDropdown;
        public Animator animator;
        readonly int cameraControlsParameter = Animator.StringToHash("camera controls");
        public readonly int PlayerTrackingHash = Animator.StringToHash("player tracking");
        public TextMeshProUGUI ownerNameTMP;
        public RawImage editPreview;
        public Slider zoomSlider;
        public Slider speedSlider;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif
        public void OnEnable()
        {
            if (Utilities.IsValid(manager.Owner))
            {
                ownerNameTMP.text = manager.Owner.displayName;
            }
            else
            {
                ownerNameTMP.text = "";
            }
            OnChangeEditorDropdown();
            animator.SetBool(manager.DirectorHash, manager.IsDirector());
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
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public bool AreCameraPreviewsVisible()
        {
            return animator.GetInteger(cameraControlsParameter) == 2 && gameObject.activeInHierarchy && enabled;
        }

        public bool AreEditorControlsVisible()
        {
            return animator.GetInteger(cameraControlsParameter) == 1 && gameObject.activeInHierarchy && enabled;
        }

        FanCam _editorFanCam;
        public FanCam EditorFanCam
        {
            get => _editorFanCam;
            set
            {
                if (Utilities.IsValid(_editorFanCam))
                {
                    _editorFanCam.Edit = false;
                }
                _editorFanCam = value;
                if (!Utilities.IsValid(_editorFanCam))
                {
                    editPreview.texture = null;
                    UnSetPlayerTrackingDropdown();
                    return;
                }
                editPreview.texture = _editorFanCam.localPreview;
                _editorFanCam.FillEditor();
                if (_editorFanCam.IsOwnerLocal())
                {
                    _editorFanCam.Edit = true;
                }
            }
        }

        public void OnChangeEditorDropdown()
        {
            if (editorDropdown.value >= 0 && editorDropdown.value < manager.fanCams.Length)
            {
                EditorFanCam = manager.fanCams[editorDropdown.value];
            }
            else
            {
                EditorFanCam = null;
            }
        }

        public TextMeshProUGUI editorOwnerTMP;

        public void SetPlayerTrackingDropdown(int playerId)
        {
            for (int i = 0; i < playerIdCache.Length; i++)
            {
                if (Utilities.IsValid(playerIdCache[i]) && playerIdCache[i] == playerId)
                {
                    playerTrackingDropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
            UnSetPlayerTrackingDropdown();
        }
        public void UnSetPlayerTrackingDropdown()
        {
            playerTrackingDropdown.SetValueWithoutNotify(playerIdCache.Length + 1);
        }

        public Vector3 editorTeleportOffset = new Vector3(0, 1f, 1f);
        public void TeleportBtn()
        {
            if (Utilities.IsValid(_editorFanCam) && _editorFanCam.IsOwnerLocal() && _editorFanCam.Edit)
            {
                var position = Networking.LocalPlayer.GetPosition();
                var rotation = Networking.LocalPlayer.GetRotation();
                if (_editorFanCam.Dolly)
                {
                    var points = _editorFanCam.dollyTrack.points;
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].sync.TeleportToWorldSpace(position + rotation * editorTeleportOffset + rotation * Vector3.right * (i - points.Length / 2f), rotation, Vector3.zero, Vector3.zero);
                    }
                    _editorFanCam.dollyTrack.targetBall.Respawn();
                }
                else
                {
                    _editorFanCam.pickupControllerPickup.TeleportToWorldSpace(position + rotation * editorTeleportOffset, rotation, Vector3.zero, Vector3.zero);
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
            Debug.LogWarning("SwitchBtn");
            if (!manager.IsDirector())
            {
                Debug.LogWarning("Not director");
                return;
            }
            if (!Utilities.IsValid(EditorFanCam))
            {
                Debug.LogWarning("Invalid editor fan cam");
                return;
            }
            Debug.LogWarning("Setting cam to " + EditorFanCam.Id);
            manager.ActiveCam = EditorFanCam.Id;
        }

        public void OnChangeZoomSlider()
        {
            if (!Utilities.IsValid(EditorFanCam))
            {
                zoomSlider.SetValueWithoutNotify(0);
                return;
            }
            if (!EditorFanCam.IsOwnerLocal() || !_editorFanCam.Edit)
            {
                zoomSlider.SetValueWithoutNotify(_editorFanCam.Zoom);
                return;
            }
            _editorFanCam.Zoom = zoomSlider.value;
            zoomSlider.SetValueWithoutNotify(_editorFanCam.Zoom);
        }
        public void OnChangeSpeedSlider()
        {
            if (!Utilities.IsValid(EditorFanCam))
            {
                speedSlider.SetValueWithoutNotify(0);
                return;
            }
            if (!EditorFanCam.IsOwnerLocal() || !_editorFanCam.Edit)
            {
                speedSlider.SetValueWithoutNotify(_editorFanCam.Speed);
                return;
            }
            _editorFanCam.Speed = speedSlider.value;
            speedSlider.SetValueWithoutNotify(_editorFanCam.Speed);
        }

        public void OnChangePlayerTrackingDropdown()
        {
            if (!Utilities.IsValid(EditorFanCam))
            {
                UnSetPlayerTrackingDropdown();
                return;
            }
            if (!EditorFanCam.IsOwnerLocal() || !_editorFanCam.Edit)
            {
                SetPlayerTrackingDropdown(EditorFanCam.TargetPlayerId);
                return;
            }
            if (playerTrackingDropdown.value >= 0 && playerTrackingDropdown.value < playerIdCache.Length)
            {
                _editorFanCam.TargetPlayerId = playerIdCache[playerTrackingDropdown.value];
            }
            else
            {
                _editorFanCam.TargetPlayerId = -1001;
                UnSetPlayerTrackingDropdown();
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
            if (Utilities.IsValid(EditorFanCam))
            {
                SetPlayerTrackingDropdown(EditorFanCam.TargetPlayerId);
            }
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
            if (!Utilities.IsValid(_editorFanCam))
            {
                return;
            }
            _editorFanCam.ToggleDolly();
        }

        public void EditBtn()
        {
            Debug.LogWarning("Edit Button");
            if (!Utilities.IsValid(_editorFanCam))
            {
                Debug.LogWarning("no cam to edit");
                return;
            }
            if (!_editorFanCam.IsOwnerLocal())
            {
                if (_editorFanCam.pickupControllerPickup.IsHeld())
                {
                    return;
                }
                Networking.SetOwner(Networking.LocalPlayer, _editorFanCam.gameObject);
            }
            _editorFanCam.Edit = !_editorFanCam.Edit;
        }
    }
}
