
using Cinemachine;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using Algolia.Search.Models.Rules;


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
        private readonly int RecHash = Animator.StringToHash("rec");
        private readonly int EditHash = Animator.StringToHash("edit");
        private readonly int DollyHash = Animator.StringToHash("dolly");
        private readonly int ZoomHash = Animator.StringToHash("zoom");
        private readonly int OwnerHash = Animator.StringToHash("owner");
        public FanCamManager manager;
        public CinemachineVirtualCamera defaultCamera;
        public CinemachineVirtualCamera dollyCamera;
        public Animator animator;
        public SmartObjectSync pickupControllerPickup;
        public SmartObjectSync[] dollyTrackPickups;
        public FanCamTrackFollower dollyTrack;

        // [UdonSynced]
        [SerializeField]
        [FieldChangeCallback(nameof(Id))]
        int _id = 0;//0 is a null value
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }

                //Non player objects get positive Ids starting from 1
                // manager.AddFanCam(this);
            }
        }

        public bool Rec
        {
            get => manager.ActiveCam == Id;
            set
            {
                animator.SetBool(RecHash, value);
                UpdatePreview();
            }
        }

        public bool _edit = false;
        public bool Edit
        {
            get => _edit;
            set
            {
                _edit = value;
                animator.SetBool(EditHash, value && IsOwnerLocal());
                manager.UpdateEdit(this);
                if (!value && IsOwnerLocal())
                {
                    StopZoom();
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
                }
                else
                {
                    foreach (var pickup in dollyTrackPickups)
                    {
                        pickup.pickup.Drop();
                    }
                }
                animator.SetBool(DollyHash, value);
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }

        public void ToggleDolly()
        {
            Dolly = !Dolly;
        }
        // [UdonSynced]
        // [FieldChangeCallback(nameof(Speed))]
        // public float _speed = 1f;
        // public float Speed
        // {
        //     get => _speed;
        //     set
        //     {
        //         _speed = value;
        //         dollyTrack._speed = value;
        //         if (IsOwnerLocal())
        //         {
        //             RequestSerialization();
        //         }
        //     }
        // }

        VRCPlayerApi owner;
        public void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
            animator.SetBool(OwnerHash, IsOwnerLocal());
            Id = Id;
            Rec = Rec;
            Edit = Edit;
            Dolly = Dolly;
        }

        public VRCPlayerApi Owner
        {
            get => owner;
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            owner = player;
            animator.SetBool(OwnerHash, IsOwnerLocal());
            Edit = Edit && IsOwnerLocal();
            if (IsOwnerLocal())
            {
                StopZoom();
            }
        }

        public bool IsOwnerLocal()
        {
            return Utilities.IsValid(owner) && owner.isLocal;
        }

        public void EnableCam()
        {
            Networking.SetOwner(Networking.LocalPlayer, manager.gameObject);
            manager.ActiveCam = Id;
        }

        public void ToggleTarget()
        {

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

        bool held = false;
        public bool Held
        {
            get => held;
        }
        public void OnPickupListener(FanCamPickupListener listener)
        {
            // localPreviewCamera.enabled = true;
            held = true;
            // manager.AddToPreviewList(this);
            manager.HeldFanCam = this;
            UpdatePreview();
        }

        public void OnDropListener(FanCamPickupListener listener)
        {
            // localPreviewCamera.enabled = false;
            held = false;
            // manager.RemoveFromPreviewList(this);
            manager.HeldFanCam = null;
            UpdatePreview();
        }

        public void UpdatePreview()
        {
            if (!Utilities.IsValid(propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();
                previewMesh.GetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            if (Rec)
            {
                propertyBlock.SetTexture("_MainTex", manager.fullResRenderTexture);
                previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            else if (held)
            {
                propertyBlock.SetTexture("_MainTex", localPreview);
                previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
            else
            {
                propertyBlock.SetTexture("_MainTex", placeholderTexture);
                previewMesh.SetPropertyBlock(propertyBlock, previewMeshMaterialSlot);
            }
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif 

        FanCamPlayerTarget target;
        public FanCamPlayerTarget Target
        {
            get => target;
        }

        Transform defaultStartTarget;
        Transform dollyStartTarget;
        [UdonSynced]
        [FieldChangeCallback(nameof(TargetPlayerId))]
        int targetId = -1001;
        public int TargetPlayerId
        {
            get => targetId;
            set
            {
                if (targetId < 0)
                {
                    defaultStartTarget = defaultCamera.LookAt;
                    dollyStartTarget = dollyCamera.LookAt;
                }
                targetId = value;
                if (IsOwnerLocal())
                {
                    RequestSerialization();
                }
                if (manager.PlayerTargets.TryGetValue(value, TokenType.Reference, out DataToken token))
                {
                    target = (FanCamPlayerTarget)token.Reference;
                    defaultCamera.LookAt = target.transform;
                    dollyCamera.LookAt = target.transform;
                }
                else
                {
                    target = null;
                    defaultCamera.LookAt = defaultStartTarget;
                    dollyCamera.LookAt = dollyStartTarget;
                }
            }
        }

        public void RenderPreview()
        {
            manager.previewCamera.targetTexture = localPreview;
            manager.previewCamera.transform.SetParent(localPreviewParent, false);
        }

        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(Zoom))]
        public float _zoom = 0.2f;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Mathf.Clamp01(value);
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
                // animator.SetFloat("Zoom", Mathf.Sqrt(_zoom));
                // LerpZoom();
                SendCustomEventDelayedFrames(nameof(LerpZoom), 1);
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
                animator.SetFloat(ZoomHash, Mathf.Pow(value, 0.5f));
            }
        }

        readonly float maxZoomSpeed = 0.15f;
        readonly float zoomAcceleration = 0.2f;

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
                // // targetDistance == 0, we're at the target
                // if (Mathf.Abs(zoomSpeed) < zoomAcceleration * Time.deltaTime)
                // {
                // }
                // else if (zoomSpeed > 0)
                // {
                //     zoomSpeed = Mathf.Max(0, zoomSpeed - zoomAcceleration * Time.deltaTime);
                // }
                // else
                // {
                //     zoomSpeed = Mathf.Min(0, zoomSpeed + zoomAcceleration * Time.deltaTime);
                // }
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
