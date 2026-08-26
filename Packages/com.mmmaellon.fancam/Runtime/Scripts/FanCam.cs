
using Cinemachine;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;



#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MMMaellon.FanCam
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(Animator))]
    public class FanCam : UdonSharpBehaviour
    {
        public RenderTexture localPreview;
        public Transform localPreviewParent;
        public MeshRenderer previewMesh;
        public int previewMeshMaterialSlot = 1;
        public Texture placeholderTexture;
        public FanCamManager manager;
        public CinemachineVirtualCamera defaultCamera;
        public CinemachineVirtualCamera dollyCamera;
        public Animator animator;
        public readonly int OwnerHash = Animator.StringToHash("owner");
        public readonly int EditHash = Animator.StringToHash("edit");
        public readonly int DollyHash = Animator.StringToHash("dolly");
        public readonly int RecHash = Animator.StringToHash("rec");
        public readonly int ZoomHash = Animator.StringToHash("zoom");
        public readonly int PlayerTrackingHash = Animator.StringToHash("player tracking");
        public SmartObjectSync pickupControllerPickup;
        public SmartObjectSync[] dollyTrackPickups;
        public FanCamTrackFollower dollyTrack;
        public FanCamPlayerTarget playerTarget;
        public Transform FOVTracker;

        [SerializeField]
        [FieldChangeCallback(nameof(Id))]
        int _id = -1001;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
            }
        }

        public bool Rec
        {
            get => manager.ActiveCam == Id;
            set
            {
                animator.SetBool(RecHash, value);
                // UpdatePreviewMesh();
                if (Utilities.IsValid(manager.menu) && manager.menu.EditorFanCam == this)
                {
                    manager.menu.animator.SetBool(RecHash, value);
                }
            }
        }

        // [UdonSynced]
        [FieldChangeCallback(nameof(Edit))]
        [System.NonSerialized]
        public bool _edit = false;
        public bool Edit
        {
            get => _edit;
            set
            {
                _edit = value && IsOwnerLocal();
                animator.SetBool(EditHash, _edit);
                // UpdatePreviewMesh();
                if (!_edit)
                {
                    if (Dolly)
                    {
                        foreach (var pickup in dollyTrackPickups)
                        {
                            pickup.pickup.Drop();
                        }
                    }
                    else
                    {
                        pickupControllerPickup.pickup.Drop();
                    }
                    if (IsOwnerLocal() && zoomSpeed != 0)
                    {
                        StopZoom();
                    }
                }
                if (Utilities.IsValid(manager.menu) && manager.menu.EditorFanCam == this)
                {
                    manager.menu.animator.SetBool(EditHash, _edit);
                }
            }
        }

        [UdonSynced]
        [FieldChangeCallback(nameof(Dolly))]
        public bool _dolly = false;
        public bool Dolly
        {
            get => _dolly;
            set
            {
                _dolly = value;
                if (value)
                {
                    pickupControllerPickup.pickup.Drop();
                    dollyTrack.Speed = Speed;
                    dollyTrack.StartTrack();
                }
                else
                {
                    foreach (var pickup in dollyTrackPickups)
                    {
                        pickup.pickup.Drop();
                    }
                    dollyTrack.StopTrack();
                }
                animator.SetBool(DollyHash, value);
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
                if (Utilities.IsValid(manager.menu) && manager.menu.EditorFanCam == this)
                {
                    manager.menu.animator.SetBool(DollyHash, value);
                }
            }
        }

        public void ToggleDolly()
        {
            if (!IsOwnerLocal() || !Edit)
            {
                return;
            }
            Dolly = !Dolly;
        }
        // public float duration = 4f;
        [UdonSynced]
        [FieldChangeCallback(nameof(Speed))]
        public float _speed = 1f;
        public float Speed
        {
            get => _speed;
            set
            {
                // Debug.LogWarning("Speed change");
                // var elapsed = pathHandle.Elapsed % duration;
                // int loopCount = Mathf.FloorToInt(pathHandle.Elapsed / duration);
                // var elapsedRatio = elapsed / duration;
                // duration = 240f / value;
                // if (pathHandle.IsPlaying)
                // {
                //     Debug.LogWarning("We were playing");
                //     pathHandle.SetDuration(duration);
                //     pathHandle.Goto(elapsedRatio * duration, true);
                //     // if (loopCount % 2 == 1)
                //     // {
                //     //     pathHandle.Flip();
                //     //     pathHandle.SetLoops(-1, VRCTweenLoopType.Yoyo);
                //     // }
                // }
                _speed = Mathf.Clamp(value, 0.1f, 10f);
                dollyTrack.Speed = _speed;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
                if (Utilities.IsValid(manager.menu) && manager.menu.EditorFanCam == this)
                {
                    manager.menu.speedSlider.SetValueWithoutNotify(Speed);
                }
            }
        }

        public void SpeedUp()
        {
            if (!Dolly)
            {
                return;
            }
            if (!IsOwnerLocal())
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            Speed += 0.1f;
        }

        public void SpeedDown()
        {
            if (!Dolly)
            {
                return;
            }
            if (!IsOwnerLocal())
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            Speed -= 0.1f;
        }


        VRCPlayerApi _owner;
        public void OnEnable()
        {
            Owner = Networking.GetOwner(gameObject);
            // UpdatePreviewMesh();
            Edit = Edit;
            Dolly = Dolly;
        }

        public void FillEditor()
        {
            if (Utilities.IsValid(manager.menu))
            {
                manager.menu.animator.SetBool(OwnerHash, IsOwnerLocal());
                manager.menu.animator.SetBool(RecHash, Rec);
                manager.menu.animator.SetBool(EditHash, Edit);
                manager.menu.animator.SetBool(DollyHash, Dolly);
                if (Utilities.IsValid(Owner))
                {
                    manager.menu.editorOwnerTMP.text = Owner.displayName;
                }
                else
                {
                    manager.menu.editorOwnerTMP.text = "";
                }
                manager.menu.SetPlayerTrackingDropdown(TargetPlayerId);
                manager.menu.zoomSlider.SetValueWithoutNotify(Zoom);
                manager.menu.speedSlider.SetValueWithoutNotify(Speed);
            }
        }

        public VRCPlayerApi Owner
        {
            get => _owner;
            set
            {
                _owner = value;
                if (IsOwnerLocal())
                {
                    animator.SetBool(OwnerHash, true);
                    if (Utilities.IsValid(manager.menu))
                    {
                        manager.menu.animator.SetBool(OwnerHash, true);
                        if (Utilities.IsValid(_owner))
                        {
                            manager.menu.editorOwnerTMP.text = _owner.displayName;
                        }
                    }
                }
                else
                {
                    Edit = false;
                    animator.SetBool(OwnerHash, false);
                    if (Utilities.IsValid(manager.menu))
                    {
                        manager.menu.animator.SetBool(OwnerHash, false);
                        if (Utilities.IsValid(_owner))
                        {
                            manager.menu.editorOwnerTMP.text = "";
                        }
                    }
                }
            }
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            Owner = player;
            if (IsOwnerLocal() && zoomSpeed != 0)
            {
                StopZoom();
            }
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(_owner) && _owner.isLocal;
        }

        public void EnableCam()
        {
            Networking.SetOwner(Networking.LocalPlayer, manager.gameObject);
            manager.ActiveCam = Id;
        }

        public void EnableEdit()
        {
            if (!IsOwnerLocal())
            {
                return;
            }
            Edit = true;
        }

        public void DisableEdit()
        {
            Edit = false;
        }

        MaterialPropertyBlock propertyBlock;

        bool _held = false;
        public bool Held
        {
            get => _held;
            set
            {
                _held = value;
                if (value)
                {
                    manager.HeldFanCam = this;
                }
                else if (manager.HeldFanCam == this)
                {
                    manager.HeldFanCam = null;
                }
                _held = true;
                // UpdatePreviewMesh();
            }
        }
        public void OnPickupListener(FanCamPickupListener listener)
        {
            if (listener.gameObject == pickupControllerPickup.gameObject && pickupControllerPickup.IsLocalOwner())
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            Held = true;
        }

        public void OnDropListener(FanCamPickupListener listener)
        {
            Held = false;
        }

        public void SetRecPreviewMesh()
        {
            if (!Utilities.IsValid(propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                previewMesh.GetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            propertyBlock.SetTexture("_MainTex", manager.fullResRenderTexture);
            previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
        }
        public void SetPreviewMesh()
        {
            if (!Utilities.IsValid(propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                previewMesh.GetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            propertyBlock.SetTexture("_MainTex", localPreview);
            previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
        }
        public void ClearPreviewMesh()
        {
            if (!Utilities.IsValid(propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                previewMesh.GetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            propertyBlock.SetTexture("_MainTex", placeholderTexture);
            previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif 

        Transform defaultStartTarget;
        Transform dollyStartTarget;
        [UdonSynced]
        [FieldChangeCallback(nameof(TargetPlayerId))]
        int targetId = -1001;
        bool firstTarget = true;
        public int TargetPlayerId
        {
            get => targetId;
            set
            {
                if (firstTarget)
                {
                    defaultStartTarget = defaultCamera.LookAt;
                    dollyStartTarget = dollyCamera.LookAt;
                    firstTarget = false;
                }
                targetId = value;

                playerTarget.Target = VRCPlayerApi.GetPlayerById(value);
                if (Utilities.IsValid(playerTarget.Target))
                {
                    defaultCamera.LookAt = playerTarget.transform;
                    dollyCamera.LookAt = playerTarget.transform;
                    animator.SetBool(PlayerTrackingHash, true);
                    if (Utilities.IsValid(manager.menu))
                    {
                        manager.menu.animator.SetBool(PlayerTrackingHash, true);
                        manager.menu.SetPlayerTrackingDropdown(value);
                    }
                }
                else
                {
                    playerTarget.Target = null;
                    targetId = -1001;
                    defaultCamera.LookAt = defaultStartTarget;
                    dollyCamera.LookAt = dollyStartTarget;
                    animator.SetBool(PlayerTrackingHash, false);
                    if (Utilities.IsValid(manager.menu))
                    {
                        manager.menu.animator.SetBool(PlayerTrackingHash, false);
                        manager.menu.UnSetPlayerTrackingDropdown();
                    }
                }
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }

        // public FanCamPlayerTarget FindPlayerTarget(VRCPlayerApi player)
        // {
        //     var objects = Networking.GetPlayerObjects(player);
        //     for (int i = 0; i < objects.Length; i++)
        //     {
        //         if (!Utilities.IsValid(objects[i])) continue;
        //         FanCamPlayerTarget foundScript = objects[i].GetComponentInChildren<FanCamPlayerTarget>();
        //         if (Utilities.IsValid(foundScript)) return foundScript;
        //     }
        //     return null;
        // }

        // public void TrackPlayerTarget(FanCamPlayerTarget newTarget)
        // {
        //     if (targetId < 0)
        //     {
        //         defaultStartTarget = defaultCamera.LookAt;
        //         dollyStartTarget = dollyCamera.LookAt;
        //     }
        //     if (Utilities.IsValid(newTarget))
        //     {
        //         targetId = newTarget.PlayerId;
        //         target = newTarget;
        //         defaultCamera.LookAt = target.transform;
        //         dollyCamera.LookAt = target.transform;
        //     }
        //     else
        //     {
        //         targetId = -1001;
        //         target = null;
        //         defaultCamera.LookAt = defaultStartTarget;
        //         dollyCamera.LookAt = dollyStartTarget;
        //     }
        //     if (Utilities.IsValid(manager.menu))
        //     {
        //         manager.menu.UpdateEditor();
        //     }
        //     animator.SetBool(PlayerTrackingHash, Utilities.IsValid(target));
        // }

        public void RenderPreview()
        {
            manager.previewCamera.targetTexture = localPreview;
            manager.previewCamera.transform.SetParent(localPreviewParent, false);
            manager.previewCamera.fieldOfView = FOVTracker.transform.localPosition.x;
        }

        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(Zoom))]
        public float _zoom = 0.2f;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Mathf.Clamp01(value);
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
                // animator.SetFloat("Zoom", Mathf.Sqrt(_zoom));
                // LerpZoom();
                SendCustomEventDelayedFrames(nameof(LerpZoom), 1);
                if (Utilities.IsValid(manager.menu) && manager.menu.EditorFanCam == this)
                {
                    manager.menu.zoomSlider.SetValueWithoutNotify(_zoom);
                }
            }
        }

        public void ZoomIn()
        {
            if (!IsOwnerLocal())
            {

                if (pickupControllerPickup.IsHeld())
                {
                    return;
                }
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            Zoom = 1.0f;
        }

        public void ZoomOut()
        {
            if (!IsOwnerLocal())
            {
                if (pickupControllerPickup.IsHeld())
                {
                    return;
                }
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            Zoom = 0f;
        }

        public void StopZoom()
        {
            if (!IsOwnerLocal())
            {
                if (pickupControllerPickup.IsHeld())
                {
                    return;
                }
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            Zoom = Mathf.Clamp01(AnimatorZoom + ConstAccelerationDistance(zoomSpeed, 0, Mathf.Sign(zoomSpeed) * -zoomAcceleration));
        }

        public float AnimatorZoom
        {
            get => Mathf.Pow(animator.GetFloat(ZoomHash), 2f);
            set
            {
                animator.SetFloat(ZoomHash, Mathf.Pow(Mathf.Clamp01(value), 0.5f));
            }
        }

        readonly float maxZoomSpeed = 0.3f;
        readonly float zoomAcceleration = 0.25f;

        float zoomSpeed = 0;

        int lastZoomLerp;
        public void LerpZoom()
        {
            if (lastZoomLerp == Time.renderedFrameCount)
            {
                return;
            }
            // Fixed version with proper handling of negative direction movement
            var startZoom = AnimatorZoom;
            var startZoomSpeed = zoomSpeed;
            var targetDistance = Zoom - startZoom;

            bool stop = false;
            bool coast = false;

            if (targetDistance > 0)
            {
                // Moving in positive direction
                // Calculate stopping distance for positive velocity
                var stoppingDist = ConstAccelerationDistance(zoomSpeed, 0, -zoomAcceleration);

                if (zoomSpeed * Time.deltaTime + stoppingDist < targetDistance)
                {
                    // Need to speed up - we won't reach target even if coasting then braking
                    zoomSpeed = Mathf.Min(maxZoomSpeed, zoomSpeed + zoomAcceleration * Time.deltaTime);
                }
                else if (stoppingDist >= targetDistance)
                {
                    // Need to slow down - would overshoot if we don't brake
                    zoomSpeed = Mathf.Max(0, zoomSpeed - zoomAcceleration * Time.deltaTime);
                    if (zoomSpeed <= 0)
                    {
                        stop = true;
                    }
                }
                else
                {
                    coast = true;
                }
            }
            else if (targetDistance < 0)
            {
                // Moving in negative direction
                // For negative movement, stopping distance is from negative speed to 0
                var stoppingDist = ConstAccelerationDistance(zoomSpeed, 0, zoomAcceleration); // Note: positive acceleration to stop from negative speed

                // stoppingDist will be negative when zoomSpeed is negative (which is what we want)
                if (zoomSpeed * Time.deltaTime + stoppingDist > targetDistance)
                {
                    // Need to speed up in negative direction (make more negative)
                    zoomSpeed = Mathf.Max(-maxZoomSpeed, zoomSpeed - zoomAcceleration * Time.deltaTime);
                }
                else if (stoppingDist <= targetDistance)
                {
                    // Need to slow down (brake from negative speed toward 0)
                    zoomSpeed = Mathf.Min(0, zoomSpeed + zoomAcceleration * Time.deltaTime);
                    if (zoomSpeed >= 0)
                    {
                        stop = true;
                    }
                }
                else
                {
                    coast = true;
                }
            }
            else
            {
                stop = true;
            }

            if (stop)
            {
                AnimatorZoom = Zoom;
                zoomSpeed = 0; // Make sure to reset speed when stopping
                return;
            }

            if (coast)
            {
                AnimatorZoom = startZoom + zoomSpeed * Time.deltaTime;
            }
            else
            {
                // Using kinematic equation: d = v₀t + 0.5at²
                // Distance traveled this frame with constant acceleration
                float accel = (zoomSpeed - startZoomSpeed) / Time.deltaTime; // actual acceleration this frame
                var accelerationDistance = startZoomSpeed * Time.deltaTime + 0.5f * accel * Time.deltaTime * Time.deltaTime;
                AnimatorZoom = startZoom + accelerationDistance;
            }
            lastZoomLerp = Time.renderedFrameCount;
            SendCustomEventDelayedFrames(nameof(LerpZoom), 1);
        }
        public float ConstAccelerationDistance(float startSpeed, float endSpeed, float acceleration)
        {
            return (Mathf.Pow(endSpeed, 2) - Mathf.Pow(startSpeed, 2)) / (2 * acceleration);
        }
    }
}
